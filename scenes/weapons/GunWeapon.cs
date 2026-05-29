using Godot;
using System;

public partial class GunWeapon : Weapon
{
	[Export] public int damage;
	[Export] public float fireRate;
	[Export] public float bulletSpeed;
	[Export] public PackedScene bulletScene;
	[Export] public Timer fireTimer;
	[Export] public Node bulletSpawnPoint;
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public Node2D pivotPoint;

	public override void _Ready()
	{
		fireTimer.WaitTime = 1 / fireRate;
	}
	public void OnShoot()
	{
		if (!fireTimer.IsStopped())
		{
			return;
		}
		fireTimer.Start();
		Bullet b = bulletScene.Instantiate<Bullet>();
		b.damage = damage;
		b.GlobalPosition = GlobalPosition;
		b.Rotation = Rotation;
		b.LinearVelocity = new Vector2(bulletSpeed, 0).Rotated(Rotation);
		bulletSpawnPoint.AddChild(b);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 aimPivot = pivotPoint != null ? pivotPoint.GlobalPosition : GlobalPosition;
		Vector2 aimDirection = GetGlobalMousePosition() - aimPivot;
		GlobalRotation = aimDirection.Angle();
		animatedSprite2D.FlipV = aimDirection.X < 0;
		if (Input.IsActionPressed("shoot"))
		{
			OnShoot();
			animatedSprite2D.Play("firing");
		} else
		{
			animatedSprite2D.Play("idle");   
		}
	}
}
