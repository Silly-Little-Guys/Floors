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
	private int level = 1;
	private int floorsSinceLastLevel = 0;
	private List<PackedScene> floors = new();
	private const string floorsPath = "res://scenes/floors";
	
	private float nextFloorAttachY = 0;
	private float currentFloorTopY = 0;
	private float currentFloorTileHeight = 0;
	private bool hasSpawnedFloor = false;

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
		SpawnNextFloor();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		CheckPlayerPositionForNextFloor();
	}

	public void SpawnNextFloor()
	{
		if (floorsSinceLastLevel >= 5)
		{
			level++;
			floorsSinceLastLevel = 0;
			PopulateFloors(level);
		}

		if (floors.Count == 0)
		{
			return;
		}

		Floor toSpawn = floors[GD.RandRange(0, floors.Count-1)].Instantiate<Floor>();
		rootParentToSpawnIn.CallDeferred("add_child", toSpawn);
		if (!TryGetFloorVerticalBounds(toSpawn, out float floorTopY, out float floorBottomY, out float tileHeight))
		{
			return;
		}

		float yOffset = hasSpawnedFloor ? nextFloorAttachY - floorBottomY : nextFloorAttachY - floorTopY;
		toSpawn.GlobalPosition += new Vector2(0, yOffset);
		currentFloorTopY = floorTopY + yOffset;
		currentFloorTileHeight = tileHeight;
		hasSpawnedFloor = true;
		nextFloorAttachY = currentFloorTopY;

		floorsSinceLastLevel++;
	}

	private void CheckPlayerPositionForNextFloor()
	{
		if (!hasSpawnedFloor || player == null)
		{
			return;
		}

		if (player.GlobalPosition.Y <= currentFloorTopY + currentFloorTileHeight)
		{
			SpawnNextFloor();
		}
	}

	private bool TryGetFloorVerticalBounds(Floor floor, out float topY, out float bottomY, out float tileHeight)
	{
		topY = 0;
		bottomY = 0;
		tileHeight = 0;

		if (floor?.mainTileMapLayer == null)
		{
			GD.PushWarning("Cannot calculate floor bounds because the floor has no main TileMapLayer assigned.");
			return false;
		}

		TileMapLayer tileMapLayer = floor.mainTileMapLayer;

		if (tileMapLayer.TileSet == null)
		{
			GD.PushWarning("Cannot calculate floor bounds because the main TileMapLayer has no TileSet.");
			return false;
		}

		Rect2I usedRect = tileMapLayer.GetUsedRect();

		if (usedRect.Size == Vector2I.Zero)
		{
			GD.PushWarning("Cannot calculate floor bounds because the main TileMapLayer has no used cells.");
			return false;
		}

		Vector2I tileSize = tileMapLayer.TileSet.TileSize;
		tileHeight = Mathf.Abs(tileMapLayer.ToGlobal(new Vector2(0, tileSize.Y)).Y - tileMapLayer.ToGlobal(Vector2.Zero).Y);
		Vector2 localTopLeft = new Vector2(usedRect.Position.X * tileSize.X, usedRect.Position.Y * tileSize.Y);
		Vector2 localBottomRight = new Vector2(usedRect.End.X * tileSize.X, usedRect.End.Y * tileSize.Y);

		Vector2[] corners =
		{
			localTopLeft,
			new Vector2(localBottomRight.X, localTopLeft.Y),
			localBottomRight,
			new Vector2(localTopLeft.X, localBottomRight.Y)
		};

		topY = float.PositiveInfinity;
		bottomY = float.NegativeInfinity;

		foreach (Vector2 corner in corners)
		{
			float globalY = tileMapLayer.ToGlobal(corner).Y;
			topY = Mathf.Min(topY, globalY);
			bottomY = Mathf.Max(bottomY, globalY);
		}

		return true;
	}
}
