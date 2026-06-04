using Godot;
using System;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene testEnemyToSpawn;
	[Export] public Timer spawnTimer;
	[Export] public Player player;
	[Export] public bool wingedEnemy;
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

		
		if (wingedEnemy)
		{
			WingedEnemy e = testEnemyToSpawn.Instantiate<WingedEnemy>();
			e.player = player;
			e.GlobalPosition = spawnPosition;
			toSpawnIn.AddChild(e);
		} else
		{
			JumpyEnemy e = testEnemyToSpawn.Instantiate<JumpyEnemy>();
			e.player = player;
			e.GlobalPosition = spawnPosition;
			toSpawnIn.AddChild(e);
		}

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
