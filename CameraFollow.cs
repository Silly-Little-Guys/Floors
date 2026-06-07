using Godot;

public partial class CameraFollow : Camera2D
{
	[Export] public FloorSpawner floorSpawner;
	[Export] public float floorPadding = 32.0f;
	[Export] public float transitionDuration = 0.75f;
	[Export] public float flyArcPixels = 28.0f;
	[Export] public float zoomOutPunch = 0.12f;
	[Export] public float minZoom = 0.25f;
	[Export] public float maxZoom = 8.0f;

	private Floor currentFloor;
	private Tween activeTween;
	private Vector2 lastViewportSize;
	private bool hasFramedFloor = false;

	public override void _Ready()
	{
		IgnoreRotation = true;
		GlobalRotation = 0.0f;
		lastViewportSize = GetViewportRect().Size;

		if (floorSpawner == null)
		{
			floorSpawner = GetNodeOrNull<FloorSpawner>("../floor_spawner");
		}

		if (floorSpawner != null)
		{
			floorSpawner.CurrentFloorChanged += OnCurrentFloorChanged;
			OnCurrentFloorChanged(floorSpawner.GetCurrentFloor());
		}
	}

	public override void _ExitTree()
	{
		if (floorSpawner != null)
		{
			floorSpawner.CurrentFloorChanged -= OnCurrentFloorChanged;
		}
	}

	public override void _Process(double delta)
	{
		GlobalRotation = 0.0f;

		Vector2 viewportSize = GetViewportRect().Size;
		if (currentFloor != null && viewportSize != lastViewportSize)
		{
			lastViewportSize = viewportSize;
			FrameFloor(currentFloor, true);
		}
	}

	private void OnCurrentFloorChanged(Floor floor)
	{
		if (floor == null || !IsInstanceValid(floor))
		{
			return;
		}

		currentFloor = floor;
		FrameFloor(currentFloor, hasFramedFloor);
		hasFramedFloor = true;
	}

	private void FrameFloor(Floor floor, bool animated)
	{
		if (!TryGetFloorFrame(floor, out Vector2 targetPosition, out Vector2 targetZoom))
		{
			return;
		}

		if (activeTween != null && activeTween.IsValid())
		{
			activeTween.Kill();
			activeTween = null;
		}

		if (!animated || transitionDuration <= 0.0f)
		{
			GlobalPosition = targetPosition;
			Zoom = targetZoom;
			return;
		}

		Vector2 startPosition = GlobalPosition;
		Vector2 startZoom = Zoom;
		Vector2 travel = targetPosition - startPosition;
		Vector2 arcDirection = travel.LengthSquared() > 0.01f
			? new Vector2(-travel.Normalized().Y, travel.Normalized().X)
			: Vector2.Right;
		Vector2 controlPosition = startPosition.Lerp(targetPosition, 0.5f) + arcDirection * flyArcPixels;

		activeTween = CreateTween();
		activeTween.SetProcessMode(Tween.TweenProcessMode.Idle);
		activeTween.TweenMethod(Callable.From<float>((progress) =>
		{
			float easedProgress = EaseOutCubic(progress);
			GlobalPosition = Bezier(startPosition, controlPosition, targetPosition, easedProgress);

			Vector2 zoom = startZoom.Lerp(targetZoom, easedProgress);
			float punch = 1.0f - (Mathf.Sin(progress * Mathf.Pi) * zoomOutPunch);
			Zoom = zoom * punch;
			GlobalRotation = 0.0f;
		}), 0.0f, 1.0f, transitionDuration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);

		activeTween.Finished += () =>
		{
			GlobalPosition = targetPosition;
			Zoom = targetZoom;
			GlobalRotation = 0.0f;
			activeTween = null;
		};
	}

	private bool TryGetFloorFrame(Floor floor, out Vector2 targetPosition, out Vector2 targetZoom)
	{
		targetPosition = Vector2.Zero;
		targetZoom = Vector2.One;

		if (!floor.TryGetFloorHorizontalBounds(out float leftX, out float rightX, out _)
			|| !floor.TryGetFloorVerticalBounds(out float topY, out float bottomY, out _))
		{
			return false;
		}

		Vector2 viewportSize = GetViewportRect().Size;
		if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
		{
			return false;
		}

		float paddedWidth = Mathf.Max((rightX - leftX) + floorPadding * 2.0f, 1.0f);
		float paddedHeight = Mathf.Max((bottomY - topY) + floorPadding * 2.0f, 1.0f);
		float zoom = Mathf.Min(viewportSize.X / paddedWidth, viewportSize.Y / paddedHeight);
		zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

		targetPosition = new Vector2((leftX + rightX) / 2.0f, (topY + bottomY) / 2.0f);
		targetZoom = new Vector2(zoom, zoom);
		return true;
	}

	private static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float progress)
	{
		float inverse = 1.0f - progress;
		return inverse * inverse * start + 2.0f * inverse * progress * control + progress * progress * end;
	}

	private static float EaseOutCubic(float value)
	{
		float inverse = 1.0f - value;
		return 1.0f - inverse * inverse * inverse;
	}
}
