using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	[Export] public int damage;
	[Export] public Timer t;
	private int initialSpeed;
    public override void _Ready()
    {
        t.Start();
		initialSpeed = (int)this.LinearVelocity.Length();
    }
	
	public void OnTimerEnded()
	{
		QueueFree();
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is IEnemy enemy && this.LinearVelocity.Length() > initialSpeed * 0.70f)
		{
			enemy.TakeDamage(damage);
			QueueFree();
		}
	}
}
