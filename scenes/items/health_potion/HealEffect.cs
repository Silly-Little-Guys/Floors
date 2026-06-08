using Godot;

[GlobalClass]
public partial class HealEffect : ItemEffect
{
	public override void Apply(Player player, ItemData item)
	{
		player.AddHealth(BuffManager.Instance.currentBuffs[BuffManager.Buffs.POTION_POTENCY]);
		player.drinkPotionSoundPlayer.Play();
	}
}
