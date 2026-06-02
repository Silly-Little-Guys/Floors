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
		toSpawnIn.AddChild(e);
		e.player = player;
		e.GlobalPosition = spawnPosition;

		if (spawnTimer != null)
		{
			spawnTimer.Start();
		}
	}

	public void OnTimerTimeout()
	{
		SpawnEnemies(currentFloor);
	}
}
