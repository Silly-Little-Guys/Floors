using Godot;
using System;

public partial class Bullet : RigidBody2D
{
	[Export] public int damage;
	[Export] public Timer t;
	private int initialSpeed;
	private bool hasImpacted;
	private static ImageTexture impactParticleTexture;

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
		if (hasImpacted)
		{
			return;
		}

		if (body is IEnemy enemy && this.LinearVelocity.Length() > initialSpeed * 0.70f)
		{
			hasImpacted = true;
			enemy.TakeDamage(damage);
			QueueFree();
			return;
		}

		if (body is not IEnemy)
		{
			hasImpacted = true;
			SpawnWallHitParticles();
			QueueFree();
		}
	}

	private void SpawnWallHitParticles()
	{
		Node parent = GetParent();
		if (parent == null)
		{
			return;
		}

		Gradient colorRamp = new Gradient();
		colorRamp.SetColor(0, new Color(1.0f, 0.92f, 0.58f, 1.0f));
		colorRamp.SetColor(1, new Color(1.0f, 0.32f, 0.16f, 0.0f));

		Curve scaleCurve = new Curve();
		scaleCurve.AddPoint(Vector2.Zero);
		scaleCurve.AddPoint(new Vector2(0.22f, 1.0f));
		scaleCurve.AddPoint(new Vector2(1.0f, 0.0f));

		Vector2 impactDirection = LinearVelocity.LengthSquared() > 0.01f ? -LinearVelocity.Normalized() : Vector2.Up;
		CpuParticles2D impactParticles = new CpuParticles2D
		{
			Name = "BulletWallImpactParticles",
			Amount = 12,
			Texture = GetImpactParticleTexture(),
			Lifetime = 0.22,
			OneShot = true,
			Explosiveness = 0.92f,
			Randomness = 0.45f,
			LifetimeRandomness = 0.35f,
			LocalCoords = false,
			DrawOrder = CpuParticles2D.DrawOrderEnum.Lifetime,
			EmissionShape = CpuParticles2D.EmissionShapeEnum.Point,
			Direction = impactDirection,
			Spread = 72.0f,
			Gravity = new Vector2(0.0f, 35.0f),
			InitialVelocityMin = 18.0f,
			InitialVelocityMax = 55.0f,
			DampingMin = 18.0f,
			DampingMax = 36.0f,
			ScaleAmountMin = 0.55f,
			ScaleAmountMax = 1.35f,
			ScaleAmountCurve = scaleCurve,
			ColorRamp = colorRamp,
			ZIndex = 4,
			GlobalPosition = GlobalPosition,
			Emitting = true,
		};

		parent.AddChild(impactParticles);
		impactParticles.Finished += impactParticles.QueueFree;
		impactParticles.Restart();
	}

	private static ImageTexture GetImpactParticleTexture()
	{
		if (impactParticleTexture != null)
		{
			return impactParticleTexture;
		}

		Image image = Image.CreateEmpty(3, 3, false, Image.Format.Rgba8);
		image.Fill(new Color(1.0f, 1.0f, 1.0f, 0.0f));
		image.SetPixel(1, 0, new Color(1.0f, 1.0f, 1.0f, 0.65f));
		image.SetPixel(0, 1, new Color(1.0f, 1.0f, 1.0f, 0.65f));
		image.SetPixel(1, 1, Colors.White);
		image.SetPixel(2, 1, new Color(1.0f, 1.0f, 1.0f, 0.65f));
		image.SetPixel(1, 2, new Color(1.0f, 1.0f, 1.0f, 0.65f));
		impactParticleTexture = ImageTexture.CreateFromImage(image);
		return impactParticleTexture;
	}
}
