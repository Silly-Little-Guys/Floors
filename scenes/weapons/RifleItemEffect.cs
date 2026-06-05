using Godot;

[GlobalClass]
public partial class RifleItemEffect : ItemEffect
{
	public override void Apply(Player player, ItemData item)
	{
		player.weaponHandler.weaponToUse = item.ItemScene;
        player.weaponHandler.UpdateWeapon();
	}
}
