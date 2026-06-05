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
	[Export] public CollisionShape2D collisionShape2D;
	[Export] public NavigationAgent2D nav;
	[Export] public Timer attackCooldownTimer;
	private bool isAttacking = false;
	public override void _Ready()
	{
		maxHealth = GetHealth();
	}

	public void TakeDamage(int damage)
	{
		SetMeta("Health", GetHealth() - damage);
		if (GetHealth() <= 0)
		{
			player.AddCash(deathCash);
			asp2d.Play();
			this.Visible = false;
			// collisionShape2D.Disabled = true;
			collisionShape2D.SetDeferred("disabled", true);
		}
		enemyHealthBar.SetProgress(Mathf.InverseLerp(0, maxHealth, GetHealth()));
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
		if (isAttacking)
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
		nav.TargetPosition = player.GlobalPosition;
		Vector2 nextPos = nav.GetNextPathPosition();
		animatedSprite2D.Play(flightAnimation);
		Vector2 updateVelo = (nextPos - GlobalPosition).Normalized() * speed;
		if (Velocity.X < 0)
		{
			animatedSprite2D.FlipH = true;
		} else if (Velocity.X > 0)
		{
			animatedSprite2D.FlipH = false;
		}
		Velocity = updateVelo;
		MoveAndSlide();
	}
}
