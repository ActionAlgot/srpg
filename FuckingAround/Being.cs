using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FuckingAround {
	public class Being {

		public event EventHandler TurnFinished;
		private event EventHandler<TileClickedEventArgs> _command;

		private Tile _place;
		public Tile Place {
			get { return _place; }
			set {
				if (value.Inhabitant != null && value != null) {
					if (value == _place) return;
					else throw new ArgumentException("Tile is occupied."); }
				if (_place != null) _place.Inhabitant = null;
				_place = value;
				if(value != null) value.Inhabitant = this;
			}
		}
		private IEnumerable<Tile> movementArea {
			get { return Place.GetShit(MovementPoints); }
		}
		public void Command(Object s, TileClickedEventArgs e) {
			if(_command != null)
				_command(s, e);
		}

		public void OnCommand(object sender, TileClickedEventArgs e) {
			if (movementArea.Any(t => t == e.Tile))
				if (e.Tile.Inhabitant == null) {
					Place = e.Tile;
					TurnFinished(this, EventArgs.Empty);
				}
		}
		public int MovementPoints;
		public Action<Graphics> Draw;

		public Being(int mp) {
			MovementPoints = mp;
			Draw = g => {
				var b = new SolidBrush(Color.Green);
				g.FillEllipse(b, Place.Rectangle);
			};
			_command += OnCommand;
		}
	}
}