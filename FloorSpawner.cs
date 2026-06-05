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
	private int levelLoopCount = 0;
	private List<PackedScene> floors = new();
	private const string floorsPath = "res://scenes/floors";
	private const string bottomFloorPath = "res://scenes/floors/bottomfloor.tscn";
	private const int firstLoopLevel = 2;
	private PackedScene bottomFloorScene;
	
	private float nextFloorAttachY = 0;
	private float currentFloorTopY = 0;
	private float currentFloorTileHeight = 0;
	private bool hasSpawnedFloor = false;
	private Floor currentFloor;
	private Floor bottomFloor;
	private Floor floorPendingReplacement;
	private float pendingBottomFloorAttachY = 0;
	private float pendingReplacementThresholdY = 0;
	private bool hasPendingFloorReplacement = false;

	/// <summary>
	/// Populates the internal floors list with floors from a given level pool of floors.
	/// </summary>
	/// <param name="level">The "level" the user is on. Increments every 5. Pulls from the floors directory from the passed in level (i.e. pulls from scenes/floors/level1  if level=1</param>
	private bool PopulateFloors(int level)
	{
		floors.Clear();

		string levelPath = $"{floorsPath}/level{level}";
		string[] fileNames = ResourceLoader.ListDirectory(levelPath);

		if (fileNames.Length == 0)
		{
			GD.PushWarning($"No floor scenes found in: {levelPath}");
			return false;
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

		return floors.Count > 0;
	}

	public override void _Ready()
	{
		bottomFloorScene = ResourceLoader.Load<PackedScene>(bottomFloorPath);

		if (bottomFloorScene == null)
		{
			GD.PushWarning($"Could not load bottom floor scene: {bottomFloorPath}");
		}

		PopulateFloors(level);
		SpawnNextFloor();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		CheckPendingFloorReplacement();
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
			enemySpawner.IncrementDifficulty();
			floorsSinceLastLevel = 0;
			if (!PopulateFloors(level))
			{
				level = firstLoopLevel;
				levelLoopCount++;
				PopulateFloors(level);
			}
		}

		if (floors.Count == 0)
		{
			return;
		}

		Floor previousFloor = currentFloor;
		Floor toSpawn = floors[GD.RandRange(0, floors.Count-1)].Instantiate<Floor>();
		rootParentToSpawnIn.CallDeferred("add_child", toSpawn);

		if (!toSpawn.TryGetFloorVerticalBounds(out float floorTopY, out float floorBottomY, out float tileHeight))
		{
			return;
		}

		float yOffset = hasSpawnedFloor ? nextFloorAttachY - floorBottomY : nextFloorAttachY - floorTopY;
		toSpawn.GlobalPosition += new Vector2(0, yOffset);
		currentFloorTopY = floorTopY + yOffset;
		float currentFloorBottomY = floorBottomY + yOffset;
		currentFloorTileHeight = tileHeight;
		nextFloorAttachY = currentFloorTopY;

		floorsSinceLastLevel++;

		if (hasSpawnedFloor)
		{
			QueueFloorBelowReplacement(previousFloor, currentFloorBottomY, tileHeight);
		}

		currentFloor = toSpawn;
		hasSpawnedFloor = true;

		if (enemySpawner != null)
		{
			enemySpawner.SpawnEnemies(toSpawn);
		}
	}

	private void QueueFloorBelowReplacement(Floor floorToReplace, float newestFloorBottomY, float tileHeight)
	{
		floorPendingReplacement = floorToReplace;
		pendingBottomFloorAttachY = newestFloorBottomY;
		pendingReplacementThresholdY = newestFloorBottomY - tileHeight;
		hasPendingFloorReplacement = true;
	}

	private void CheckPendingFloorReplacement()
	{
		if (!hasPendingFloorReplacement || player == null)
		{
			return;
		}

		if (player.GlobalPosition.Y > pendingReplacementThresholdY)
		{
			return;
		}

		ReplaceFloorBelow(floorPendingReplacement, pendingBottomFloorAttachY);
		floorPendingReplacement = null;
		hasPendingFloorReplacement = false;
	}

	private void ReplaceFloorBelow(Floor floorToReplace, float newestFloorBottomY)
	{
		if (floorToReplace != null && IsInstanceValid(floorToReplace))
		{
			floorToReplace.QueueFree();
		}

		if (bottomFloor != null && IsInstanceValid(bottomFloor))
		{
			bottomFloor.QueueFree();
		}

		if (bottomFloorScene == null)
		{
			return;
		}

		Floor newBottomFloor = bottomFloorScene.Instantiate<Floor>();
		rootParentToSpawnIn.CallDeferred("add_child", newBottomFloor);

		if (!newBottomFloor.TryGetFloorVerticalBounds(out float bottomFloorTopY, out _, out _))
		{
			newBottomFloor.QueueFree();
			return;
		}

		newBottomFloor.GlobalPosition += new Vector2(0, newestFloorBottomY - bottomFloorTopY);
		bottomFloor = newBottomFloor;
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
