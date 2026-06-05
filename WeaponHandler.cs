using Godot;
using System;

public partial class WeaponHandler : Node2D
{
	[Export] public PackedScene weaponToUse;
	private Weapon currentWeapon;

	[Export] public Node bulletSpawnPoint;
	[Export] public Player player;
	[Export] public HUD hud;

	public void UpdateWeapon()
	{
		// When a weapon is dropped, we should unsubscribe from that weapon's events because they're no longer relevant (we don't need to know its ammo count anymore)
		if (currentWeapon != null)
		{
			currentWeapon.OnAmmoCountUpdated -= UpdateAmmoDisplay;	
		}

		if (weaponToUse != null)
		{
			if (currentWeapon != null)
			{
				currentWeapon.QueueFree();
			}
			currentWeapon = weaponToUse.Instantiate<Weapon>();
			currentWeapon.bulletSpawnPoint = bulletSpawnPoint;
			currentWeapon.player = player;
			currentWeapon.OnAmmoCountUpdated += UpdateAmmoDisplay;
			UpdateAmmoDisplay(currentWeapon.ammoCount);
			AddChild(currentWeapon);
		}
	}

	public void UpdateAmmoDisplay(int ammo)
	{
		hud.SetAmmoDisplay(ammo);
	}

	public bool HasWeapon()
	{
		return currentWeapon != null;
	}
}
