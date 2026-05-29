using Godot;
using System;

public partial class WingedEnemy : CharacterBody2D, IEnemy
{
	public const float speed = 100.0f;
	public Player player;
    string flightAnimation = "default";
    [Export] public AnimatedSprite2D animatedSprite2D;

    public void TakeDamage(int damage)
    {
        SetMeta("Health", (int)GetMeta("Health") - damage);
        if ((int)GetMeta("Health") <= 0)
        {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
	{
        animatedSprite2D.Play(flightAnimation);
		Vector2 velocity = Velocity;
		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		velocity = direction * speed;

		Velocity = velocity;
		MoveAndSlide();
	}
}
