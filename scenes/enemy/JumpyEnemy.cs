using Godot;
using System;

public partial class JumpyEnemy : RigidBody2D, IEnemy
{
	// Called when the node enters the scene tree for the first time.
	public Player player;
	[Export] public AudioStreamPlayer2D asp2d;
	[Export] public EnemyHealthBar enemyHealthBar;
	[Export] public CollisionShape2D collisionShape2D;
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public NavigationAgent2D nav;
	[Export] public ShapeCast2D shape;
	public const float speed = 30.0f;
	public const float jumpForce = 200.0f;
	public const float jumpRadius = 35.0f;
	public const float gCompensation = 3f;
	public int maxHealth;
	string walk = "walk";
	string state = "walking";
	public override void _Ready()
	{
		maxHealth = GetHealth();
	}

	public void TakeDamage(int damage)
	{
		SetMeta("Health", GetHealth() - damage);
		if (GetHealth() <= 0)
		{
			asp2d.Play();
			this.Visible = false;
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

	public override void _PhysicsProcess(double delta)
	{
		if (state.Equals("walking"))
		{
			nav.TargetPosition = player.GlobalPosition;
			Vector2 nextPos = new Vector2(nav.GetNextPathPosition().X, GlobalPosition.Y);
			animatedSprite2D.Play(walk);
			ApplyCentralForce((nextPos - GlobalPosition).Normalized() * speed);

			if (nextPos.X < GlobalPosition.X)
			{
				animatedSprite2D.FlipH = true;
			} 
			else if (nextPos.X > GlobalPosition.X)
			{
				animatedSprite2D.FlipH = false;
			}

			if ((nextPos - GlobalPosition).Length() < jumpRadius && shape.IsColliding())
			{
				state = "jumping";
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
