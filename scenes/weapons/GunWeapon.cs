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
	[Export] public RayCast2D shotDirection;
	
	private Vector2 pivotPointStartPosition;
	private Vector2 shotDirectionStartPosition;

	public override void _Ready()
	{
		fireTimer.WaitTime = 1 / fireRate;
		pivotPointStartPosition = pivotPoint.Position;
		shotDirectionStartPosition = shotDirection.Position;
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
		b.GlobalPosition = shotDirection.GlobalPosition;
		b.Rotation = shotDirection.GlobalRotation;
		b.LinearVelocity = new Vector2(bulletSpeed, 0).Rotated(shotDirection.GlobalRotation);
		bulletSpawnPoint.AddChild(b);
	}

	public override void _PhysicsProcess(double delta)
	{
		bool shouldMirror = GetGlobalMousePosition().X < GlobalPosition.X;
		animatedSprite2D.FlipV = shouldMirror;
		pivotPoint.Position = new Vector2(pivotPointStartPosition.X, shouldMirror ? -pivotPointStartPosition.Y : pivotPointStartPosition.Y);
		shotDirection.Position = new Vector2(shotDirectionStartPosition.X, shouldMirror ? -shotDirectionStartPosition.Y : shotDirectionStartPosition.Y);

		Vector2 aimPivot = pivotPoint != null ? pivotPoint.GlobalPosition : GlobalPosition;
		Vector2 aimDirection = GetGlobalMousePosition() - aimPivot;
		GlobalRotation = aimDirection.Angle();
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
