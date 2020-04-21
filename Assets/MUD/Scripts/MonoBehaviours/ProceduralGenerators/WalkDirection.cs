using UnityEngine;

public enum WalkDirection
{
    North,
    East,
    South,
    West
}


public static class WalkDirections
{
	public const int Count = 4;

	public static WalkDirection RandomValue
	{
		get
		{
			return (WalkDirection)Random.Range(0, Count);
		}
	}

	private static WalkDirection[] opposites = {
		WalkDirection.South,
		WalkDirection.West,
		WalkDirection.North,
		WalkDirection.East
	};

	public static WalkDirection GetOpposite(this WalkDirection direction)
	{
		return opposites[(int)direction];
	}

	private static Vector2Int[] vectors = {
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0)
	};

	public static Vector2Int ToIntVector2(this WalkDirection direction)
	{
		return vectors[(int)direction];
	}

	private static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	public static Quaternion ToRotation(this WalkDirection direction)
	{
		return rotations[(int)direction];
	}
}
