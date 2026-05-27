using Godot;
using System;
using System.Collections.Generic;
// no i was here
//I was here
public partial class FloorSpawner : Node2D
{
	[Export] public Player player;
	[Export] public Node2D rootParentToSpawnIn;
	[Export] public EnemySpawner enemySpawner;
	[Export] public int level = 1;
	private List<PackedScene> floors = new();
	private const string floorsPath = "res://scenes/floors";
	
	private float previousHeight = 0;

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
		if (floors.Count == 0)
		{
			level++;
			PopulateFloors(level);
		}
		Floor toSpawn = floors[0].Instantiate<Floor>();
		floors.RemoveAt(0);
		rootParentToSpawnIn.AddChild(toSpawn);
		toSpawn.GlobalPosition = new Vector2(toSpawn.GlobalPosition.X, toSpawn.GlobalPosition.Y + previousHeight);
		previousHeight = GetFloorHeight(toSpawn);
	}

	private float GetFloorHeight(Floor floor)
	{
		if (floor?.mainTileMapLayer == null)
		{
			GD.PushWarning("Cannot calculate floor height because the floor has no main TileMapLayer assigned.");
			return 0;
		}

		TileMapLayer tileMapLayer = floor.mainTileMapLayer;
		Rect2I usedRect = tileMapLayer.GetUsedRect();

		if (usedRect.Size.Y == 0 || tileMapLayer.TileSet == null)
		{
			return 0;
		}

		int lowestTileY = usedRect.Position.Y;
		int highestTileY = usedRect.End.Y - 1;
		int tileRows = highestTileY - lowestTileY + 1;
		float tileHeight = tileMapLayer.TileSet.TileSize.Y;
		float layerScale = Mathf.Abs(tileMapLayer.GlobalScale.Y);
		return tileRows * tileHeight * layerScale;
	}
}
