using Godot;
using System;

public partial class HUD : CanvasLayer
{
	[Export] public Player player;
	private ProgressBar healthBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		healthBar = GetNode<ProgressBar>("HealthBar");
		healthBar.Value = player.GetHealth();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnButtonPressed()
	{
		player.AddHealth(-5);
	}

	public void OnPlayerHealthUpdated()
	{
		healthBar.Value = player.GetHealth();
	}
}
