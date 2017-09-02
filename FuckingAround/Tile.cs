using System;
using System.Collections.Generic;

namespace srpg {
	public class Tile {
		private Being _inhabitant;
		public Being Inhabitant {
			get { return _inhabitant; }
			set {
				if (_inhabitant != null && value != null) {
					if (_inhabitant == value) return;
					else throw new ArgumentException("Tile is occupied."); }
				_inhabitant = value;
				if(value != null) value.Place = this;
			}
		}
		private ChannelingInstance _channelingInstance;
		public ChannelingInstance ChannelingInstance {
			get { return _channelingInstance; }
			set {
				if (_channelingInstance != null && value != null) {
					if (_channelingInstance == value) return;
					else throw new ArgumentException("Tile is occupied.");
				}
				_channelingInstance = value;
				if (value != null) value.Place = this;
			}
		}
		public int X;
		public int Y;
		public int Height { get; protected set; }
		public int TraverseCost;
		private TileSet Owner;
		public IEnumerable<Tile> GetArea(int range) {
			return Owner.GetArea(this, range);
		}

		public Tile North	{ get { return Y+1 < Owner.YLength ? Owner[X, Y+1] : null; } }
		public Tile East	{ get { return X+1 < Owner.XLength ? Owner[X+1, Y] : null; } }
		public Tile West	{ get { return X-1 >= 0 ? Owner[X-1, Y] : null; } }
		public Tile South	{ get { return Y-1 >= 0 ? Owner[X, Y-1] : null; } }
		public Tile GetAdjacent(Cardinal dir) {
			switch (dir) {
				case Cardinal.North: return North;
				case Cardinal.East: return East;
				case Cardinal.South: return South;
				case Cardinal.West: return West;
			}
			throw new ArgumentException("Unhandled cardinal");
		}

		public IEnumerable<Tile> Adjacent {
			get {
				if (X - 1 >= 0) yield return Owner[X - 1, Y];
				if (X + 1 < Owner.XLength) yield return Owner[X + 1, Y];
				if (Y - 1 >= 0) yield return Owner[X, Y - 1];
				if (Y + 1 < Owner.YLength) yield return Owner[X, Y + 1];
			}
		}

		public override string ToString() {
			return "(X: " + X + ", Y: " + Y + ")";
		}

		public Tile(int x, int y, TileSet owner) {
			Inhabitant = null;
			X = x;
			Y = y;
			Owner = owner;
		}

		public Tile(int x, int y, TileSet owner, int height)
			: this(x, y, owner) {
			Height = height;
		}
	}
}