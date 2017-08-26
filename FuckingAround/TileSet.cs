using System;
using System.Collections.Generic;

namespace srpg {
	public class TileClickedEventArgs : EventArgs {
		public Tile Tile;
		public TileClickedEventArgs(Tile tile) {
			Tile = tile;
		}
	}

	public class TileSet {
		private Tile[,] Tiles;
		public Tile this[int x, int y]{
			get { return Tiles[x, y]; }
		}
		public int XLength { get { return Tiles.GetLength(0); } }
		public int YLength { get { return Tiles.GetLength(1); } }
		public event EventHandler<TileClickedEventArgs> TileClicked;

		public TileSet(int x, int y) {
			Tiles = new Tile[x, y];
			for (int ix = 0; ix < x; ix++)
				for (int iy = 0; iy < y; iy++)
					Tiles[ix, iy] = new Tile(ix, iy, this);
			foreach (var tile in Tiles) tile.TraverseCost = 1;
		}

		public void SelectTile(Tile tile) {
			TileClicked(this, new TileClickedEventArgs(tile));
		}

		public IEnumerable<Tile> GetPath(Tile start, Tile destination, Func<Tile, int> travCostCalc) {	//TODO properly reuse code from 'GetTraversalArea' rather than copypasta

			var accumTravCost = new Dictionary<Tile, int>();    //dictionary for accumulated traversal cost
			var prev = new Dictionary<Tile, Tile>();
			var tiles = new LinkedList<Tile>();

			accumTravCost.Add(start, 0);
			tiles.AddFirst(start);
			for (var node = tiles.First; node != null; node = node.Next) {
				if (node.Value == destination) break;
				
				foreach (var adjT in node.Value.Adjacent) {
					if (accumTravCost.ContainsKey(adjT) == false) {	//check if adjT has been handled
						int adjTravCost = travCostCalc(adjT);
						if (adjTravCost >= 0) {
							accumTravCost.Add(adjT, accumTravCost[node.Value] + adjTravCost);
							var added = false;
							for (var node2 = node.Next; node2 != null; node2 = node2.Next)
								if (accumTravCost[node2.Value] >= accumTravCost[adjT]) {
									added = true;
									tiles.AddBefore(node2, adjT);
									break;
								}
							if (!added) tiles.AddLast(adjT);
							prev[adjT] = node.Value;
				}	}	}
			}
			var tindex = destination;
			var rList = new List<Tile>();
			while (true) {
				rList.Add(tindex);
				if (tindex == start) break;
				tindex = prev[tindex];
			}
			rList.Reverse();
			return rList;
		}

		public IEnumerable<Tile> GetTraversalArea(Tile start, Func<Tile, int> travCostCalc, int mp) {
			var accumTravCost = new Dictionary<Tile, int>();    //dictionary for accumulated traversal cost
			var tils = new LinkedList<Tile>();

			accumTravCost.Add(start, 0);
			tils.AddFirst(start);
			for (var node = tils.First; node != null; node = node.Next) {
				if(node.Value != start)
					yield return node.Value;

				foreach (var adjT in node.Value.Adjacent) {	//insert tiles adjacent to current node
					if (accumTravCost.ContainsKey(adjT) == false) {
						int adjTravCost = travCostCalc(adjT);
						if (adjTravCost >= 0) {
							int _accumTravCost = accumTravCost[node.Value] + adjTravCost;
							if (_accumTravCost <= mp) {
								accumTravCost.Add(adjT, _accumTravCost);
								var added = false;
								for (var node2 = node.Next; node2 != null; node2 = node2.Next) {
									if (accumTravCost[node2.Value] >= accumTravCost[adjT]) {
										added = true;
										tils.AddBefore(node2, adjT);
										break;
									}
								}
								if (!added) tils.AddLast(adjT);
				}	}	}	}
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
	}
}
