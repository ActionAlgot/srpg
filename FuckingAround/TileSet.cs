using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace srpg {
	public class TileClickedEventArgs : EventArgs {
		public Tile Tile;
		public TileClickedEventArgs(Tile tile) {
			Tile = tile;
		}
	}
	[Serializable]
	public class TileSet : IDeserializationCallback {
		private Tile[,] Tiles;
		public Tile this[int x, int y]{
			get { return Tiles[x, y]; }
		}
		public int XLength { get { return Tiles.GetLength(0); } }
		public int YLength { get { return Tiles.GetLength(1); } }
		//public event EventHandler<TileClickedEventArgs> TileClicked;

		public TileSet(int x, int y) {
			Tiles = new Tile[x, y];
			for (int ix = 0; ix < x; ix++)
				for (int iy = 0; iy < y; iy++)
					Tiles[ix, iy] = new Tile(ix, iy, this);
			foreach (var tile in Tiles) tile.TraverseCost = 1;
		}

		public TileSet(int x, int y, int mHeight) {
			Random rand = new Random();
			Tiles = new Tile[x, y];
			for (int ix = 0; ix < x; ix++)
				for (int iy = 0; iy < y; iy++)
					Tiles[ix, iy] = new Tile(ix, iy, this, rand.Next(0, mHeight));
			foreach (var tile in Tiles) tile.TraverseCost = 1;
		}

		//public void SelectTile(Tile tile) {
		//	TileClicked(this, new TileClickedEventArgs(tile));
		//}

		public IEnumerable<Tile> GetArea(Tile start, int range) {
			
			int RightCut = start.X + range < XLength ? start.X + range : (XLength - 1);
			int LeftCut = start.X - range > 0 ? start.X - range : 0;
			int TopCut = start.Y + range;	//out of range for y is handled in loop
			int BottomCut = start.Y - range + 1;

			for (int x = LeftCut; x <= RightCut; x++) {
				int xoff = Math.Abs(start.X - x);
				for (int y = BottomCut + xoff > 0 ? BottomCut + xoff : 0;
						 y < (TopCut - xoff < YLength ? TopCut - xoff: YLength);
						 y++) {
					yield return Tiles[x, y];
				}
			}
		}

		public IEnumerable<Tile> AsEnumerable() {
			foreach (var t in Tiles) yield return t;
		}

		public void OnDeserialization(object sender) {
			foreach (var t in Tiles)
				t.SetOwner(this);
		}
	}
}
