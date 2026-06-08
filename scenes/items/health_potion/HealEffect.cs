using Godot;

[GlobalClass]
public partial class HealEffect : ItemEffect
{
	public override bool Apply(Player player, ItemData item)
	{
		if (player.GetHealth() == player.maxHealth)
		{
			player.hud.FlashHealthErrortip();
			return false;
		}
		player.AddHealth(BuffManager.Instance.currentBuffs[BuffManager.Buffs.POTION_POTENCY]);
		player.drinkPotionSoundPlayer.Play();
		return true;
	}
}
