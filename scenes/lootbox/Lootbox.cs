using Godot;

public partial class Lootbox : Node2D, IInteractable
{
	[Export] public ItemData[] LootTable;

	private bool opened = false;
	private bool taken = false;
	private ItemData item = new();

	[Export] public Sprite2D chestSprite;
	[Export] public Sprite2D itemSprite;
	[Export] public AudioStreamPlayer2D openSoundPlayer;

	public ItemData Interact()
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
		itemSprite.AddChild(itemNode);

		openSoundPlayer.Play();

		return null; // So player does nothing else, only opens chest
	}
}
