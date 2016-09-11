using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Tile {
		public static int Size = 15;
		public static Pen BorderPen = new Pen(Color.Black);
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
		public int X;
		public int Y;
		public int TraverseCost;
		public SolidBrush Brush;
		private TileSet Owner;
		public IEnumerable<Tile> GetArea(int range) {
			return Owner.GetShit(this, range);
		}
		public Func<Being, int, IEnumerable<Tile>> GetShit {
			get { return (b, n) => Owner.GetShit(this, b, n); }
		}
		public Action<Graphics> Draw;
		public Rectangle Rectangle {
			get { return new Rectangle(X * Size + Owner.XOffset, Y * Size + Owner.YOffset, Size, Size); }
		}
		public IEnumerable<Tile> Adjacent {
			get {
				if (X - 1 >= 0) yield return Owner[X - 1, Y];
				if (X + 1 < Owner.XLength) yield return Owner[X + 1, Y];
				if (Y - 1 >= 0) yield return Owner[X, Y - 1];
				if (Y + 1 < Owner.YLength) yield return Owner[X, Y + 1];
			}
		}

		public Tile(int x, int y, TileSet owner, SolidBrush brush) {
			Inhabitant = null;
			X = x;
			Y = y;
			Owner = owner;
			Brush = brush;
			Draw = g => {
				g.FillRectangle(Brush, Rectangle);
			};
		}
	}
}