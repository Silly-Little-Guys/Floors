using Godot;
using System;
using System.ComponentModel;

public partial class HUD : CanvasLayer
{
	[Export] public Player player;
	[Export] public ProgressBar healthBar;
	[Export] public Label ammoLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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

	/// <summary>
	/// Updates the ammo label text to the integer passed as a parameter.
	/// </summary>
	public void SetAmmoDisplay(int ammo)
	{
		ammoLabel.Text = ammo.ToString();
	}

	public void OnAmmoCountUpdated()
	{
		
	}

	public void OnGunEquipped()
	{
		
	}

	public void OnGunUnequipped()
	{
			
	}
}
