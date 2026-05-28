using Godot;
using System;
using System.ComponentModel;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void HealthUpdatedEventHandler();

	public const float Speed = 125.0f;
	public const float JumpVelocity = -300.0f;
	

	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public Timer coyoteTimer;
	[Export] public Timer jumpBufferTimer;
	[Export] public HUD hud;
	[Export] public WeaponHandler weaponHandler;

	public override void _Ready()
	{
		weaponHandler.bulletSpawnPoint = GetParent();
	}

	/// <summary>
	/// Add an integer amount of health to the player's health. Player health will be clamped from 0 to 100. Additive can be negative.
	/// </summary>
	public void AddHealth(int additive)
	{
		int newHealth = Mathf.Clamp((int) GetMeta("Health") + additive, 0, 100);
		SetMeta("Health", newHealth);
		GD.Print("Health: " + this.GetHealth());
		EmitSignal(SignalName.HealthUpdated);	
	}

	public int GetHealth()
	{
		return (int) GetMeta("Health");
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
