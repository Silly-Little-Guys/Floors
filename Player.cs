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
	public delegate void CashAddedEventHandler(int amountAdded);

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
	private int heldItem = -1;

	private int maxHealth;

	public override void _Ready()
	{
		weaponHandler.bulletSpawnPoint = bulletSpawnPoint;
		weaponHandler.hud = hud;
		maxHealth = (int)GetMeta("Health");
		weaponHandler.UpdateWeapon();
	}

	/// <summary>
	/// Add an integer amount of health to the player's health. Player health will be clamped from 0 to 100. Additive can be negative.
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
	}

	public void AddCash(int amount)
	{
		int newCash = GetCash() + amount;
		SetMeta("Cash", newCash);
		EmitSignal(SignalName.CashUpdated, newCash);
		EmitSignal(SignalName.CashAdded, amount);
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

	public void OnInteractionAreaAreaEntered(Area2D area)
	{
		if (area.IsInGroup("interactable"))
		{
			nearbyInteractables.Add(area.GetParent() as IInteractable);
		}
	}

	public void OnInteractionAreaAreaExited(Area2D area)
	{
		if (area.IsInGroup("interactable"))
		{
			nearbyInteractables.Remove(area.GetParent() as IInteractable);
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

		// Handle interaction.
		if (Input.IsActionJustPressed("interact"))
		{
			if (nearbyInteractables.Count > 0)
			{
				var interactable = nearbyInteractables[0];
				heldItem = interactable.Interact();
				hud.UpdateHeldItem(heldItem);
			}
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
