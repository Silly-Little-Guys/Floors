using Godot;

public partial class Lootbox : Node2D, IInteractable
{
	[Export] public ItemData[] LootTable;

	private bool opened = false;
	private bool taken = false;
	private ItemData item = new();

	// 0: health potion
	// 1: rifle
	// 2: pistol
	private static int maxNum = 3;

	[Export] public Sprite2D chestSprite;
	[Export] public Sprite2D itemSprite;

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

		// If lootbox is not opened (nor item has been taken)
		var rng = new RandomNumberGenerator();
		int index = rng.RandiRange(0, LootTable.Length - 1);
		item = LootTable[index];

		opened = true;
		chestSprite.Frame = 1;
		Node itemNode = item.ItemScene.Instantiate();
		itemSprite.AddChild(itemNode);

		return null; // So player does nothing
	}
}
