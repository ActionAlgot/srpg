using System;

namespace srpg {
	public enum Cardinal { North, East, South, West }

	public static class CardinalUtilities {
		public static Cardinal GetMovementCardinal(Tile start, Tile End) {
			switch (new { x = End.X - start.X, y = End.Y - start.Y }) {
				case var obj when obj.x == 0 && obj.y == 1:
					return Cardinal.North;
				case var obj when obj.x == 1 && obj.y == 0:
					return Cardinal.East;
				case var obj when obj.x == 0 && obj.y == -1:
					return Cardinal.South;
				case var obj when obj.x == -1 && obj.y == 0:
					return Cardinal.West;
			}
			throw new ArgumentException("Argument tiles are not adjacent");
		}
	}
}