using Godot;

[GlobalClass]
public abstract partial class ItemEffect : Resource
{
    public abstract void Apply(Player player, ItemData item);
}