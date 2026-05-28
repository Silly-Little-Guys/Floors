using Godot;
using System;

public partial class WeaponHandler : Node2D
{
	[Export] public PackedScene weaponToUse;
	private Weapon currentWeapon;

    [Export] public Node bulletSpawnPoint;

	public override void _Ready()
	{
		if (weaponToUse != null)
		{
			currentWeapon = weaponToUse.Instantiate<Weapon>();
			AddChild(currentWeapon);
			if (currentWeapon is GunWeapon gun)
			{
				gun.bulletSpawnPoint = bulletSpawnPoint;
			}
		}
	}

	public void UpdateWeapon()
	{
		if (weaponToUse != null)
		{
			if (currentWeapon != null)
			{
				currentWeapon.QueueFree();
			}
			currentWeapon = weaponToUse.Instantiate<Weapon>();
			AddChild(currentWeapon);
		}
	}

	public bool HasWeapon()
	{
		return currentWeapon != null;
	}
}
