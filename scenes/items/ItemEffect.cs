using Godot;

[GlobalClass]
public abstract partial class ItemEffect : Resource
{
	/// <summary>
	/// Abstract function for applying an item effect.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <returns>Whether the item was successfully used or not.</returns>
	public abstract bool Apply(Player player, ItemData item);
}
