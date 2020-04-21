using GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStateManager : MonoBehaviour
{
    /// <summary>
    /// Procedural Level Map Model
    /// </summary>
    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;
    [SerializeField] private int smoothStep = 5;
    [SerializeField] private int borderSize = 1;

    [Range(0.01f, 10)]
    [SerializeField] private float mapCellScale = 1.0f;
    [Range(1, 10)]
    [SerializeField] private int cellsPerPathWidth = 3;
    [Range(1, 10)]
    [SerializeField] private int cellsPerWallWidth = 1;
    [Range(0, 100)]
    [SerializeField] private int randomFillPercent;

    [Tooltip("Floor Tile Prefab")]
    [SerializeField] private LevelTile levelTilePrefab;
    [Tooltip("Path Tile Prefab")]
    [SerializeField] private PathTile pathTilePrefab;
    [Tooltip("Wall Tile Prefab")]
    [SerializeField] private WallTile[] wallTilePrefab;
    [Tooltip("Door Tile Prefab")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float doorProbability = 0.1f;
    [Tooltip("Room Settings")]
    [SerializeField] private MazeRoomSettings[] roomSettings;
    [SerializeField] private DoorTile doorTilePrefab;
    [Tooltip("Start Tile Prefab")]
    [SerializeField] private SpawnTile spawnStartPrefab;
    [Tooltip("End Tile Prefab")]
    [SerializeField] private SpawnTile spawnEndPrefab;
    [Tooltip("Checkpoints Tile Prefab")]
    [SerializeField] private SpawnTile spawnCheckpointPrefab;
    [Tooltip("Ghost Tile Prefab")]
    [SerializeField] private SpawnTile spawnGhostPrefab;
    [Tooltip("Checkpoints Tile Prefab")]
    [SerializeField] private SpawnTile spawnPickUpPrefab;
    

    [Tooltip("Speed of Path")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float speed = 0.0003f;
    Vector2 currentPositonInMap = new Vector2();
    [Tooltip("Generate Room on Map or just view the map")]
    [SerializeField] private bool spawnRoom = true;
    //[Range(1, 100)]
    [SerializeField] private Vector2Int mapDimensions;

    private LevelTile[,] map;
    private List<MazeRoom> rooms = new List<MazeRoom>();

    private void Start()
    {
        //GenerateMap();
    }

    public void GenerateMap()
    {
        ResetMap();
        RandomFillMap();
        // smooth borders
        for (int i = 0; i < smoothStep; i++)
        {
            SmoothMap();
        }
        // walk te map and create level
        Walk();
    }
    public void ResetMap()
    {
        StopAllCoroutines();
        //TODO clear map objects before recreating map
        // generate base map
        map = new LevelTile[mapDimensions.x, mapDimensions.y];
        // Create clean TileMap
        for (int x = 0; x < mapDimensions.x; x++)
        {
            // Debug.Log("Drawing Maze Row" + x);
            for (int y = 0; y < mapDimensions.y; y++)
            {
                CreateCell(new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// Walk on Tile Map functions
    /// </summary>
    private void Walk()
    {
        // scan for a floor tile
        //StartCoroutine(scannRowWalk());
        
        // walk from floor tile
        StartCoroutine(randomWalk());
    }

    private IEnumerator scannRowWalk()
    {
        Vector2Int mapIndex = new Vector2Int((int)currentPositonInMap.x % mapDimensions.x, (int)currentPositonInMap.y % mapDimensions.y);
        while (IsTileInMap(mapIndex) && mapIndex.x <= mapDimensions.x - 1 && mapIndex.y <= mapDimensions.y - 1)
        { 
            // Every half a sec. create a tile
            yield return new WaitForSeconds(0.01f);
            mapIndex = new Vector2Int((int)currentPositonInMap.x % mapDimensions.x, (int)currentPositonInMap.y % mapDimensions.y);
            // Update the current position on the spline based on the speed and spline segment count
            float speedSpline = (float)mapDimensions.x * speed;
            currentPositonInMap.x += speedSpline + Time.deltaTime;
            if (currentPositonInMap.x > mapDimensions.x - 1)
            {
                currentPositonInMap.x = 0.0f;
                currentPositonInMap.y += 1.0f;
                if (currentPositonInMap.y >= mapDimensions.y)
                {
                    currentPositonInMap.y = 0.0f;
                }
            }
        }
    }

    public bool CreateInteractables()
    {
        int spawnCountTotal = 1;
        spawmAgents(spawnStartPrefab, spawnCountTotal);
        spawnCountTotal = 1;
        spawmAgents(spawnEndPrefab, spawnCountTotal);
        spawnCountTotal = 2;
        spawmAgents(spawnCheckpointPrefab, spawnCountTotal);
        spawnCountTotal = 3;
        spawmAgents(spawnPickUpPrefab, spawnCountTotal);
        spawnCountTotal = 5;
        spawmAgents(spawnGhostPrefab, spawnCountTotal);
        return true;
    }
    private void spawmAgents(SpawnTile spawnTilePrefab, int spawnCountTotal)
    {
        int spawnCount = 0;
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        float spawnProbability = (float)pseudoRandom.Next(0, 10);
        Vector2Int mapIndex = new Vector2Int(Random.Range(0, mapDimensions.x - 1), Random.Range(0, mapDimensions.y - 1));
        while (spawnCount <= spawnCountTotal)
        {
            if(map[mapIndex.x, mapIndex.y].tileState == UnitType.Path)
            {
                SpawnTile spawnPoint = Instantiate(spawnTilePrefab) as SpawnTile;
                spawnPoint.Initialize(map[mapIndex.x, mapIndex.y], null, WalkDirections.RandomValue);
                spawnCount++;
            }
            mapIndex = new Vector2Int(Random.Range(0, mapDimensions.x - 1), Random.Range(0, mapDimensions.y - 1));
        }
    }
    
    private IEnumerator randomWalk()
    {
        WaitForSeconds delay = new WaitForSeconds(0.01f);
        map = new LevelTile[mapDimensions.x, mapDimensions.y];
        List<LevelTile> activeCells = new List<LevelTile>();
        DoFirstGenerationStep(activeCells);
        while (activeCells.Count > 0)
        {
            yield return delay;
            DoNextGenerationStep(activeCells);
        }
        yield return CreateInteractables();
    }

    private void DoFirstGenerationStep(List<LevelTile> activeCells)
    {
        LevelTile newCell = CreateCell(RandomCoordinates);
        newCell.Initialize(CreateRoom(-1));
        activeCells.Add(newCell);
        //activeCells.Add(CreateCell(RandomCoordinates));
    }

    private void DoNextGenerationStep(List<LevelTile> activeCells)
    {
        int currentIndex = activeCells.Count - 1;
        LevelTile currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized)
        {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        WalkDirection direction = currentCell.RandomUninitializedDirection;
        Vector2Int coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates))
        {
            LevelTile neighbor = GetCell(coordinates);
            if (neighbor == null)
            {
                neighbor = CreateCell(coordinates);
                CreatePath(currentCell, neighbor, direction);
                activeCells.Add(neighbor);
            }
            else if (currentCell.room.settingsIndex == neighbor.room.settingsIndex)
            {
                CreatePassageInSameRoom(currentCell, neighbor, direction);
            }
            else
            {
                CreateWall(currentCell, neighbor, direction);
            }
        }
        else
        {
            CreateWall(currentCell, null, direction);
        }
    }

    private LevelTile CreateCell(Vector2Int coordinates)
    {
        LevelTile newCell = Instantiate(levelTilePrefab) as LevelTile;
        newCell.tileState = UnitType.Floor;
        map[coordinates.x, coordinates.y] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.y;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector3(coordinates.x - mapDimensions.x * 0.5f + 0.5f, 0f, coordinates.y - mapDimensions.y * 0.5f + 0.5f);
        return newCell;
    }
    private void CreatePath(LevelTile cell, LevelTile otherCell, WalkDirection direction)
    {
        PathTile prefab = Random.value < doorProbability ? doorTilePrefab : pathTilePrefab;
        cell.tileState = UnitType.Path;
        PathTile passage = Instantiate(prefab) as PathTile;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(prefab) as PathTile;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (passage is DoorTile)
        {
            otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
        }
        else
        {
            otherCell.Initialize(cell.room);
        }
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall(LevelTile cell, LevelTile otherCell, WalkDirection direction)
    {
        cell.tileState = UnitType.Wall;
        WallTile wall = Instantiate(wallTilePrefab[Random.Range(0, wallTilePrefab.Length)]) as WallTile;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null)
        {
            wall = Instantiate(wallTilePrefab[Random.Range(0, wallTilePrefab.Length)]) as WallTile;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
    }

    private MazeRoom CreateRoom(int indexToExclude)
    {
        MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
        newRoom.settingsIndex = Random.Range(0, roomSettings.Length);
        if (newRoom.settingsIndex == indexToExclude)
        {
            newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
        }
        newRoom.settings = roomSettings[newRoom.settingsIndex];
        rooms.Add(newRoom);
        return newRoom;
    }

    private void CreatePassageInSameRoom(LevelTile cell, LevelTile otherCell, WalkDirection direction)
    {
        PathTile passage = Instantiate(pathTilePrefab) as PathTile;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(pathTilePrefab) as PathTile;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (cell.room != otherCell.room)
        {
            MazeRoom roomToAssimilate = otherCell.room;
            cell.room.Assimilate(roomToAssimilate);
            rooms.Remove(roomToAssimilate);
            Destroy(roomToAssimilate);
        }
    }

    public Vector2Int RandomCoordinates
    {
        get
        {
            return new Vector2Int(Random.Range(0, mapDimensions.x), Random.Range(0, mapDimensions.y));
        }
    }

    public bool ContainsCoordinates(Vector2Int coordinate)
    {
        return coordinate.x >= 0 && coordinate.x < mapDimensions.x && coordinate.y >= 0 && coordinate.y < mapDimensions.y;
    }

    public LevelTile GetCell(Vector2Int coordinates)
    {
        return map[coordinates.x, coordinates.y];
    }

    /// <summary>
    /// Fill Map functions
    /// </summary>

    private void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        for (int x = 0; x < mapDimensions.x; x++)
        {
            for (int y = 0; y < mapDimensions.y; y++)
            {
                WalkDirection direction = WalkDirection.North;
                //Map Edge Walls
                if (x == 0 || x == mapDimensions.x - 1 || y == 0 || y == mapDimensions.y - 1)
                {
                    map[x, y].tileState = UnitType.Wall;
                    if (x == 0)
                        direction = WalkDirection.East;
                    if (y == 0)
                        direction = WalkDirection.South;
                    if (x == mapDimensions.x - 1)
                        direction = WalkDirection.West;
                    if (y == mapDimensions.y - 1)
                        direction = WalkDirection.North;
                }
                else
                {
                    map[x, y].tileState = UnitType.Floor;
                    // Randomise Tile to wall or floor
                    map[x, y].tileState = (pseudoRandom.Next(0, 100) < randomFillPercent) ? UnitType.Wall : UnitType.Floor;
                }
                if(map[x, y].tileState == UnitType.Wall)
                {
                    CreateWall(map[x, y], null, direction);
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < mapDimensions.x; x++)
        {
            for (int y = 0; y < mapDimensions.y; y++)
            {
                int neighbourWallTiles = GetSurroundingAgentCount(new Vector2Int(x,y), UnitType.Wall);

                if (neighbourWallTiles > 4)
                    map[x, y].tileState = UnitType.Wall;
                else if (neighbourWallTiles < 4)
                    map[x, y].tileState = UnitType.Floor;

            }
        }
    }

    int GetSurroundingAgentCount(Vector2Int index, UnitType agentType)
    {
        int agentCount = 0;
        for (int neighbourX = index.x - 1; neighbourX <= index.x + 1; neighbourX++)
        {
            for (int neighbourY = index.y - 1; neighbourY <= index.y + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < mapDimensions.x && neighbourY >= 0 && neighbourY < mapDimensions.y)
                {
                    // check tiles surounding give tile 
                    if (neighbourX != index.x || neighbourY != index.y)
                    {
                        // if is a wall tile
                        if (map[neighbourX, neighbourY].tileState == agentType) agentCount++;// += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    // if edge of the map
                    agentCount++;
                }
            }
        }

        return agentCount;
    }
    
    /// <summary>
    /// Get random walk direction
    /// </summary>
    /// <returns></returns>
    private WalkDirection RandomDirection()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        return (WalkDirection)pseudoRandom.Next(0, 4);
    }

    /// <summary>
    /// Get Random Walk Direction Index
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private static Vector2Int RandomDirectionIndex(WalkDirection direction)
    {
        Vector2Int[] directionIndex =
        {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1,0),
        };
        return directionIndex[(int)direction];
    }

    /// <summary>
    /// Get a random time from the TileMap
    /// </summary>
    private Vector2Int RandomTileIndex
    {
        get
        {
            if (useRandomSeed)
            {
                seed = Time.time.ToString();
            }
            System.Random pseudoRandom = new System.Random(seed.GetHashCode());
            return new Vector2Int(pseudoRandom.Next(0, mapDimensions.x), pseudoRandom.Next(0, mapDimensions.y));
        }
    }

    /// <summary>
    /// Check if TileMap Index is in range
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool IsTileInMap(Vector2Int index)
    {
        return (index.x >= 0 && index.x < mapDimensions.x && index.y >= 0 && index.y < mapDimensions.y);
    }

    /// <summary>
    /// Static function to get Opposit Direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static WalkDirection GetOpposite(WalkDirection direction)
    {
        return oppositeWalkDirection[(int)direction];
    }

    private static WalkDirection[] oppositeWalkDirection =
    {
        WalkDirection.South,
        WalkDirection.West,
        WalkDirection.North,
        WalkDirection.East
    };

    public static Quaternion GetWalkDirectionToWorldOrientation(WalkDirection direction)
    {
        return walkDirectionToWorldOrientation[(int)direction];
    }

    private static Quaternion[] walkDirectionToWorldOrientation =
    {
       Quaternion.identity,
       Quaternion.Euler(0f, 90f, 0f),
       Quaternion.Euler(0f, 180f, 0f),
       Quaternion.Euler(0f, 270f, 0f),
    };

    /// <summary>
    /// Draw TileMap Debuger 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (map != null)
        {
            Vector2 mapSmoothIndex = new Vector2(currentPositonInMap.x%(mapDimensions.x), currentPositonInMap.y%mapDimensions.y);
            Vector2Int mapIndex = new Vector2Int((int)currentPositonInMap.x%mapDimensions.x, (int)currentPositonInMap.y%mapDimensions.y);
            Vector3 position = new Vector3(-mapDimensions.y / 2 + mapIndex.x, 0, -mapDimensions.y / 2 + mapIndex.y);
            position *= mapCellScale;

            Gizmos.color = Color.yellow;
            LevelTile currentTile = map[mapIndex.x, mapIndex.y];
            if (currentTile.IsFullyInitialized)
            {
                Gizmos.color = Color.red;
            }
            // Debug.Log("Current Point - mapSmoothIndex: " + mapSmoothIndex + "mapIndex: " + mapIndex + "position: " + position);
            
            Gizmos.DrawCube(position, Vector3.one * mapCellScale);
            // smooth step from one box to the next
            position = new Vector3(-mapDimensions.x / 2 + mapSmoothIndex.x, 0, -mapDimensions.y / 2 + mapSmoothIndex.y);
            position *= mapCellScale;
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(position, Vector3.one * mapCellScale);

            // Draw TileMap
            for (int x = 0; x < mapDimensions.x; x++)
            {
                // Debug.Log("Drawing Maze Row" + x);
                for (int y = 0; y < mapDimensions.y; y++)
                {
                    position = new Vector3(x - mapDimensions.x / 2, 0,y - mapDimensions.y / 2);
                    position *= mapCellScale;

                    if (map[x, y].IsFullyInitialized)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(position, Vector3.one * mapCellScale);
                    }
                    else if(map[x, y].tileState == UnitType.Start)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawCube(position, Vector3.one * mapCellScale);
                    }
                    else if (map[x, y].tileState == UnitType.Path)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(position, Vector3.one * mapCellScale);
                    }
                    else if (map[x, y].tileState == UnitType.Wall)
                    {
                        Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.25f);
                        Gizmos.DrawCube(position, Vector3.one * mapCellScale);
                    }
                    else if (map[x, y].tileState == UnitType.Floor)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(position, Vector3.one * mapCellScale);
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(position, Vector3.one * mapCellScale);
                    }

                }
            }
        }

    }

}




