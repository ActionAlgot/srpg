using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public static class PathFinder {

		public static int GetTraversalCostWITHOUTJUMP(Being b, Tile t) {
			if (t.Inhabitant != null && t.Inhabitant.IsAlive)
				if (t.Inhabitant.Team == b.Team)
					return t.TraverseCost;
				else return -1;
			return t.TraverseCost;
		}

		public static int GetTraversalCost(Being b, Tile t1, Tile t2) {
			if (t2.Inhabitant != null && t2.Inhabitant.IsAlive)
				if (t2.Inhabitant.Team == b.Team)
					return t2.TraverseCost;
				else return -1;
			else if (t1 != null && t2.Height - t1.Height > b[StatType.Jump].Value)
				return -1;
			return t2.TraverseCost;
		}
		
		private static Dictionary<Tile, int> GetLongJumps(Tile startT, Being b) {  //shouldn't use dic just to return a series of paired values but fuck it

			var rd = new Dictionary<Tile, int>();
			
			int startMoveCost = GetTraversalCostWITHOUTJUMP(b, startT);
			Tile mid, dest;
			int heightDelta;

			foreach (Cardinal c in Enum.GetValues(typeof(Cardinal))){

				mid = startT.GetAdjacent(c);
				if (mid != null) dest = mid.GetAdjacent(c);
				else continue;

				if (dest != null && GetTraversalCostWITHOUTJUMP(b, dest) >= 0) {
					heightDelta = mid.Height - startT.Height;
					if (heightDelta <= 0 && dest.Height - startT.Height <= 0) { //can't jump over or to higher tiles
						int midTravCost = GetTraversalCostWITHOUTJUMP(b, mid);
						int jumpMoveCost = startMoveCost + GetTraversalCostWITHOUTJUMP(b, dest);

						if ((midTravCost != -1 || false)  //TODO this should account for heightDelta for jumping over opponents
							&& (heightDelta <= -1 //TODO Being.MaxDownJump
							 || midTravCost > startMoveCost))

							rd.Add(dest, jumpMoveCost);
			}	}	}

			return rd;
		}

		public static IEnumerable<Tile> GetPath(Tile start, Tile destination, Being b) {    //TODO properly reuse code from 'GetTraversalArea' rather than copypasta

			var accumTravCost = new Dictionary<Tile, int>();
			var prev = new Dictionary<Tile, Tile>();
			var tilesByOrder = new LinkedList<Tile>();

			accumTravCost.Add(start, 0);
			tilesByOrder.AddFirst(start);
			for (var node = tilesByOrder.First; node != null; ((Action)(() => { node = node.Next; tilesByOrder.RemoveFirst();}))() ) {
				Tile current = node.Value;
				if (current == destination) break;
				var jumps = GetLongJumps(current, b);

				foreach (var adjT in current.Adjacent.Concat(jumps.Keys)) { //work through tiles that can be reached from current
					int adjTravCost =
						jumps.ContainsKey(adjT) ? 
						jumps[adjT] : GetTraversalCost(b, node.Value, adjT);

					if (adjTravCost >= 0) {	//negative values exists as codes for obstacles
						bool visited = accumTravCost.ContainsKey(adjT);
						int _accumTravCost = accumTravCost[current] + adjTravCost;
						if (!visited || _accumTravCost < accumTravCost[adjT]) {
							accumTravCost[adjT] = _accumTravCost;

							if (visited) tilesByOrder.Remove(adjT);
							var added = false;
							for (var node2 = node.Next; node2 != null; node2 = node2.Next)
								if (accumTravCost[node2.Value] >= _accumTravCost) {
									added = true;
									tilesByOrder.AddBefore(node2, adjT);
									break;
								}
							if (!added) tilesByOrder.AddLast(adjT);
							prev[adjT] = current;
			}	}	}	}
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

		public static IEnumerable<Tile> GetTraversalArea(Tile start, Being b) {
			double mp = b[StatType.MovementPoints].Value;
			var accumTravCost = new Dictionary<Tile, int>();
			var tilesByOrder = new LinkedList<Tile>();

			accumTravCost.Add(start, 0);
			tilesByOrder.AddFirst(start);
			for (var node = tilesByOrder.First; node != null; node = node.Next) {
				Tile current = node.Value;
				if (current != start)
					yield return current;
				var jumps = GetLongJumps(current, b);

				foreach (var adjT in current.Adjacent.Concat(jumps.Keys)) { //work through tiles that can be reached from current
					int adjTravCost =
						jumps.ContainsKey(adjT) ?
						jumps[adjT] : GetTraversalCost(b, node.Value, adjT);

					if (adjTravCost >= 0) {
						bool visited = accumTravCost.ContainsKey(adjT);
						int _accumTravCost = accumTravCost[current] + adjTravCost;
						if (!visited || _accumTravCost < accumTravCost[adjT]) {
							if (_accumTravCost <= mp) {
								accumTravCost[adjT] = _accumTravCost;

								if (visited) tilesByOrder.Remove(adjT);
								var added = false;
								for (var node2 = node.Next; node2 != null; node2 = node2.Next) {
									if (accumTravCost[node2.Value] >= _accumTravCost) {
										added = true;
										tilesByOrder.AddBefore(node2, adjT);
										break;
									}
								}
								if (!added) tilesByOrder.AddLast(adjT);
							}
						}
					}
				}
			}
		}
	}
}
