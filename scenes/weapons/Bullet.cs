using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	[Export] public int damage;
	[Export] public Timer t;
    public override void _Ready()
    {
        t.Start();
    }
	
	public void OnTimerEnded()
	{
		QueueFree();
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is IEnemy enemy)
		{
			enemy.TakeDamage(damage);
		}
		QueueFree();
	}
}
