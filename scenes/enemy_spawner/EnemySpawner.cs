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

		if (!toSpawnIn.TryGetFloorVerticalBounds(out float topY, out float bottomY, out float tileHeight) ||
			!toSpawnIn.TryGetFloorHorizontalBounds(out float leftX, out float rightX, out float tileWidth))
		{
			return;
		}

		float spawnMinX = Mathf.Min(leftX, rightX) + tileWidth;
		float spawnMaxX = Mathf.Max(leftX, rightX) - tileWidth;
		float spawnUpperY = Mathf.Min(topY, bottomY) + tileHeight;
		float spawnLowerY = Mathf.Max(topY, bottomY) - tileHeight;

		if (spawnMinX > spawnMaxX || spawnUpperY > spawnLowerY)
		{
			GD.PushWarning("Cannot spawn enemy because the floor bounds are too small after one-tile padding.");
			return;
		}

		WingedEnemy e = testEnemyToSpawn.Instantiate<WingedEnemy>();
		toSpawnIn.AddChild(e);
		e.player = player;
		e.GlobalPosition = new Vector2(
			(float)GD.RandRange(spawnMinX, spawnMaxX),
			(float)GD.RandRange(spawnUpperY, spawnLowerY)
		);
		spawnTimer.Start();
	}

	public void OnTimerTimeout()
	{
		SpawnEnemies(currentFloor);
	}
}
