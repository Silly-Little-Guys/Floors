using Godot;
using System;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene testEnemyToSpawn;
	public void SpawnEnemies(Floor toSpawnIn)
	{
		Enemy e = testEnemyToSpawn.Instantiate<Enemy>();
		toSpawnIn.AddChild(e);
		e.GlobalPosition = new Vector2(0, 10);
	}
}
