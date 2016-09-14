using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FuckingAround {
	public class TileClickedEventArgs : EventArgs {
		public MouseEventArgs MouseEventArgs;
		public Tile Tile;
		public TileClickedEventArgs(MouseEventArgs mea, Tile tile) {
			MouseEventArgs = mea;
			Tile = tile;
		}
	}

	public class TileSet {
		private Tile[,] Tiles;
		public Tile this[int x, int y]{
			get { return Tiles[x, y]; }
		}
		public Tile ClickedTile;
		public int XLength { get { return Tiles.GetLength(0); } }
		public int YLength { get { return Tiles.GetLength(1); } }
		public SolidBrush SelectedBrush = new SolidBrush(Color.Red);
		public event EventHandler<TileClickedEventArgs> TileClicked;

		public TileSet(int x, int y) {
			var defaultBrush = new SolidBrush(Color.BlanchedAlmond);
			Tiles = new Tile[x, y];
			for (int ix = 0; ix < x; ix++)
				for (int iy = 0; iy < y; iy++)
					Tiles[ix, iy] = new Tile(ix, iy, this, defaultBrush);
			foreach (var tile in Tiles) tile.TraverseCost = 1;
		}

		public bool ClickTile(MouseEventArgs e) {
			ClickedTile = SelectTile(e.X, e.Y);
			if (ClickedTile != null) {
				TileClicked(this, new TileClickedEventArgs(e, ClickedTile));
				return true;
			} else return false;
		}

		public Tile SelectTile(int x, int y) {
			if (x >= 0 && x <= Tiles.GetLength(0) * Tile.Size && y >= 0 && y <= Tiles.GetLength(1) * Tile.Size)	//within tileset area
				return Tiles[x / Tile.Size, y / Tile.Size];
			else return null;
		}

		public IEnumerable<Tile> GetTraversalArea(Tile start, Func<Tile, int> travCostCalc, int mp) {
			var accumTravCost = new Dictionary<Tile, int>();    //dictionary for accumulated traversal cost
			var tils = new LinkedList<Tile>();
			accumTravCost.Add(start, 0);
			tils.AddFirst(start);
			Action<LinkedListNode<Tile>> AddAdjTiles = (node) => {
				foreach (var adjT in node.Value.Adjacent)
					if (accumTravCost.ContainsKey(adjT) == false) {
						int adjTravCost = travCostCalc(adjT);
						if (adjTravCost >= 0) {
							int _accumTravCost = accumTravCost[node.Value] + adjTravCost;
							if (_accumTravCost <= mp) {
								accumTravCost.Add(adjT, _accumTravCost);
								var added = false;
								for (var node2 = node.Next; node2 != null; node2 = node2.Next)
									if (accumTravCost[node2.Value] >= accumTravCost[adjT]) {
										added = true;
										tils.AddBefore(node2, adjT);
										break; }
								if (!added) tils.AddLast(adjT);
			} } } };
			AddAdjTiles(tils.First);    //skip 'start'
			for (var node = tils.First.Next; node != null; node = node.Next) {
				yield return node.Value;
				AddAdjTiles(node);
			}
		}

		public IEnumerable<Tile> GetTraversalArea(Tile start, Being being, int mp) {
			return GetTraversalArea(start, being.GetTraversalCost, mp);
		}

		public IEnumerable<Tile> GetArea(Tile start, int range) {
			
			int RightCut = start.X + range < XLength ? start.X + range : (XLength - 1);
			int LeftCut = start.X - range > 0 ? start.X - range : 0;
			int TopCut = start.Y + range;	//out of range for y is handled in loop
			int BottomCut = start.Y - range + 1;

			for (int x = LeftCut; x < RightCut; x++) {
				int xoff = Math.Abs(start.X - x);
				for (int y = BottomCut + xoff > 0 ? BottomCut + xoff : 0;
						 y < (TopCut - xoff < YLength ? TopCut - xoff: YLength);
						 y++) {
					yield return Tiles[x, y];
				}
			}
		}

		public IEnumerable<Tile> GetTraversalArea(Being b, int mp) {
			return ClickedTile != null
				? GetTraversalArea(ClickedTile, b, mp)
				: new Tile[0];
		}

		public IEnumerable<Tile> AsEnumerable() {
			foreach (var t in Tiles) yield return t;
		}
	}
}
