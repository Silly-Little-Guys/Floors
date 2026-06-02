using Godot;
using System;

public partial class WingedEnemy : CharacterBody2D, IEnemy
{
	public const float speed = 50.0f;
	public Player player;
	public int maxHealth;
	[Export] public AudioStreamPlayer2D asp2d;
	[Export] public EnemyHealthBar enemyHealthBar;
	string flightAnimation = "default";
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public CollisionShape2D collisionShape2D;
	[Export] public NavigationAgent2D nav;

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
