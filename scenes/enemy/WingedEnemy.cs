using Godot;
using System;
using System.Net;

public partial class WingedEnemy : CharacterBody2D, IEnemy
{
	public const float speed = 50.0f;
	public Player player;
	public int maxHealth;
	[Export] public int attackDamage;
	[Export] public int deathCash;
	[Export] public AudioStreamPlayer2D asp2d;
	[Export] public EnemyHealthBar enemyHealthBar;
	string flightAnimation = "default";
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public CollisionShape2D bodyCollisionShape2D;
	[Export] public CollisionShape2D attackCollisionShape2D;
	[Export] public NavigationAgent2D nav;
	[Export] public Timer attackCooldownTimer;
	private bool isAttacking = false;
	private bool isDying = false;
	private const float minimumMoveDistance = 2.0f;
	public override void _Ready()
	{
		MotionMode = MotionModeEnum.Floating;
		maxHealth = GetHealth();
		enemyHealthBar.UpdateHealth(GetHealth(), maxHealth);
	}

	public void TakeDamage(int damage)
	{
		SetMeta("Health", GetHealth() - damage);
		if (GetHealth() <= 0)
		{
			player.AddCash(deathCash, GlobalPosition);
			isDying = true;
			asp2d.Play();
			this.Visible = false;
			attackCollisionShape2D.SetDeferred("disabled", true);
			bodyCollisionShape2D.SetDeferred("disabled", true);
		}
		enemyHealthBar.UpdateHealth(GetHealth(), maxHealth);
	}

	private int GetHealth()
	{
		return (int) GetMeta("Health");
	}

	public void OnAudioFinished()
	{
		QueueFree();
	}

	public void Attack()
	{
		if (isAttacking && !isDying)
		{
			player.TakeDamage(attackDamage);
			attackCooldownTimer.Start();
		}
	}

	public void OnAttackCooldownTimeout()
	{
		Attack();
	}

	public void OnAttackAreaBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			isAttacking = true;
			attackCooldownTimer.Start();
			Attack();
		}
	}

	public void OnAttackAreaBodyExited(Node2D body)
	{
		if (body is Player)
		{
			isAttacking = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (player == null || isDying)
		{
			Velocity = Vector2.Zero;
			return;
		}

		animatedSprite2D.Play(flightAnimation);
		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
		Velocity = toPlayer.LengthSquared() > minimumMoveDistance * minimumMoveDistance
			? toPlayer.Normalized() * speed
			: Vector2.Zero;

		if (Velocity.X < 0)
		{
			animatedSprite2D.FlipH = true;
		} else if (Velocity.X > 0)
		{
			animatedSprite2D.FlipH = false;
		}

		MoveAndSlide();
	}
}
