using Godot;
using System;
using System.ComponentModel;

public partial class Lootbox : Node2D, IInteractable
{
	private bool opened = false;

	[Export] public Sprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public int Interact()
	{
		if (opened) return -1;
		opened = true;
		
		GD.Print("Interacted");
		sprite.Frame = 1;

		return 0;
	}
}
