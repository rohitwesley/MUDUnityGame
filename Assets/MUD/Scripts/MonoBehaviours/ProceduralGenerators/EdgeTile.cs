using UnityEngine;

public abstract class EdgeTile : MonoBehaviour
{
    public Vector2Int tile, otherTile;
    public WalkDirection directionIndex;

    public void Initialize(Vector2Int tile, Vector2Int otherTile, WalkDirection direction)
    {
        this.tile = tile;
        this.otherTile = otherTile;
        this.directionIndex = direction;
    }
}