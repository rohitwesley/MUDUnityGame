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
    [SerializeField] private MazeCell MazeCellPrefab;
    [Tooltip("Path Tile Prefab")]
    [SerializeField] private MazePassage MazePassagePrefab;
    [Tooltip("Door Tile Prefab")]
    [SerializeField] private MazeDoor MazeDoorPrefab;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float doorProbability = 0.1f;
    [Tooltip("Wall Tile Prefab")]
    [SerializeField] private MazeWall[] MazeWallPrefab;
    [Tooltip("Room Settings")]
    [SerializeField] private MazeRoomSettings[] roomSettings;
    

    [Tooltip("Speed of Path")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float speed = 0.0003f;
    Vector2 currentPositonInMap = new Vector2();
    [Tooltip("Generate Room on Map or just view the map")]
    [SerializeField] private bool spawnRoom = true;
    //[Range(1, 100)]
    [SerializeField] private IntVector2 mapDimensions;

    private MazeCell[,] map;
    private List<MazeRoom> rooms = new List<MazeRoom>();

    private void Start()
    {
        GenerateMap();
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
        map = new MazeCell[mapDimensions.x, mapDimensions.z];
        // Create clean TileMap
        for (int x = 0; x < mapDimensions.x; x++)
        {
            // Debug.Log("Drawing Maze Row" + x);
            for (int y = 0; y < mapDimensions.z; y++)
            {
                CreateCell(RandomCoordinates);
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
        Vector2Int mapIndex = new Vector2Int((int)currentPositonInMap.x % mapDimensions.x, (int)currentPositonInMap.y % mapDimensions.z);
        while (IsTileInMap(mapIndex) && mapIndex.x <= mapDimensions.x - 1 && mapIndex.y <= mapDimensions.z - 1)
        { 
            // Every half a sec. create a tile
            yield return new WaitForSeconds(0.01f);
            mapIndex = new Vector2Int((int)currentPositonInMap.x % mapDimensions.x, (int)currentPositonInMap.y % mapDimensions.z);
            // Update the current position on the spline based on the speed and spline segment count
            float speedSpline = (float)mapDimensions.x * speed;
            currentPositonInMap.x += speedSpline + Time.deltaTime;
            if (currentPositonInMap.x > mapDimensions.x - 1)
            {
                currentPositonInMap.x = 0.0f;
                currentPositonInMap.y += 1.0f;
                if (currentPositonInMap.y >= mapDimensions.z)
                {
                    currentPositonInMap.y = 0.0f;
                }
            }
        }
    }

    private IEnumerator randomWalk()
    {
        WaitForSeconds delay = new WaitForSeconds(0.01f);
        map = new MazeCell[mapDimensions.x, mapDimensions.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGenerationStep(activeCells);
        while (activeCells.Count > 0)
        {
            yield return delay;
            DoNextGenerationStep(activeCells);
        }
    }

    private void DoFirstGenerationStep(List<MazeCell> activeCells)
    {
        MazeCell newCell = CreateCell(RandomCoordinates);
        newCell.Initialize(CreateRoom(-1));
        activeCells.Add(newCell);
    }

    private void DoNextGenerationStep(List<MazeCell> activeCells)
    {
        int currentIndex = activeCells.Count - 1;
        MazeCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized)
        {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        MazeDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates))
        {
            MazeCell neighbor = GetCell(coordinates);
            if (neighbor == null)
            {
                neighbor = CreateCell(coordinates);
                CreatePassage(currentCell, neighbor, direction);
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

    private MazeCell CreateCell(IntVector2 coordinates)
    {
        MazeCell newCell = Instantiate(MazeCellPrefab) as MazeCell;
        map[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector3(coordinates.x - mapDimensions.x * 0.5f + 0.5f, 0f, coordinates.z - mapDimensions.z * 0.5f + 0.5f);
        return newCell;
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazePassage prefab = Random.value < doorProbability ? MazeDoorPrefab : MazePassagePrefab;
        MazePassage passage = Instantiate(prefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(prefab) as MazePassage;
        if (passage is MazeDoor)
        {
            otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
        }
        else
        {
            otherCell.Initialize(cell.room);
        }
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreatePassageInSameRoom(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazePassage passage = Instantiate(MazePassagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(MazePassagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (cell.room != otherCell.room)
        {
            MazeRoom roomToAssimilate = otherCell.room;
            cell.room.Assimilate(roomToAssimilate);
            rooms.Remove(roomToAssimilate);
            Destroy(roomToAssimilate);
        }
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazeWall wall = Instantiate(MazeWallPrefab[Random.Range(0, MazeWallPrefab.Length)]) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null)
        {
            wall = Instantiate(MazeWallPrefab[Random.Range(0, MazeWallPrefab.Length)]) as MazeWall;
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

    public IntVector2 RandomCoordinates
    {
        get
        {
            return new IntVector2(Random.Range(0, mapDimensions.x), Random.Range(0, mapDimensions.z));
        }
    }

    public bool ContainsCoordinates(IntVector2 coordinate)
    {
        return coordinate.x >= 0 && coordinate.x < mapDimensions.x && coordinate.z >= 0 && coordinate.z < mapDimensions.z;
    }

    public MazeCell GetCell(IntVector2 coordinates)
    {
        return map[coordinates.x, coordinates.z];
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
            for (int y = 0; y < mapDimensions.z; y++)
            {
                MazeDirection direction = MazeDirection.North;
                //Map Edge Walls
                if (x == 0 || x == mapDimensions.x - 1 || y == 0 || y == mapDimensions.z - 1)
                {
                    map[x, y].tileState = UnitType.Wall;
                    if (x == 0)
                        direction = MazeDirection.East;
                    if (y == 0)
                        direction = MazeDirection.South;
                    if (x == mapDimensions.x - 1)
                        direction = MazeDirection.West;
                    if (y == mapDimensions.z - 1)
                        direction = MazeDirection.North;
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
            for (int y = 0; y < mapDimensions.z; y++)
            {
                int neighbourMazeWalls = GetSurroundingAgentCount(new Vector2Int(x,y), UnitType.Wall);

                if (neighbourMazeWalls > 4)
                    map[x, y].tileState = UnitType.Wall;
                else if (neighbourMazeWalls < 4)
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
                if (neighbourX >= 0 && neighbourX < mapDimensions.x && neighbourY >= 0 && neighbourY < mapDimensions.z)
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
    private MazeDirection RandomDirection()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        return (MazeDirection)pseudoRandom.Next(0, 4);
    }

    /// <summary>
    /// Get Random Walk Direction Index
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private static Vector2Int RandomDirectionIndex(MazeDirection direction)
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
            return new Vector2Int(pseudoRandom.Next(0, mapDimensions.x), pseudoRandom.Next(0, mapDimensions.z));
        }
    }

    /// <summary>
    /// Check if TileMap Index is in range
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool IsTileInMap(Vector2Int index)
    {
        return (index.x >= 0 && index.x < mapDimensions.x && index.y >= 0 && index.y < mapDimensions.z);
    }

    /// <summary>
    /// Static function to get Opposit Direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static MazeDirection GetOpposite(MazeDirection direction)
    {
        return oppositeMazeDirection[(int)direction];
    }

    private static MazeDirection[] oppositeMazeDirection =
    {
        MazeDirection.South,
        MazeDirection.West,
        MazeDirection.North,
        MazeDirection.East
    };

    public static Quaternion GetMazeDirectionToWorldOrientation(MazeDirection direction)
    {
        return MazeDirectionToWorldOrientation[(int)direction];
    }

    private static Quaternion[] MazeDirectionToWorldOrientation =
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
            Vector2 mapSmoothIndex = new Vector2(currentPositonInMap.x%(mapDimensions.x), currentPositonInMap.y%mapDimensions.z);
            Vector2Int mapIndex = new Vector2Int((int)currentPositonInMap.x%mapDimensions.x, (int)currentPositonInMap.y%mapDimensions.z);
            Vector3 position = new Vector3(-mapDimensions.z / 2 + mapIndex.x, 0, -mapDimensions.z / 2 + mapIndex.y);
            position *= mapCellScale;

            Gizmos.color = Color.yellow;
            MazeCell currentTile = map[mapIndex.x, mapIndex.y];
            if (currentTile.IsFullyInitialized)
            {
                Gizmos.color = Color.red;
            }
            // Debug.Log("Current Point - mapSmoothIndex: " + mapSmoothIndex + "mapIndex: " + mapIndex + "position: " + position);
            
            Gizmos.DrawCube(position, Vector3.one * mapCellScale);
            // smooth step from one box to the next
            position = new Vector3(-mapDimensions.x / 2 + mapSmoothIndex.x, 0, -mapDimensions.z / 2 + mapSmoothIndex.y);
            position *= mapCellScale;
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(position, Vector3.one * mapCellScale);

            // Draw TileMap
            for (int x = 0; x < mapDimensions.x; x++)
            {
                // Debug.Log("Drawing Maze Row" + x);
                for (int y = 0; y < mapDimensions.z; y++)
                {
                    position = new Vector3(x - mapDimensions.x / 2, 0,y - mapDimensions.z / 2);
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




