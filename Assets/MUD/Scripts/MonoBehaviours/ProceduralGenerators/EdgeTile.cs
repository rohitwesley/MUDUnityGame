using UnityEngine;

public abstract class EdgeTile : MonoBehaviour
{
    public LevelTile tile, otherTile;
    public WalkDirection directionIndex;

    public virtual void Initialize(LevelTile tile, LevelTile otherTile, WalkDirection direction)
    {
        this.tile = tile;
        this.otherTile = otherTile;
        this.directionIndex = direction;
        tile.SetEdge(direction, this);
        transform.parent = tile.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = LevelStateManager.GetWalkDirectionToWorldOrientation(direction);
    }
}