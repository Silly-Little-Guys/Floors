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
		WingedEnemy e = testEnemyToSpawn.Instantiate<WingedEnemy>();
		toSpawnIn.AddChild(e);
		e.player = player;
		e.GlobalPosition = toSpawnIn.GlobalPosition + new Vector2(0, -50);
		spawnTimer.Start();
	}

	public void OnTimerTimeout()
	{
		SpawnEnemies(currentFloor);
	}
}
