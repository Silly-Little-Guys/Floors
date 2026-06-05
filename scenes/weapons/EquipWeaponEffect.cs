using Godot;

[GlobalClass]
public partial class EquipWeaponEffect : ItemEffect
{
	public override void Apply(Player player, ItemData item)
	{
		player.weaponHandler.weaponToUse = item.ItemScene;
		player.weaponHandler.UpdateWeapon();
		player.equipWeaponSoundPlayer.Play();
	}
}
