using Godot;

public partial class Lootbox : Node2D, IInteractable
{
	[Export] public ItemData[] LootTable;

	public bool opened { get; private set; } = false;
	public bool taken { get; private set; } = false;
	public ItemData item { get; private set; } = new();

	[Export] public Sprite2D chestSprite;
	[Export] public Sprite2D itemSprite;
	[Export] public AudioStreamPlayer2D openSoundPlayer;

	public ItemData Interact(Player player)
	{
		// If item already taken, do nothing
		if (taken) return null;

		// If chest already opened, give player the item
		if (opened)
		{
			taken = true;
			itemSprite.GetChild(0).QueueFree();
			return item;
		}

		// If lootbox is not opened (nor item has been taken), pick a random item, display it, and store it to give to the player if they want it
		var rng = new RandomNumberGenerator();
		int index = rng.RandiRange(0, LootTable.Length - 1);
		item = LootTable[index];

		opened = true;
		chestSprite.Frame = 1;
		Node itemNode = item.ItemScene.Instantiate();

		foreach (Node node in itemNode.GetChildren())
		{
			if (node is AudioStreamPlayer2D asp2d)
			{
				asp2d.ProcessMode = ProcessModeEnum.Disabled;
			}
		} // Disable all audio streams in mini item scene (because it's just previewing the item)

		itemSprite.AddChild(itemNode);
		openSoundPlayer.Play();

		return null; // So player does nothing else, only opens chest
	}
}
