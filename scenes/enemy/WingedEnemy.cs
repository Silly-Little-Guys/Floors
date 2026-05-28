using Godot;
using System;

public partial class WingedEnemy : CharacterBody2D, IEnemy
{
	public const float speed = 100.0f;
	private Node2D player;

    public override void _Ready()
    {
        player = (Node2D)GetNode("../Player");
    }

    public void TakeDamage(int damage)
    {
        throw new NotImplementedException();
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		velocity = direction * speed;

		Velocity = velocity;
		MoveAndSlide();
	}
}
