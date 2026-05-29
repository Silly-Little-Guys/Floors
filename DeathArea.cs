using Godot;
using System;

public partial class DeathArea : Area2D
{
	public void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			body.QueueFree();
			GetTree().ReloadCurrentScene();
		}
	}
}
