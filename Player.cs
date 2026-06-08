using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void HealthUpdatedEventHandler(bool damaged);

	[Signal]
	public delegate void CashUpdatedEventHandler(int newCash);

	[Signal]
	public delegate void CashAddedEventHandler(int amountAdded, Vector2 sourceGlobalPosition);

	public const float Speed = 100.0f;
	public const float JumpVelocity = -300.0f;
	private List<IInteractable> nearbyInteractables = new();
	[Export] public float weightInKilograms = 1000.0f;
	

	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public Timer coyoteTimer;
	[Export] public Timer jumpBufferTimer;
	[Export] public HUD hud;
	[Export] public WeaponHandler weaponHandler;
	[Export] public Node bulletSpawnPoint;
	[Export] public AudioStreamPlayer2D equipWeaponSoundPlayer;
	[Export] public AudioStreamPlayer2D drinkPotionSoundPlayer;
	[Export] public AudioStreamPlayer2D damageSoundPlayer;
	[Export] public Node2D mainScene;
	private ItemData heldItem;
	private int currentFloor;

	public int maxHealth { get; private set; }

	public override void _Ready()
	{
		weaponHandler.bulletSpawnPoint = bulletSpawnPoint;
		weaponHandler.hud = hud;
		SetMeta("Health", BuffManager.Instance.currentBuffs[BuffManager.Buffs.MAX_HEALTH]);
		maxHealth = (int)GetMeta("Health");
		weaponHandler.UpdateWeapon();
	}

	/// <summary>
	/// Add an integer amount of health to the player's health.
	/// </summary>
	public void AddHealth(int amount)
	{
		int newHealth = Mathf.Clamp(GetHealth() + amount, 0, 100);
		SetMeta("Health", newHealth);
		EmitSignal(SignalName.HealthUpdated, false);	
	}

	public void TakeDamage(int amount)
	{
		int newHealth = Mathf.Clamp(GetHealth() - amount, 0, 100);
		SetMeta("Health", newHealth);
		EmitSignal(SignalName.HealthUpdated, true);
		damageSoundPlayer.Play();
		if (GetHealth() <= 0)
        {
            ShowDeathScreen();
        }
    }

    private void ShowDeathScreen()
    {
        DeathScreen deathScreen = ResourceLoader.Load<PackedScene>("res://scenes/death_screen/death_screen.tscn").Instantiate() as DeathScreen;
		deathScreen.cashLabel.Text = $"${GetCash()}";
		deathScreen.floorLabel.Text = $"Floor {currentFloor}";
		
		var root = GetTree().Root;
		root.AddChild(deathScreen);
		GetTree().CurrentScene = deathScreen;
		mainScene.QueueFree();
    }

	public void OnNextFloorSpawn(int currentFloorNumber)
	{
		currentFloor = currentFloorNumber;
		hud.floorLabel.Text = $"Floor {currentFloorNumber}";
	}

    public void AddCash(int amount)
	{
		AddCash(amount, GlobalPosition);
	}

	public void AddCash(int amount, Vector2 sourceGlobalPosition)
	{
		int newCash = GetCash() + amount;
		SetMeta("Cash", newCash);
		EmitSignal(SignalName.CashUpdated, newCash);
		EmitSignal(SignalName.CashAdded, amount, sourceGlobalPosition);
	}

	public void TakeCash(int amount)
	{
		int newCash = GetCash() - amount;
		SetMeta("Cash", newCash);
		EmitSignal(SignalName.CashUpdated, newCash);
	}

	public int GetCash()
	{
		return (int) GetMeta("Cash");
	}

	public int GetHealth()
	{
		return (int) GetMeta("Health");
	}

	public void UpdateAndDisplayInteractTooltipLootbox(Lootbox lb)
	{
		if (lb.taken)
		{
			hud.interactTooltip.Visible = false;
			return;
		}
		if (lb.opened)
		{
			hud.interactTooltip.Text = $"Press E to pick up {lb.item.ItemName}";
		}
		else
		{
			hud.interactTooltip.Text = $"Press E to open lootbox";
		}
		hud.interactTooltip.Visible = true;
	}

	public void OnInteractionAreaAreaEntered(Area2D area)
	{
		if (area.IsInGroup("interactable"))
		{
			hud.interactTooltip.Text = "Press E to interact";
			if (area.IsInGroup("lootbox"))
			{
				nearbyInteractables.Add(area.GetParent() as IInteractable);
				UpdateAndDisplayInteractTooltipLootbox(area.GetParent() as Lootbox);
			} else if (area.IsInGroup("vending"))
			{
				nearbyInteractables.Add(area as IInteractable);
				VendingMachine vendingMachine = area as VendingMachine;
				vendingMachine.UpdatePlayerToolTip(this);
				hud.interactTooltip.Visible = true;
			}
			else
			{
				nearbyInteractables.Add(area.GetParent() as IInteractable);
				hud.interactTooltip.Visible = true;
			}
		}
	}

	public void OnInteractionAreaAreaExited(Area2D area)
	{
		if (area.IsInGroup("interactable"))
		{
			if (area.IsInGroup("vending"))
			{
				nearbyInteractables.Remove(area as IInteractable);
			} else
			{
				nearbyInteractables.Remove(area.GetParent() as IInteractable);
			}
		}
		if (nearbyInteractables.Count <= 0)
		{
			hud.interactTooltip.Visible = false;
		}
	}

	public bool UseItem(ItemData item)
	{
		foreach (var effect in item.Effects)
		{
			if (!effect.Apply(this, item))
			{
				return false;
			}
		}
		return true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("interact"))
		{
			if (nearbyInteractables.Count > 0)
			{
				var interactable = nearbyInteractables[0];
				var receivedItem = interactable.Interact(this);
				if (interactable is Lootbox lb)
				{
					UpdateAndDisplayInteractTooltipLootbox(lb);
				}
				if (receivedItem is not null) 
				{
					heldItem = receivedItem;
					hud.UpdateHeldItem(receivedItem);
				}
			}
		}

		if (@event.IsActionPressed("use_item"))
		{
			if (heldItem is null) return;

			if (UseItem(heldItem))
			{
				heldItem = null;
				hud.UpdateHeldItem(heldItem);
			}
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		bool hasWeapon = weaponHandler.HasWeapon();
		string runAnimation = hasWeapon ? "run_weapon" : "run_no_weapon";
		string idleAnimation = hasWeapon ? "idle_weapon" : "idle_no_weapon";
		Vector2 velocity = Velocity;
		bool wasOnFloor = IsOnFloor();

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump"))
		{
			jumpBufferTimer.Start();
		}
		if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
		{
			velocity.Y *= 0.25f;
		}

		if (!jumpBufferTimer.IsStopped() && (!coyoteTimer.IsStopped() || IsOnFloor()))
		{
			velocity.Y = JumpVelocity;
			jumpBufferTimer.Stop();
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
		if (hasWeapon)
		{
			animatedSprite2D.FlipH = GetGlobalMousePosition().X < GlobalPosition.X;
		}
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			if (!hasWeapon)
			{
				if (direction.X < 0)
				{
					animatedSprite2D.FlipH = true;
				} else if (direction.X > 0)
				{
					animatedSprite2D.FlipH = false;
				}
			} 
			animatedSprite2D.Play(runAnimation);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			animatedSprite2D.Play(idleAnimation);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (IsOnFloor() && !wasOnFloor)
		{
			coyoteTimer.Stop();
		}
		else if (!IsOnFloor() && wasOnFloor)
		{
			coyoteTimer.Start();
		}
	}
}
