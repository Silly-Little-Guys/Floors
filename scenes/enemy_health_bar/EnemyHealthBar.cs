using Godot;
using System;

public partial class EnemyHealthBar : Node2D
{
	[Export] public ProgressBar pbar;
	[Export] public Label healthLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetProgress(float val)
	{
		pbar.Value = val * 100;
	}

	public void UpdateHealth(int currHealth, int maxHealth)
	{
		this.SetProgress(Mathf.InverseLerp(0, maxHealth, currHealth));
		this.healthLabel.Text = $"{currHealth} HP";
	}
}
