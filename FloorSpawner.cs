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
	private int floorsSinceLastLevel = 4;
	private List<PackedScene> floors = new();
	private const string floorsPath = "res://scenes/floors";
	
	private float nextFloorAttachY = 0;
	private float currentFloorTopY = 0;
	private float currentFloorTileHeight = 0;
	private bool hasSpawnedFloor = false;

	/// <summary>
	/// Populates the internal floors list with floors from a given level pool of floors.
	/// </summary>
	/// <param name="level">The "level" the user is on. Increments every 5. Pulls from the floors directory from the passed in level (i.e. pulls from scenes/floors/level1  if level=1</param>
	private void PopulateFloors(int level)
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

	/// <summary>
	/// Spawns in the next floor on top of the current one, randomly pulled from the current level pool of floors.
	/// </summary>
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

		if (!toSpawn.TryGetFloorVerticalBounds(out float floorTopY, out float floorBottomY, out float tileHeight))
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

		if (enemySpawner != null)
		{
			enemySpawner.SpawnEnemies(toSpawn);
		}
	}

	/// <summary>
	/// Checks if the player is one tile away from the top of the highest floor, if so it tries to spawn the next floor.
	/// </summary>
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

}
