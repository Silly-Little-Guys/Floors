using Godot;
using System;

public partial class VendingMachine : Area2D, IInteractable
{
	[Export] public BuffManager.Buffs buff;
	[Export] public int amountToIncrement;
	[Export] public int initialCost;

	private int calculateCost()
	{
		return initialCost;
	}

	public void UpdatePlayerToolTip(Player player)
	{
		player.hud.interactTooltip.Text = "$" + calculateCost() + "\nPress E to increase " + BuffManager.Instance.GetBuffAsString(buff) + " by " + amountToIncrement;
	}

    public ItemData Interact(Player player)
	{
		UpdatePlayerToolTip(player);
		int cost = calculateCost();
		if (player.GetCash() >= cost)
		{
			player.TakeCash(cost, GlobalPosition);
			BuffManager.Instance.currentBuffs[buff] += amountToIncrement;
		} else
		{
			player.hud.interactTooltip.Text = "Not Enough Money";
		}
		return null;
	}
}
