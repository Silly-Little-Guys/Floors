using Godot;
using System;

public partial class EnemyHealthBar : Node2D
{
	[Export] public CharacterBody2D enemy;
	[Export] public ProgressBar pbar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pbar.Value = (int) enemy.GetMeta("Health");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnHealthChanged()
	{
		pbar.Value = (int) enemy.GetMeta("Health");
	}
}
