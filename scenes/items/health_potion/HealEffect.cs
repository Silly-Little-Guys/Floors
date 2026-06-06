using Godot;

[GlobalClass]
public partial class HealEffect : ItemEffect
{
	public override void Apply(Player player, ItemData item)
	{
		player.AddHealth(20);
		player.drinkPotionSoundPlayer.Play();
	}
}
