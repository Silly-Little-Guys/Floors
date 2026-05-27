using Godot;
using System;

public partial class CameraFollow : Camera2D
{
	[Export] public Player player;
	[Export] public float followSpeed = 8.0f;

	public override void _Ready()
	{
		IgnoreRotation = true;

		if (player == null)
		{
			player = GetNodeOrNull<Player>("../player");
		}

		if (player != null)
		{
			GlobalPosition = player.GlobalPosition;
		}
	}

	public override void _Process(double delta)
	{
		if (player == null)
		{
			return;
		}

		float followWeight = 1.0f - Mathf.Exp(-followSpeed * (float)delta);
		GlobalPosition = GlobalPosition.Lerp(player.GlobalPosition, followWeight);
		GlobalRotation = 0;
	}
}
