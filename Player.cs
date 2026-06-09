using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void HealthUpdatedEventHandler(bool damaged);

	[Signal]
	public delegate void CashUpdatedEventHandler(int newCash);

	[Signal]
	public delegate void CashAddedEventHandler(int amountAdded, Vector2 sourceGlobalPosition);

	[Signal]
	public delegate void CashSpentEventHandler(int amountSpent, Vector2 destinationGlobalPosition);

	public const float Speed = 100.0f;
	public const float JumpVelocity = -300.0f;
	private const float FootDustMoveThreshold = 12.0f;
	private List<IInteractable> nearbyInteractables = new();
	[Export] public float weightInKilograms = 1000.0f;
	

	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public Timer coyoteTimer;
	[Export] public Timer jumpBufferTimer;
	[Export] public HUD hud;
	[Export] public WeaponHandler weaponHandler;
	[Export] public Node bulletSpawnPoint;
	[Export] public AudioStreamPlayer2D equipWeaponSoundPlayer;
	[Export] public AudioStreamPlayer2D drinkPotionSoundPlayer;
	[Export] public AudioStreamPlayer2D damageSoundPlayer;
	[Export] public Node2D mainScene;
	private ItemData heldItem;
	private int currentFloor;
	private CpuParticles2D footDustParticles;
	private ImageTexture footDustTexture;
	private readonly Dictionary<TileSetAtlasSource, Image> tileSourceImages = new();
	private Color currentGroundDustColor = new Color(0.58f, 0.55f, 0.48f, 0.78f);

	public int maxHealth { get; private set; }

	public override void _Ready()
	{
		weaponHandler.bulletSpawnPoint = bulletSpawnPoint;
		weaponHandler.hud = hud;
		SetMeta("Health", BuffManager.Instance.currentBuffs[BuffManager.Buffs.MAX_HEALTH]);
		maxHealth = (int)GetMeta("Health");
		weaponHandler.UpdateWeapon();
		CreateFootDustParticles();
	}

	/// <summary>
	/// Add an integer amount of health to the player's health.
	/// </summary>
	public void AddHealth(int amount)
	{
		int newHealth = Mathf.Clamp(GetHealth() + amount, 0, 100);
		SetMeta("Health", newHealth);
		EmitSignal(SignalName.HealthUpdated, false);	
	}

	public void TakeDamage(int amount)
	{
		int newHealth = Mathf.Clamp(GetHealth() - amount, 0, 100);
		SetMeta("Health", newHealth);
		EmitSignal(SignalName.HealthUpdated, true);
		damageSoundPlayer.Play();
		if (GetHealth() <= 0)
        {
            ShowDeathScreen();
        }
    }

    private void ShowDeathScreen()
    {
        DeathScreen deathScreen = ResourceLoader.Load<PackedScene>("res://scenes/death_screen/death_screen.tscn").Instantiate() as DeathScreen;
		deathScreen.cashLabel.Text = $"${GetCash()}";
		deathScreen.floorLabel.Text = $"Floor {currentFloor}";
		
		var root = GetTree().Root;
		root.AddChild(deathScreen);
		GetTree().CurrentScene = deathScreen;
		mainScene.QueueFree();
    }

	public void OnNextFloorSpawn(int currentFloorNumber)
	{
		currentFloor = currentFloorNumber;
		hud.floorLabel.Text = $"Floor {currentFloorNumber}";
	}

    public void AddCash(int amount)
	{
		AddCash(amount, GlobalPosition);
	}

	public void AddCash(int amount, Vector2 sourceGlobalPosition)
	{
		int newCash = GetCash() + amount;
		SetMeta("Cash", newCash);
		EmitSignal(SignalName.CashUpdated, newCash);
		EmitSignal(SignalName.CashAdded, amount, sourceGlobalPosition);
	}

	public void TakeCash(int amount)
	{
		int newCash = GetCash() - amount;
		SetMeta("Cash", newCash);
		EmitSignal(SignalName.CashUpdated, newCash);
	}

	public void TakeCash(int amount, Vector2 destinationGlobalPosition)
	{
		TakeCash(amount);
		EmitSignal(SignalName.CashSpent, amount, destinationGlobalPosition);
	}

	public int GetCash()
	{
		return (int) GetMeta("Cash");
	}

	public int GetHealth()
	{
		return (int) GetMeta("Health");
	}

	public void UpdateAndDisplayInteractTooltipLootbox(Lootbox lb)
	{
		if (lb.taken)
		{
			hud.interactTooltip.Visible = false;
			return;
		}
		if (lb.opened)
		{
			hud.interactTooltip.Text = $"Press E to pick up {lb.item.ItemName}";
		}
		else
		{
			hud.interactTooltip.Text = $"Press E to open lootbox";
		}
		hud.interactTooltip.Visible = true;
	}

	public void OnInteractionAreaAreaEntered(Area2D area)
	{
		if (area.IsInGroup("interactable"))
		{
			hud.interactTooltip.Text = "Press E to interact";
			if (area.IsInGroup("lootbox"))
			{
				nearbyInteractables.Add(area.GetParent() as IInteractable);
				UpdateAndDisplayInteractTooltipLootbox(area.GetParent() as Lootbox);
			} else if (area.IsInGroup("vending"))
			{
				nearbyInteractables.Add(area as IInteractable);
				VendingMachine vendingMachine = area as VendingMachine;
				vendingMachine.UpdatePlayerToolTip(this);
				hud.interactTooltip.Visible = true;
			}
			else
			{
				nearbyInteractables.Add(area.GetParent() as IInteractable);
				hud.interactTooltip.Visible = true;
			}
		}
	}

	public void OnInteractionAreaAreaExited(Area2D area)
	{
		if (area.IsInGroup("interactable"))
		{
			if (area.IsInGroup("vending"))
			{
				nearbyInteractables.Remove(area as IInteractable);
			} else
			{
				nearbyInteractables.Remove(area.GetParent() as IInteractable);
			}
		}
		if (nearbyInteractables.Count <= 0)
		{
			hud.interactTooltip.Visible = false;
		}
	}

	public bool UseItem(ItemData item)
	{
		foreach (var effect in item.Effects)
		{
			if (!effect.Apply(this, item))
			{
				return false;
			}
		}
		return true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("interact"))
		{
			if (nearbyInteractables.Count > 0)
			{
				var interactable = nearbyInteractables[0];
				var receivedItem = interactable.Interact(this);
				if (interactable is Lootbox lb)
				{
					UpdateAndDisplayInteractTooltipLootbox(lb);
				}
				if (receivedItem is not null) 
				{
					heldItem = receivedItem;
					hud.UpdateHeldItem(receivedItem);
				}
			}
		}

		if (@event.IsActionPressed("use_item"))
		{
			if (heldItem is null) return;

			if (UseItem(heldItem))
			{
				heldItem = null;
				hud.UpdateHeldItem(heldItem);
			}
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		bool hasWeapon = weaponHandler.HasWeapon();
		string runAnimation = hasWeapon ? "run_weapon" : "run_no_weapon";
		string idleAnimation = hasWeapon ? "idle_weapon" : "idle_no_weapon";
		Vector2 velocity = Velocity;
		bool wasOnFloor = IsOnFloor();

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump"))
		{
			jumpBufferTimer.Start();
		}
		if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
		{
			velocity.Y *= 0.25f;
		}

		if (!jumpBufferTimer.IsStopped() && (!coyoteTimer.IsStopped() || IsOnFloor()))
		{
			velocity.Y = JumpVelocity;
			jumpBufferTimer.Stop();
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
		if (hasWeapon)
		{
			animatedSprite2D.FlipH = GetGlobalMousePosition().X < GlobalPosition.X;
		}
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			if (!hasWeapon)
			{
				if (direction.X < 0)
				{
					animatedSprite2D.FlipH = true;
				} else if (direction.X > 0)
				{
					animatedSprite2D.FlipH = false;
				}
			} 
			animatedSprite2D.Play(runAnimation);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			animatedSprite2D.Play(idleAnimation);
		}

		Velocity = velocity;
		MoveAndSlide();
		UpdateFootDust(direction, wasOnFloor);

		if (IsOnFloor() && !wasOnFloor)
		{
			coyoteTimer.Stop();
		}
		else if (!IsOnFloor() && wasOnFloor)
		{
			coyoteTimer.Start();
		}
	}

	private void CreateFootDustParticles()
	{
		footDustTexture = CreateFootDustTexture();
		Gradient alphaRamp = new Gradient();
		alphaRamp.SetColor(0, new Color(1.0f, 1.0f, 1.0f, 0.75f));
		alphaRamp.SetColor(1, new Color(1.0f, 1.0f, 1.0f, 0.0f));

		Curve scaleCurve = new Curve();
		scaleCurve.AddPoint(Vector2.Zero);
		scaleCurve.AddPoint(new Vector2(0.35f, 1.0f));
		scaleCurve.AddPoint(new Vector2(1.0f, 0.18f));

		footDustParticles = new CpuParticles2D
		{
			Name = "FootDustParticles",
			Amount = 22,
			Texture = footDustTexture,
			Lifetime = 0.38,
			OneShot = false,
			Preprocess = 0.0,
			SpeedScale = 1.0,
			Explosiveness = 0.24f,
			Randomness = 0.72f,
			LifetimeRandomness = 0.35f,
			LocalCoords = false,
			DrawOrder = CpuParticles2D.DrawOrderEnum.Lifetime,
			EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
			EmissionRectExtents = new Vector2(4.0f, 1.1f),
			Direction = Vector2.Up,
			Spread = 84.0f,
			Gravity = new Vector2(0.0f, 16.0f),
			InitialVelocityMin = 3.5f,
			InitialVelocityMax = 18.0f,
			LinearAccelMin = -5.0f,
			LinearAccelMax = 2.0f,
			DampingMin = 14.0f,
			DampingMax = 26.0f,
			ScaleAmountMin = 0.72f,
			ScaleAmountMax = 1.65f,
			ScaleAmountCurve = scaleCurve,
			ColorRamp = alphaRamp,
			Color = currentGroundDustColor,
			ZIndex = 0,
			Emitting = false,
		};

		AddChild(footDustParticles);
		footDustParticles.Position = new Vector2(0.0f, 8.0f);
	}

	private static ImageTexture CreateFootDustTexture()
	{
		Image image = Image.CreateEmpty(4, 4, false, Image.Format.Rgba8);
		image.Fill(new Color(1.0f, 1.0f, 1.0f, 0.0f));

		Color softWhite = Colors.White;
		image.SetPixel(1, 0, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(2, 0, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(0, 1, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(1, 1, softWhite);
		image.SetPixel(2, 1, softWhite);
		image.SetPixel(3, 1, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(0, 2, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(1, 2, softWhite);
		image.SetPixel(2, 2, softWhite);
		image.SetPixel(3, 2, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(1, 3, new Color(1.0f, 1.0f, 1.0f, 0.45f));
		image.SetPixel(2, 3, new Color(1.0f, 1.0f, 1.0f, 0.45f));

		return ImageTexture.CreateFromImage(image);
	}

	private void UpdateFootDust(Vector2 inputDirection, bool wasOnFloor)
	{
		if (footDustParticles == null)
		{
			return;
		}

		bool isWalkingOnGround = IsOnFloor() && Mathf.Abs(Velocity.X) > FootDustMoveThreshold && inputDirection.X != 0.0f;
		footDustParticles.GlobalPosition = GlobalPosition + new Vector2(0.0f, 8.0f);
		footDustParticles.Emitting = isWalkingOnGround;

		if (!isWalkingOnGround)
		{
			return;
		}

		footDustParticles.Direction = new Vector2(-Mathf.Sign(Velocity.X) * 0.65f, -0.35f).Normalized();
		Color sampledColor = GetGroundColorAt(GlobalPosition + new Vector2(0.0f, 9.0f));
		currentGroundDustColor = currentGroundDustColor.Lerp(sampledColor, wasOnFloor ? 0.35f : 1.0f);
		footDustParticles.Color = currentGroundDustColor;
	}

	private Color GetGroundColorAt(Vector2 globalPosition)
	{
		TileMapLayer groundLayer = FindGroundTileLayer(globalPosition);
		if (groundLayer == null || groundLayer.TileSet == null)
		{
			return currentGroundDustColor;
		}

		Vector2I cell = groundLayer.LocalToMap(groundLayer.ToLocal(globalPosition));
		int sourceId = groundLayer.GetCellSourceId(cell);
		if (sourceId < 0)
		{
			return currentGroundDustColor;
		}

		TileSetSource source = groundLayer.TileSet.GetSource(sourceId);
		if (source is not TileSetAtlasSource atlasSource)
		{
			return currentGroundDustColor;
		}

		Image sourceImage = GetTileSourceImage(atlasSource);
		if (sourceImage == null)
		{
			return currentGroundDustColor;
		}

		Vector2I atlasCoords = groundLayer.GetCellAtlasCoords(cell);
		Rect2I textureRegion = atlasSource.GetTileTextureRegion(atlasCoords, 0);
		Vector2I samplePosition = textureRegion.Position + textureRegion.Size / 2;
		Color tileColor = sourceImage.GetPixel(
			Mathf.Clamp(samplePosition.X, 0, sourceImage.GetWidth() - 1),
			Mathf.Clamp(samplePosition.Y, 0, sourceImage.GetHeight() - 1));

		return BoostDustColor(tileColor);
	}

	private TileMapLayer FindGroundTileLayer(Vector2 globalPosition)
	{
		Node searchRoot = GetTree().CurrentScene ?? GetTree().Root;
		return FindGroundTileLayer(searchRoot, globalPosition);
	}

	private TileMapLayer FindGroundTileLayer(Node node, Vector2 globalPosition)
	{
		if (node is TileMapLayer tileMapLayer && tileMapLayer.Name.ToString().ToLowerInvariant().Contains("background"))
		{
			Vector2I cell = tileMapLayer.LocalToMap(tileMapLayer.ToLocal(globalPosition));
			if (tileMapLayer.GetCellSourceId(cell) >= 0)
			{
				return tileMapLayer;
			}
		}

		foreach (Node child in node.GetChildren())
		{
			TileMapLayer result = FindGroundTileLayer(child, globalPosition);
			if (result != null)
			{
				return result;
			}
		}

		return null;
	}

	private Image GetTileSourceImage(TileSetAtlasSource atlasSource)
	{
		if (tileSourceImages.TryGetValue(atlasSource, out Image image))
		{
			return image;
		}

		Texture2D texture = atlasSource.Texture;
		if (texture == null)
		{
			return null;
		}

		image = texture.GetImage();
		tileSourceImages[atlasSource] = image;
		return image;
	}

	private static Color BoostDustColor(Color color)
	{
		float maxChannel = Mathf.Max(color.R, Mathf.Max(color.G, color.B));
		float boost = maxChannel < 0.42f ? 1.35f : 1.12f;
		return new Color(
			Mathf.Clamp(color.R * boost + 0.08f, 0.0f, 1.0f),
			Mathf.Clamp(color.G * boost + 0.08f, 0.0f, 1.0f),
			Mathf.Clamp(color.B * boost + 0.08f, 0.0f, 1.0f),
			0.78f);
	}
}
