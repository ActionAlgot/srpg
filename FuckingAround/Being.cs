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

		public Weapon Unarmed = new Weapon { Range = 1, Damage = 1 };

		private int _team;
		public int Team { get { return _team; } }
		public object SelectedAction;

		private Weapon _weapon;
		public Weapon Weapon {
			get { return _weapon ?? Unarmed; }
			set { _weapon = value; }
		}
		public Armour Armour;

		public int MovementCost(Tile t) {
			if (t.Inhabitant != null) {
				if (t.Inhabitant.Team == this.Team)
					return t.TraverseCost;
				return -1;
			}
			return t.TraverseCost;
		}

		private bool ActionTaken;
		private bool Moved;
		public void EndTurn() {
			ActionTaken = false;
			Moved = false;
			TurnFinished(this, EventArgs.Empty);
		}
		public void StandardAttack (object sender, TileClickedEventArgs e) {
			if(Place.GetArea(Weapon.Range).Any(t => t == e.Tile)){
				Debug.WriteLine(Place.X + ", " + Place.Y + " attacked " + e.Tile.X + ", " + e.Tile.Y);
			}
		}

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
			get { return Place.GetShit(this, MovementPoints); }
		}
		public void Command(Object s, TileClickedEventArgs e) {
			if(_command != null)
				_command(s, e);
		}

		public void OnCommand(object sender, TileClickedEventArgs e) {

			ActionTaken = true;

			if(SelectedAction != null) {

			}
			else if (SelectedAction == null && e.Tile.Inhabitant == null && !Moved) {
				Move(sender, e);
			}
			else if(e.Tile.Inhabitant != null){
				StandardAttack(this, e);
			}
		}

		public void Move(object sender, TileClickedEventArgs e) {
			if (movementArea.Any(t => t == e.Tile))
				if (e.Tile.Inhabitant == null) {
					Place = e.Tile;
					Moved = true;
					if (ActionTaken && Moved) EndTurn();
				}
		}
		public int MovementPoints;
		public Action<Graphics> Draw;

		public Being(int team, int mp) {
			MovementPoints = mp;
			_team = team;
			Draw = g => {
				var b = new SolidBrush(Color.Green);
				g.FillEllipse(b, Place.Rectangle);
			};
			_command += OnCommand;
		}
	}
}