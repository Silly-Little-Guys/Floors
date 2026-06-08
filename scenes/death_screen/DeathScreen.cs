using Godot;
using System;

public partial class DeathScreen : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MusicManager.Instance?.PlayMenuMusic();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnStartButtonPressed()
	{
		MusicManager.Instance?.PlayGameMusic();
		GetTree().ChangeSceneToFile("res://main.tscn");
	}
	
	public void OnMainMenuButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/title_screen/TitleScreen.tscn");
	}

	public void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
