using Godot;
using System;
using System.ComponentModel;
using System.Globalization;

public partial class HUD : CanvasLayer
{
	[Export] public Player player;
	[Export] public ProgressBar healthBar;
	[Export] public Label ammoLabel;
	[Export] public AnimationPlayer damageAnimation;
	[Export] public Label cashLabel;
	[Export] public CompressedTexture2D cashImage;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		healthBar.Value = player.GetHealth();
		OnPlayerCashUpdated(player.GetCash());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnButtonPressed()
	{
		player.TakeDamage(10);
	}

	public void OnPlayerHealthUpdated(bool damaged)
	{
		healthBar.Value = player.GetHealth();
		if (damaged) damageAnimation.Play("damage_flash");
	}

	public void OnPlayerCashUpdated(int newAmount)
	{
		cashLabel.Text = $"${newAmount.ToString("N0", CultureInfo.InvariantCulture)}";
	}

	public void OnPlayerCashAdded(int amount)
	{
		// do some fancy animatino to show the cash added.
	}

	public void UpdateHeldItem(int item)
	{
		
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
