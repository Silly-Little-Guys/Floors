using Godot;
using System;
using System.Collections.Generic;
// no i was here
//I was here
public partial class FloorSpawner : Node2D
{
	[Export] public Player player;
	[Export] public int level = 1;
	private List<PackedScene> floors = new();
	private const string floorsPath = "res://scenes/floors";

	public void PopulateFloors(int level)
	{
		floors.Clear();

		string levelPath = $"{floorsPath}/level{level}";
		string[] fileNames = ResourceLoader.ListDirectory(levelPath);

		if (fileNames.Length == 0)
		{
			GD.PushWarning($"No floor scenes found in: {levelPath}");
			return;
		}

		foreach (string fileName in fileNames)
		{
			if (!fileName.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string scenePath = $"{levelPath}/{fileName}";
			PackedScene floorScene = ResourceLoader.Load<PackedScene>(scenePath);

			if (floorScene == null)
			{
				GD.PushWarning($"Could not load floor scene: {scenePath}");
				continue;
			}

			floors.Add(floorScene);
		}

		ShuffleFloors();
	}

	private void ShuffleFloors()
	{
		for (int index = floors.Count - 1; index > 0; index--)
		{
			int swapIndex = GD.RandRange(0, index);
			(floors[index], floors[swapIndex]) = (floors[swapIndex], floors[index]);
		}
	}

	public override void _Ready()
	{
		PopulateFloors(level);
	}
	
	public override void _PhysicsProcess(double delta)
	{
		
	}

	public void SpawnNextFloor()
	{
		
	}
}
