using Godot;
using System;

public partial class WeaponHandler : Node2D
{
	[Export] public PackedScene weaponToUse;
	private Weapon currentWeapon;

    [Export] public Node bulletSpawnPoint;

	public void UpdateWeapon()
	{
		if (weaponToUse != null)
		{
			if (currentWeapon != null)
			{
				currentWeapon.QueueFree();
			}
			currentWeapon = weaponToUse.Instantiate<Weapon>();
			if (currentWeapon is GunWeapon gun)
			{
				GD.Print("ts is bulllllllllll");
				gun.bulletSpawnPoint = bulletSpawnPoint;
			}
			AddChild(currentWeapon);
		}
	}

	public bool HasWeapon()
	{
		return currentWeapon != null;
	}
}
