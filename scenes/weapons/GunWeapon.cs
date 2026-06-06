using Godot;
using System;
using System.ComponentModel;

public partial class GunWeapon : Weapon
{	
	private Vector2 pivotPointStartPosition;
	private Vector2 shotDirectionStartPosition;

	public override void _Ready()
	{
		maxAmmoCount = ammoCount;
		fireTimer.OneShot = true;
		fireTimer.WaitTime = 1.0 / fireRate;
		pivotPointStartPosition = pivotPoint.Position;
		shotDirectionStartPosition = shotDirection.Position;
		animatedSprite2D.SpriteFrames.SetAnimationLoop("firing", false);
		animatedSprite2D.Play("idle");
	}
	public bool OnShoot()
	{
		if (!fireTimer.IsStopped())
		{
			return false;
		}
		fireTimer.Start();
		if (ammoCount <= 0)
		{
			emptySoundPlayer.Play();
			return false;
		}

		ammoCount--;
		EmitSignal(SignalName.OnAmmoCountUpdated, ammoCount);

		shootSoundPlayer.Play();

		float shotSpread = Mathf.DegToRad((float)GD.RandRange(-spreadDegrees, spreadDegrees));
		float shotRotation = shotDirection.GlobalRotation + shotSpread;
		Bullet b = bulletScene.Instantiate<Bullet>();
		b.damage = damage;
		b.GlobalPosition = shotDirection.GlobalPosition;
		b.Rotation = shotRotation;
		b.LinearVelocity = new Vector2(bulletSpeed, 0).Rotated(shotRotation);
		bulletSpawnPoint.AddChild(b);

		float weightScale = 70.0f / Mathf.Max(player.weightInKilograms, 1.0f);
		Vector2 recoilVelocity = new Vector2(-bulletSpeed * 0.90f, 0).Rotated(shotRotation) * weightScale;
		if (recoilVelocity.Y < 0.0f)
		{
			recoilVelocity.Y *= player.IsOnFloor() ? 0.0f : Mathf.Clamp(airUpwardRecoilScale, 0.0f, 1.0f);
		}
		player.Velocity += recoilVelocity;

		return true;
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
		bool firedThisFrame = false;
		if (Input.IsActionPressed("shoot"))
		{
			firedThisFrame = OnShoot();
		}
		if (firedThisFrame)
		{
			animatedSprite2D.Stop();
			animatedSprite2D.Play("firing");
		} else if (animatedSprite2D.Animation != "firing" || !animatedSprite2D.IsPlaying())
		{
			animatedSprite2D.Play("idle");   
		}
	}
}
