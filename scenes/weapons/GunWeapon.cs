using Godot;
using System;

public partial class GunWeapon : Weapon
{
	[Export] public int damage;
	[Export] public float fireRate;
	[Export] public float bulletSpeed;
	[Export] public float spreadDegrees = 2.0f;
	[Export] public PackedScene bulletScene;
	[Export] public Timer fireTimer;
	[Export] public Node bulletSpawnPoint;
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public Node2D pivotPoint;
	[Export] public RayCast2D shotDirection;
	[Export] public Player player;
	[Export] public int ammoCount = 100;
	public int maxAmmoCount;
	
	private Vector2 pivotPointStartPosition;
	private Vector2 shotDirectionStartPosition;

	public override void _Ready()
	{
		maxAmmoCount = ammoCount;
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
		if (ammoCount <= 0)
		{
			return;
		}
		ammoCount--;
		fireTimer.Start();
		float shotSpread = Mathf.DegToRad((float)GD.RandRange(-spreadDegrees, spreadDegrees));
		float shotRotation = shotDirection.GlobalRotation + shotSpread;
		Bullet b = bulletScene.Instantiate<Bullet>();
		b.damage = damage;
		b.GlobalPosition = shotDirection.GlobalPosition;
		b.Rotation = shotRotation;
		b.LinearVelocity = new Vector2(bulletSpeed, 0).Rotated(shotRotation);
		bulletSpawnPoint.AddChild(b);

		Vector2 recoilVelocity = new Vector2(-bulletSpeed * 0.90f, 0).Rotated(shotRotation);
		if (Mathf.Abs(recoilVelocity.Y) > 0.0f)
		{
			float weightScale = 70.0f / Mathf.Max(player.weightInKilograms, 1.0f);
			recoilVelocity.Y *= weightScale;
		}
		player.Velocity += recoilVelocity;

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
