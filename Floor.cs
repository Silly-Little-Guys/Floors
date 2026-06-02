using Godot;
using System;

public partial class Floor : Node2D
{
	[Export] public TileMapLayer mainTileMapLayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Attempts to calculate the global vertical bounds of a floor from its main tile map layer.
	/// </summary>
	/// <param name="floor">The floor whose main tile map layer should be measured.</param>
	/// <param name="topY">The highest global Y position occupied by the floor when the calculation succeeds; otherwise 0.</param>
	/// <param name="bottomY">The lowest global Y position occupied by the floor when the calculation succeeds; otherwise 0.</param>
	/// <param name="tileHeight">The global height of one tile in the floor's tile map layer when the calculation succeeds; otherwise 0.</param>
	/// <returns>True if the floor has a valid main tile map layer, tile set, and used cells; otherwise false.</returns>
	public bool TryGetFloorVerticalBounds(out float topY, out float bottomY, out float tileHeight)
	{
		topY = 0;
		bottomY = 0;
		tileHeight = 0;

		if (this.mainTileMapLayer == null)
		{
			GD.PushWarning("Cannot calculate floor bounds because the floor has no main TileMapLayer assigned.");
			return false;
		}

		TileMapLayer tileMapLayer = this.mainTileMapLayer;

		if (tileMapLayer.TileSet == null)
		{
			GD.PushWarning("Cannot calculate floor bounds because the main TileMapLayer has no TileSet.");
			return false;
		}

		Rect2I usedRect = tileMapLayer.GetUsedRect();

		if (usedRect.Size == Vector2I.Zero)
		{
			GD.PushWarning("Cannot calculate floor bounds because the main TileMapLayer has no used cells.");
			return false;
		}

		Vector2I tileSize = tileMapLayer.TileSet.TileSize;
		tileHeight = Mathf.Abs(tileMapLayer.ToGlobal(new Vector2(0, tileSize.Y)).Y - tileMapLayer.ToGlobal(Vector2.Zero).Y);
		Vector2 localTopLeft = new Vector2(usedRect.Position.X * tileSize.X, usedRect.Position.Y * tileSize.Y);
		Vector2 localBottomRight = new Vector2(usedRect.End.X * tileSize.X, usedRect.End.Y * tileSize.Y);

		Vector2[] corners =
		{
			localTopLeft,
			new Vector2(localBottomRight.X, localTopLeft.Y),
			localBottomRight,
			new Vector2(localTopLeft.X, localBottomRight.Y)
		};

		topY = float.PositiveInfinity;
		bottomY = float.NegativeInfinity;

		foreach (Vector2 corner in corners)
		{
			float globalY = tileMapLayer.ToGlobal(corner).Y;
			topY = Mathf.Min(topY, globalY);
			bottomY = Mathf.Max(bottomY, globalY);
		}

		return true;
	}
}
