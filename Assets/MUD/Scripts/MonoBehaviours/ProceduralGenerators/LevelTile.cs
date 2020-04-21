using GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTile : MonoBehaviour
{
    public Vector2Int coordinates;
    public UnitType tileState = new UnitType();
    public MazeRoom room;
    public EdgeTile[] edges = new EdgeTile[WalkDirections.Count];
    private int initializedEdgeCount;

    public void Initialize(MazeRoom room)
    {
        room.Add(this);
        transform.GetChild(0).GetComponent<Renderer>().material = room.settings.floorMaterial;
    }

    public EdgeTile GetEdge(WalkDirection direction)
    {
        return edges[(int)direction];
    }
    public void SetEdge(WalkDirection direction, EdgeTile edge)
    {
        edges[(int)direction] = edge;
        initializedEdgeCount += 1;
    }

    public bool IsFullyInitialized
    {
        get
        {
            return initializedEdgeCount == WalkDirections.Count;
        }
    }

    public WalkDirection RandomUninitializedDirection
    {
        get
        {
            string seed = Time.time.ToString();
            System.Random pseudoRandom = new System.Random(seed.GetHashCode());
            int skips = pseudoRandom.Next(0, WalkDirections.Count - initializedEdgeCount);
            for (int i = 0; i < WalkDirections.Count; i++)
            {
                if(edges[i] == null)
                {
                    if(skips == 0)
                    {
                        return (WalkDirection)i;
                    }
                    skips -= 1;
                }
            }
            throw new System.InvalidOperationException("Tile has no uninitialized directin left. ");
        }
    }
}
