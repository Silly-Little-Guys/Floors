using Godot;
using System;

public partial class DeathArea : Area2D
{
	public void OnBodyEntered(Node2D body)
	{
		GD.Print("bouta die bro");
		if (body is Player)
		{
			body.QueueFree();
			GetTree().ReloadCurrentScene();
		}
	}
}
