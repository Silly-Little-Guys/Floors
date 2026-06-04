using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node2D
{
	[Export] public int difficultyLevel = 0;
	[Export] public Timer spawnTimer;
	[Export] public Player player;
	private Floor currentFloor;
	private readonly List<EnemyScene> enemyScenes = new();
	private const string enemyScenesPath = "res://scenes/enemy";

	private class EnemyScene
	{
		public int Difficulty { get; }
		public PackedScene Scene { get; }

		public EnemyScene(int difficulty, PackedScene scene)
		{
			Difficulty = difficulty;
			Scene = scene;
		}
	}

	public override void _Ready()
	{
		PopulateEnemyScenes();
	}

	private void PopulateEnemyScenes()
	{
		enemyScenes.Clear();

		string[] fileNames = ResourceLoader.ListDirectory(enemyScenesPath);
		if (fileNames.Length == 0)
		{
			GD.PushWarning($"No enemy scenes found in: {enemyScenesPath}");
			return;
		}

		foreach (string fileName in fileNames)
		{
			if (!fileName.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			int underscoreIndex = fileName.IndexOf('_');
			if (underscoreIndex <= 0 || !int.TryParse(fileName[..underscoreIndex], out int enemyDifficulty))
			{
				GD.PushWarning($"Enemy scene name does not start with a difficulty number: {fileName}");
				continue;
			}

			string scenePath = $"{enemyScenesPath}/{fileName}";
			PackedScene enemyScene = ResourceLoader.Load<PackedScene>(scenePath);

			if (enemyScene == null)
			{
				GD.PushWarning($"Could not load enemy scene: {scenePath}");
				continue;
			}

			enemyScenes.Add(new EnemyScene(enemyDifficulty, enemyScene));
		}

		enemyScenes.Sort((a, b) => a.Difficulty.CompareTo(b.Difficulty));
	}

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

		PackedScene enemyScene = GetEnemySceneToSpawn();
		if (enemyScene == null)
		{
			return;
		}

		Node enemyNode = enemyScene.Instantiate();
		if (enemyNode is not Node2D enemy)
		{
			GD.PushWarning("Enemy scene root must extend Node2D.");
			enemyNode.QueueFree();
			return;
		}

		if (!SetEnemyPlayer(enemy))
		{
			GD.PushWarning("Enemy scene root must extend WingedEnemy or JumpyEnemy.");
			enemy.QueueFree();
			return;
		}

		enemy.GlobalPosition = spawnPosition;
		toSpawnIn.AddChild(enemy);

		if (spawnTimer != null)
		{
			spawnTimer.Start();
		}
	}

	private PackedScene GetEnemySceneToSpawn()
	{
		if (enemyScenes.Count == 0)
		{
			PopulateEnemyScenes();
		}

		if (enemyScenes.Count == 0)
		{
			return null;
		}

		List<EnemyScene> eligibleEnemyScenes = new();
		foreach (EnemyScene enemyScene in enemyScenes)
		{
			if (enemyScene.Difficulty <= difficultyLevel)
			{
				eligibleEnemyScenes.Add(enemyScene);
			}
		}

		if (eligibleEnemyScenes.Count == 0)
		{
			GD.PushWarning($"No enemy scenes are at or below difficulty level {difficultyLevel}.");
			return null;
		}

		float totalWeight = 0.0f;
		foreach (EnemyScene enemyScene in eligibleEnemyScenes)
		{
			totalWeight += GetEnemyWeight(enemyScene);
		}

		float roll = (float)GD.Randf() * totalWeight;
		float accumulatedWeight = 0.0f;

		foreach (EnemyScene enemyScene in eligibleEnemyScenes)
		{
			accumulatedWeight += GetEnemyWeight(enemyScene);
			if (roll <= accumulatedWeight)
			{
				return enemyScene.Scene;
			}
		}

		return eligibleEnemyScenes[^1].Scene;
	}

	private float GetEnemyWeight(EnemyScene enemyScene)
	{
		return difficultyLevel - enemyScene.Difficulty + 1;
	}

	private bool SetEnemyPlayer(Node2D enemy)
	{
		if (enemy is WingedEnemy wingedEnemy)
		{
			wingedEnemy.player = player;
			return true;
		}

		if (enemy is JumpyEnemy jumpyEnemy)
		{
			jumpyEnemy.player = player;
			return true;
		}

		return false;
	}

	public void OnTimerTimeout()
	{
		SpawnEnemies(currentFloor);
	}
}
