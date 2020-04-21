using GameLogic;
using System;
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
    //[Range(1, 100)]
    [SerializeField] private Vector2Int mapDimensions;
    [Range(0, 100)]
    [SerializeField] private int randomFillPercent;

    [Tooltip("Path Tile Prefab")]
    [SerializeField] private PathTile pathTilePrefab;
    [Tooltip("Path Tile Prefab")]
    [SerializeField] private WallTile wallTilePrefab;

    [Tooltip("Speed of Path")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float speed = 0.0003f;
    Vector2 currentPositonInMap = new Vector2();
    [Tooltip("Generate Room on Map or just view the map")]
    [SerializeField] private bool spawnRoom = true;

    UnitType[,] map;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        //StopAllCoroutines();
        // generate base map
        map = new UnitType[mapDimensions.x, mapDimensions.y];
        RandomFillMap();
        // smooth borders
        for (int i = 0; i < smoothStep; i++)
        {
            SmoothMap();
        }
        // walk te map and create level
        Walk();
    }
    public void ClearWalk()
    {
        StopAllCoroutines();
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
        while (IsTileInMap(mapIndex))
        { 
            // Every half a sec. create a tile
            yield return new WaitForSeconds(0.1f);
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

    private IEnumerator randomWalk()
    {
        // Start at random floor tile
        while (map[(int)currentPositonInMap.x, (int)currentPositonInMap.y] != UnitType.Floor)
            currentPositonInMap = RandomTileIndex;
        Vector2Int pathStartIndex = new Vector2Int((int)currentPositonInMap.x, (int)currentPositonInMap.y);

        // Initialize the Backtracking Stack with start tile
        List<Vector2Int> activeTileLIFOStack = new List<Vector2Int>();
        activeTileLIFOStack.Add(pathStartIndex);

        while (activeTileLIFOStack.Count>0) // check if we can backtrack
        {
            // Every 0.5 sec. create a tile
            yield return new WaitForSeconds(0.1f);
            int LIFOIndex = activeTileLIFOStack.Count - 1;
            WalkDirection direction = RandomDirection();
            Vector2Int directionIndex = RandomDirectionIndex(direction);
            currentPositonInMap = activeTileLIFOStack[LIFOIndex];
            Vector2Int currentTileIndex = new Vector2Int((int)currentPositonInMap.x, (int)currentPositonInMap.y);
            Vector2Int neighbourTileIndex = currentTileIndex + directionIndex;

            // Goto Neighboring tile
            if (IsTileInMap(neighbourTileIndex)) // check if tile is in map range
            {
                if (map[neighbourTileIndex.x, neighbourTileIndex.y] == UnitType.Floor)
                {
                    // move in direction to new tile and create path
                    if (spawnRoom) CreatePath(currentTileIndex, neighbourTileIndex, direction);

                    activeTileLIFOStack.Add(neighbourTileIndex);
                    map[neighbourTileIndex.x, neighbourTileIndex.y] = UnitType.Path;
                }
                else // Backtrack if hit a dead end
                {
                    //create wall and backtrack to previous tile
                    if (spawnRoom) CreateWall(currentTileIndex, neighbourTileIndex, direction);

                    activeTileLIFOStack.Remove(neighbourTileIndex);
                    map[neighbourTileIndex.x, neighbourTileIndex.y] = UnitType.Wall;
                }
            }
            else
            {
                //create wall as it is the edge of the map (send a negative index to represnet outside the map)
                if(spawnRoom)CreateWall(currentTileIndex, new Vector2Int(-1,-1), direction);
                activeTileLIFOStack.Remove(neighbourTileIndex);
                map[neighbourTileIndex.x, neighbourTileIndex.y] = UnitType.Wall;
            }

            // Update start tile state
            if (map[pathStartIndex.x, pathStartIndex.y] != UnitType.Start)
            {
                map[pathStartIndex.x, pathStartIndex.y] = UnitType.Start;
            }
        }

    }

    private void CreatePath(Vector2Int currentTileIndex, Vector2Int neighbourTileIndex, WalkDirection direction)
    {
        PathTile passage = Instantiate(pathTilePrefab) as PathTile;
        passage.Initialize(currentTileIndex, neighbourTileIndex, direction);
        GetWorldSpaceTransformOnMap(currentTileIndex, direction, passage.gameObject.transform);
        passage = Instantiate(pathTilePrefab) as PathTile;
        passage.Initialize(neighbourTileIndex, currentTileIndex, GetOpposite(direction));
        GetWorldSpaceTransformOnMap(currentTileIndex, direction, passage.gameObject.transform);
    }

    private void GetWorldSpaceTransformOnMap(Vector2Int currentTileIndex, WalkDirection direction, Transform tileTransform)
    {
        Vector3 position = new Vector3(currentTileIndex.x - mapDimensions.x / 2, 0, currentTileIndex.y - mapDimensions.y / 2);
        // TODO update objects when smap scale is changed
        position *= mapCellScale;
        tileTransform.parent = this.transform;
        tileTransform.position = position;
        //tileTransform.localPosition = position;
        tileTransform.localRotation = GetWalkDirectionToWorldOrientation(direction);
    }

    private void CreateWall(Vector2Int currentTileIndex, Vector2Int neighbourTileIndex, WalkDirection direction)
    {
        WallTile wall = Instantiate(wallTilePrefab) as WallTile;
        wall.Initialize(currentTileIndex, neighbourTileIndex, direction);
        GetWorldSpaceTransformOnMap(currentTileIndex, direction, wall.gameObject.transform);
        if (neighbourTileIndex.x >= 0 || neighbourTileIndex.y >= 0)
        { 
            wall = Instantiate(wallTilePrefab) as WallTile;
            wall.Initialize(neighbourTileIndex, currentTileIndex, GetOpposite(direction));
            GetWorldSpaceTransformOnMap(currentTileIndex, direction, wall.gameObject.transform);
        }
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
                //Map Edge Walls
                if (x == 0 || x == mapDimensions.x - 1 || y == 0 || y == mapDimensions.y - 1)
                {
                    map[x, y] = UnitType.Wall;
                }
                else
                {
                    map[x, y] = UnitType.Floor;
                    // Randomise Tile to wall or floor
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? UnitType.Wall : UnitType.Floor;
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
                    map[x, y] = UnitType.Wall;
                else if (neighbourWallTiles < 4)
                    map[x, y] = UnitType.Floor;

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
                        if (map[neighbourX, neighbourY] == agentType) agentCount++;// += map[neighbourX, neighbourY];
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

            // Debug.Log("Current Point - mapSmoothIndex: " + mapSmoothIndex + "mapIndex: " + mapIndex + "position: " + position);
            // check if near a wall
            if (GetSurroundingAgentCount(mapIndex, UnitType.Wall)>0)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.yellow;
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

                    if (map[x, y] == UnitType.Start)
                    {
                        Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.25f);
                        Gizmos.DrawCube(position, Vector3.one * mapCellScale);
                    }
                    else if (map[x, y] == UnitType.Path)
                    {
                        Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.75f);
                        Gizmos.DrawCube(position, Vector3.one * mapCellScale);
                    }
                    else if (map[x, y] == UnitType.Wall)
                    {
                        Gizmos.color = Color.Lerp(Color.green, Color.yellow, 0.25f);
                        Gizmos.DrawCube(position, Vector3.one * mapCellScale);
                    }
                    else if (map[x, y] == UnitType.Floor)
                    {
                        Gizmos.color = Color.Lerp(Color.green, Color.yellow, 0.75f);
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

public enum WalkDirection
{
    North,
    East,
    South,
    West
}

public class Tile : MonoBehaviour
{

    public UnitType tileState;
    public EdgeTile[] edges = new EdgeTile[System.Enum.GetValues(typeof(WalkDirection)).Length];

    public EdgeTile GetEdge(WalkDirection direction)
    {
        return edges[(int)direction];
    }
    public void SetEdge(WalkDirection direction, EdgeTile edge)
    {
        edges[(int)direction] = edge;
    }

}



