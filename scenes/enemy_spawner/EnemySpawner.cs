using Godot;
using System;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene testEnemyToSpawn;
	[Export] public Timer spawnTimer;
	[Export] public Player player;
	private Floor currentFloor;

	public void SpawnEnemies(Floor toSpawnIn)
	{
		currentFloor = toSpawnIn;

		if (toSpawnIn == null)
		{
			return;
		}

		if (!toSpawnIn.TryGetRandomNonCollidingSpawnPosition(out Vector2 spawnPosition))
		{
			return;
		}

		WingedEnemy e = testEnemyToSpawn.Instantiate<WingedEnemy>();
		e.player = player;
		e.GlobalPosition = spawnPosition;

		if (spawnTimer != null)
		{
			spawnTimer.Start();
		}
		toSpawnIn.AddChild(e);
	}

	public void OnTimerTimeout()
	{
		SpawnEnemies(currentFloor);
	}
}
