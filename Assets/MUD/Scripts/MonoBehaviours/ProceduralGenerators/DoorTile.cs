using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTile : PathTile
{

	public Transform hinge;
	private DoorTile OtherSideOfDoor
	{
		get
		{
			return otherTile.GetEdge(directionIndex.GetOpposite()) as DoorTile;
		}
	}

	public override void Initialize(LevelTile primary, LevelTile other, WalkDirection direction)
	{
		base.Initialize(primary, other, direction);
		if (OtherSideOfDoor != null)
		{
			hinge.localScale = new Vector3(-1f, 1f, 1f);
			Vector3 p = hinge.localPosition;
			p.x = -p.x;
			hinge.localPosition = p;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (child != hinge)
			{
				child.GetComponent<Renderer>().material = tile.room.settings.wallMaterial;
			}
		}
	}

}
