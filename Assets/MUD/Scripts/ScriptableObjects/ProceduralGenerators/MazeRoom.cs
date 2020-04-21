using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MazeRoomSettings
{

	public Material floorMaterial, wallMaterial;
}

[CreateAssetMenu(fileName = "Level1", menuName = "MUDTools/Room", order = 1)]
public class MazeRoom : ScriptableObject
{

	public int settingsIndex;

	public MazeRoomSettings settings;

	private List<LevelTile> cells = new List<LevelTile>();

	public void Add(LevelTile cell)
	{
		cell.room = this;
		cells.Add(cell);
	}

	public void Assimilate(MazeRoom room)
	{
		for (int i = 0; i < room.cells.Count; i++)
		{
			Add(room.cells[i]);
		}
	}

}
