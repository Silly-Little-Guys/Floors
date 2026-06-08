using Godot;
using System;

public partial class JumpyEnemy : RigidBody2D, IEnemy
{
	// Called when the node enters the scene tree for the first time.
	public Player player;
	[Export] public AudioStreamPlayer2D asp2d;
	[Export] public EnemyHealthBar enemyHealthBar;
	[Export] public CollisionShape2D bodyCollisionShape2D;
	[Export] public CollisionShape2D attackCollisionShape2D;
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public NavigationAgent2D nav;
	[Export] public ShapeCast2D shape;
	[Export] public Timer attackCooldownTimer;
	[Export] public int deathCash;
	[Export] public int attackDamage;
	public const float speed = 30.0f;
	public const float jumpForce = 200.0f;
	public const float jumpRadius = 35.0f;
	public const float gCompensation = 2f;
	public const float maxHorizontalSpeed = 80.0f;
	public const float minimumMoveDistance = 2.0f;
	public const float stuckSpeed = 5.0f;
	public int maxHealth;
	private bool isAttacking = false;
	private bool isDying = false;
	string walk = "walk";
	string state = "walking";
	public override void _Ready()
	{
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
			return;
		}

		if (state.Equals("walking"))
		{
			nav.TargetPosition = player.GlobalPosition;
			Vector2 pathPos = nav.GetNextPathPosition();
			float targetX = pathPos.X;
			if (Mathf.Abs(targetX - GlobalPosition.X) < minimumMoveDistance)
			{
				targetX = player.GlobalPosition.X;
			}

			Vector2 nextPos = new Vector2(targetX, GlobalPosition.Y);
			animatedSprite2D.Play(walk);
			Vector2 toTarget = nextPos - GlobalPosition;
			if (toTarget.LengthSquared() > minimumMoveDistance * minimumMoveDistance)
			{
				ApplyCentralForce(toTarget.Normalized() * speed);
			}

			if (Mathf.Abs(LinearVelocity.X) > maxHorizontalSpeed)
			{
				LinearVelocity = new Vector2(Mathf.Sign(LinearVelocity.X) * maxHorizontalSpeed, LinearVelocity.Y);
			}
	
			if (nextPos.X < GlobalPosition.X)
			{
				animatedSprite2D.FlipH = true;
			} 
			else if (nextPos.X > GlobalPosition.X)
			{
				animatedSprite2D.FlipH = false;
			}
	
			bool isCloseToPathPoint = (pathPos - GlobalPosition).Length() < jumpRadius;
			bool isStuckNearPlayer = Mathf.Abs(LinearVelocity.X) < stuckSpeed
				&& Mathf.Abs(player.GlobalPosition.X - GlobalPosition.X) < jumpRadius * 2.0f;

			if ((isCloseToPathPoint || isStuckNearPlayer) && shape.IsColliding())
			{
				state = "jumping";
				LinearVelocity = Vector2.Zero;
				ApplyCentralImpulse(((nextPos - GlobalPosition).Normalized() + Vector2.Up * gCompensation).Normalized() * jumpForce);
				animatedSprite2D.Play("leap");
			}
		}
		else if (state.Equals("jumping"))
		{
			animatedSprite2D.Play("leap");
			if (shape.IsColliding())
			{
				state = "walking";
			}
		}
	}
}
