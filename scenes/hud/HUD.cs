using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

public partial class HUD : CanvasLayer
{
	[Export] public Player player;
	[Export] public ProgressBar healthBar;
	[Export] public Label ammoLabel;
	[Export] public AnimationPlayer damageAnimation;
	[Export] public AnimationPlayer lowHealthAnimation;
	[Export] public Label cashLabel;
	[Export] public Label floorLabel;
	[Export] public CompressedTexture2D cashImage;
	[Export] public TextureRect itemTextureRect;
	[Export] public Label itemTooltip;
	[Export] public Label interactTooltip;
	[Export] public Label healthErrortip;
	[Export] public Control pauseScreen;
	private bool paused = false;

	private const int MaxCashSprites = 50;
	private const float CashSpriteSize = 32.0f;
	private const float CashBurstDuration = 0.95f;
	private const float CashLabelPulseDuration = 0.25f;
	private static readonly Color CashGainTint = Colors.White;
	private static readonly Color CashSpentTint = new Color(1.0f, 0.22f, 0.2f, 1.0f);

	private const int LowHealthThreshold = 50;

	private readonly List<CashAnimation> cashAnimations = new();
	private readonly RandomNumberGenerator random = new();
	private float cashLabelPulseTimer = 0.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		random.Randomize();
		healthBar.Value = player.GetHealth();
		OnPlayerCashUpdated(player.GetCash());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateCashAnimations((float)delta);
		UpdateCashLabelPulse((float)delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			paused = !paused;
			pauseScreen.Visible = paused;
			GetTree().Paused = paused;
		}
	}

	public void OnPlayerHealthUpdated(bool damaged)
	{
		int health = player.GetHealth();
		healthBar.Value = health;
		if (damaged) damageAnimation.Play("damage_flash");

		if (health <= LowHealthThreshold) 
		{
			lowHealthAnimation.Play("low_health");
		}
		else
		{
			lowHealthAnimation.Play("RESET");
		}
	}

	public void OnPlayerCashUpdated(int newAmount)
	{
		cashLabel.Text = $"${newAmount.ToString("N0", CultureInfo.InvariantCulture)}";
	}

	public void OnPlayerCashAdded(int amount, Vector2 sourceGlobalPosition)
	{
		SpawnCashBurst(amount, WorldToHudPosition(sourceGlobalPosition), cashLabel.GetGlobalRect().GetCenter(), CashGainTint, true);
		cashLabelPulseTimer = CashLabelPulseDuration;
	}

	public void OnPlayerCashSpent(int amount, Vector2 destinationGlobalPosition)
	{
		SpawnCashBurst(amount, cashLabel.GetGlobalRect().GetCenter(), WorldToHudPosition(destinationGlobalPosition), CashSpentTint, false);
	}

	private void SpawnCashBurst(int amount, Vector2 sourcePosition, Vector2 endPosition, Color tint, bool burstFirst)
	{
		if (cashImage == null || cashLabel == null)
		{
			return;
		}

		int spriteCount = Mathf.Clamp(amount, 1, MaxCashSprites);

		for (int i = 0; i < spriteCount; i++)
		{
			TextureRect cashSprite = new TextureRect
			{
				Texture = cashImage,
				Size = new Vector2(CashSpriteSize, CashSpriteSize),
				PivotOffset = new Vector2(CashSpriteSize, CashSpriteSize) / 2.0f,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ZIndex = 100,
				Modulate = new Color(tint.R, tint.G, tint.B, 0.0f),
				Scale = Vector2.Zero,
			};

			float angle = random.RandfRange(0.0f, Mathf.Pi * 2.0f);
			Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			Vector2 startPosition = sourcePosition + direction * random.RandfRange(0.0f, burstFirst ? 18.0f : 10.0f);
			Vector2 burstPosition = burstFirst
				? sourcePosition + direction * random.RandfRange(50.0f, 115.0f)
				: sourcePosition + direction * random.RandfRange(18.0f, 46.0f);
			Vector2 midpoint = (burstPosition + endPosition) / 2.0f;
			Vector2 arcLift = Vector2.Up * random.RandfRange(burstFirst ? 70.0f : 35.0f, burstFirst ? 150.0f : 95.0f);
			Vector2 arcSide = new Vector2(-direction.Y, direction.X) * random.RandfRange(-45.0f, 45.0f);

			AddChild(cashSprite);
			cashSprite.Position = startPosition - cashSprite.PivotOffset;

			cashAnimations.Add(new CashAnimation
			{
				Sprite = cashSprite,
				StartPosition = startPosition,
				BurstPosition = burstPosition,
				ControlPosition = midpoint + arcLift + arcSide,
				EndPosition = endPosition,
				Delay = i * 0.015f + random.RandfRange(0.0f, 0.08f),
				Duration = (burstFirst ? CashBurstDuration : 0.82f) + random.RandfRange(-0.08f, 0.16f),
				StartRotation = random.RandfRange(-0.9f, 0.9f),
				EndRotation = random.RandfRange(-Mathf.Pi * 3.0f, Mathf.Pi * 3.0f),
				PeakScale = random.RandfRange(burstFirst ? 1.25f : 0.95f, burstFirst ? 1.8f : 1.35f),
				Tint = tint,
			});
		}
	}

	private Vector2 WorldToHudPosition(Vector2 worldPosition)
	{
		return GetViewport().GetCanvasTransform() * worldPosition;
	}

	private void UpdateCashAnimations(float delta)
	{
		for (int i = cashAnimations.Count - 1; i >= 0; i--)
		{
			CashAnimation cashAnimation = cashAnimations[i];
			cashAnimation.Elapsed += delta;

			float rawProgress = (cashAnimation.Elapsed - cashAnimation.Delay) / cashAnimation.Duration;
			if (rawProgress < 0.0f)
			{
				cashAnimations[i] = cashAnimation;
				continue;
			}

			if (rawProgress >= 1.0f)
			{
				cashAnimation.Sprite.QueueFree();
				cashAnimations.RemoveAt(i);
				continue;
			}

			float progress = Mathf.Clamp(rawProgress, 0.0f, 1.0f);
			float scatterEnd = 0.28f;
			Vector2 position;
			float scale;
			float alpha;

			if (progress < scatterEnd)
			{
				float scatterProgress = progress / scatterEnd;
				float easedScatter = EaseOutBack(scatterProgress);
				position = cashAnimation.StartPosition.Lerp(cashAnimation.BurstPosition, easedScatter);
				scale = Mathf.Lerp(0.1f, cashAnimation.PeakScale, EaseOutBack(scatterProgress));
				alpha = EaseOutCubic(Mathf.Clamp(scatterProgress * 2.0f, 0.0f, 1.0f));
			}
			else
			{
				float flyProgress = (progress - scatterEnd) / (1.0f - scatterEnd);
				float easedFly = EaseInCubic(flyProgress);
				position = QuadraticBezier(cashAnimation.BurstPosition, cashAnimation.ControlPosition, cashAnimation.EndPosition, easedFly);
				scale = Mathf.Lerp(cashAnimation.PeakScale, 0.45f, EaseOutCubic(flyProgress));
				alpha = 1.0f - EaseInCubic(Mathf.Clamp((flyProgress - 0.82f) / 0.18f, 0.0f, 1.0f));
			}

			cashAnimation.Sprite.Position = position - cashAnimation.Sprite.PivotOffset;
			cashAnimation.Sprite.Scale = Vector2.One * scale;
			cashAnimation.Sprite.Rotation = Mathf.Lerp(cashAnimation.StartRotation, cashAnimation.EndRotation, EaseOutCubic(progress));
			cashAnimation.Sprite.Modulate = new Color(cashAnimation.Tint.R, cashAnimation.Tint.G, cashAnimation.Tint.B, alpha);
			cashAnimations[i] = cashAnimation;
		}
	}

	private void UpdateCashLabelPulse(float delta)
	{
		if (cashLabel == null)
		{
			return;
		}

		if (cashLabelPulseTimer <= 0.0f)
		{
			cashLabel.Scale = Vector2.One;
			return;
		}

		cashLabel.PivotOffset = cashLabel.Size / 2.0f;
		cashLabelPulseTimer = Mathf.Max(cashLabelPulseTimer - delta, 0.0f);
		float progress = 1.0f - cashLabelPulseTimer / CashLabelPulseDuration;
		float pulse = Mathf.Sin(progress * Mathf.Pi);
		cashLabel.Scale = Vector2.One * (1.0f + pulse * 0.18f);
	}

	private static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float weight)
	{
		return start.Lerp(control, weight).Lerp(control.Lerp(end, weight), weight);
	}

	private static float EaseOutCubic(float weight)
	{
		return 1.0f - Mathf.Pow(1.0f - weight, 3.0f);
	}

	private static float EaseInCubic(float weight)
	{
		return weight * weight * weight;
	}

	private static float EaseOutBack(float weight)
	{
		const float backAmount = 1.70158f;
		float shiftedWeight = weight - 1.0f;
		return 1.0f + (backAmount + 1.0f) * Mathf.Pow(shiftedWeight, 3.0f) + backAmount * Mathf.Pow(shiftedWeight, 2.0f);
	}

	private struct CashAnimation
	{
		public TextureRect Sprite;
		public Vector2 StartPosition;
		public Vector2 BurstPosition;
		public Vector2 ControlPosition;
		public Vector2 EndPosition;
		public float Elapsed;
		public float Delay;
		public float Duration;
		public float StartRotation;
		public float EndRotation;
		public float PeakScale;
		public Color Tint;
	}

	public void UpdateHeldItem(ItemData item)
	{
		if (item is null)
		{
			itemTextureRect.Texture = null;
			itemTooltip.Visible = false;
			return;
		}
		itemTooltip.Text = $"Press F to equip/use {item.ItemName}";
		itemTooltip.Visible = true;
		itemTextureRect.Texture = item.ItemTexture;
	}

	public async void FlashHealthErrortip()
	{
		healthErrortip.Visible = true;
		await ToSignal(GetTree().CreateTimer(2f), SceneTreeTimer.SignalName.Timeout);
		healthErrortip.Visible = false;
	}

	/// <summary>
	/// Updates the ammo label text to the integer passed as a parameter.
	/// </summary>
	public void SetAmmoDisplay(int ammo)
	{
		ammoLabel.Text = ammo.ToString();
	}

	public void OnAmmoCountUpdated()
	{
		
	}

	public void OnGunEquipped()
	{
		
	}

	public void OnGunUnequipped()
	{
			
	}
}
