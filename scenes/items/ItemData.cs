using Godot;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemName { get; private set; }

    [Export] public PackedScene ItemScene { get; private set; }

    [Export] public Texture2D ItemTexture { get; private set; }

    [Export] public Godot.Collections.Array<ItemEffect> Effects { get; set; }
}