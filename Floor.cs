using Godot;
using System;
using System.Collections.Generic;

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

	/// <summary>
	/// Attempts to calculate the global horizontal bounds of a floor from its main tile map layer.
	/// </summary>
	/// <param name="leftX">The leftmost global X position occupied by the floor when the calculation succeeds; otherwise 0.</param>
	/// <param name="rightX">The rightmost global X position occupied by the floor when the calculation succeeds; otherwise 0.</param>
	/// <param name="tileWidth">The global width of one tile in the floor's tile map layer when the calculation succeeds; otherwise 0.</param>
	/// <returns>True if the floor has a valid main tile map layer, tile set, and used cells; otherwise false.</returns>
	public bool TryGetFloorHorizontalBounds(out float leftX, out float rightX, out float tileWidth)
	{
		leftX = 0;
		rightX = 0;
		tileWidth = 0;

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
		tileWidth = Mathf.Abs(tileMapLayer.ToGlobal(new Vector2(tileSize.X, 0)).X - tileMapLayer.ToGlobal(Vector2.Zero).X);
		Vector2 localTopLeft = new Vector2(usedRect.Position.X * tileSize.X, usedRect.Position.Y * tileSize.Y);
		Vector2 localBottomRight = new Vector2(usedRect.End.X * tileSize.X, usedRect.End.Y * tileSize.Y);

		Vector2[] corners =
		{
			localTopLeft,
			new Vector2(localBottomRight.X, localTopLeft.Y),
			localBottomRight,
			new Vector2(localTopLeft.X, localBottomRight.Y)
		};

		leftX = float.PositiveInfinity;
		rightX = float.NegativeInfinity;

		foreach (Vector2 corner in corners)
		{
			float globalX = tileMapLayer.ToGlobal(corner).X;
			leftX = Mathf.Min(leftX, globalX);
			rightX = Mathf.Max(rightX, globalX);
		}

		return true;
	}

	/// <summary>
	/// Attempts to find a random tile center that is inside this floor's tile map and has no TileSet collision polygons.
	/// </summary>
	/// <param name="spawnPosition">A global position safe for spawning when the calculation succeeds; otherwise Vector2.Zero.</param>
	/// <returns>True if at least one non-colliding used cell exists; otherwise false.</returns>
	public bool TryGetRandomNonCollidingSpawnPosition(out Vector2 spawnPosition)
	{
		spawnPosition = Vector2.Zero;

		if (this.mainTileMapLayer == null)
		{
			GD.PushWarning("Cannot find spawn position because the floor has no main TileMapLayer assigned.");
			return false;
		}

		TileMapLayer tileMapLayer = this.mainTileMapLayer;

		if (tileMapLayer.TileSet == null)
		{
			GD.PushWarning("Cannot find spawn position because the main TileMapLayer has no TileSet.");
			return false;
		}

		List<Vector2I> spawnableCells = new();
		Rect2I usedRect = tileMapLayer.GetUsedRect();
		int highestTileY = usedRect.Position.Y;
		int lowestTileY = usedRect.End.Y - 1;

		foreach (Vector2I cell in tileMapLayer.GetUsedCells())
		{
			if (cell.Y == highestTileY || cell.Y == lowestTileY)
			{
				continue;
			}

			TileData tileData = tileMapLayer.GetCellTileData(cell);

			if (tileData == null || CellHasCollision(tileData, tileMapLayer.TileSet))
			{
				continue;
			}

			spawnableCells.Add(cell);
		}

		if (spawnableCells.Count == 0)
		{
			GD.PushWarning("Cannot find spawn position because the floor has no non-colliding used cells.");
			return false;
		}

		Vector2I spawnCell = spawnableCells[GD.RandRange(0, spawnableCells.Count - 1)];
		spawnPosition = tileMapLayer.ToGlobal(tileMapLayer.MapToLocal(spawnCell));
		return true;
	}

	private static bool CellHasCollision(TileData tileData, TileSet tileSet)
	{
		for (int layerId = 0; layerId < tileSet.GetPhysicsLayersCount(); layerId++)
		{
			if (tileData.GetCollisionPolygonsCount(layerId) > 0)
			{
				return true;
			}
		}

		return false;
	}
}
