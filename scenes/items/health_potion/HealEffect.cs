using Godot;

[GlobalClass]
public partial class HealEffect : ItemEffect
{
	public override void Apply(Player player)
	{
		player.AddHealth(20);
	}
}
