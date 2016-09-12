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

		private int _strength;
		public int Strength { get { return _strength; } }
		private int _maxHP;
		public int MaxHP { get { return _maxHP; } }
		private int _HP;
		public int HP { get { return _HP; } }

		private IEnumerable<Skill> _skills;
		public IEnumerable<Skill> Skills {
			get { return _skills; }
			set { _skills = value; }	//For debugging only
		}

		public event EventHandler TurnFinished;
		private event EventHandler<TileClickedEventArgs> _command;

		public Weapon Unarmed = new Weapon { Range = 1, Damage = 1 };

		private int _team;
		public int Team { get { return _team; } }
		public Skill SelectedAction;

		private Weapon _weapon;
		public Weapon Weapon {
			get { return _weapon ?? Unarmed; }
			set { _weapon = value; }
		}
		public Armour Armour;

		public int GetTraversalCost(Tile t) {
			if (t.Inhabitant != null) {
				if (t.Inhabitant.Team == this.Team)
					return t.TraverseCost;
				return -1;
			}
			return t.TraverseCost;
		}

		public bool ActionTaken { get; protected set; }
		public bool Moved { get; protected set; }
		public void EndTurn() {
			SelectedAction = null;
			ActionTaken = false;
			Moved = false;
			TurnFinished(this, EventArgs.Empty);
		}
		public void StandardAttack (object sender, TileClickedEventArgs e) {
			if(Place.GetArea(Weapon.Range).Any(t => t == e.Tile)){
				e.Tile.Brush = new SolidBrush(Color.DarkRed);
				ConsoleLoggerHandlerOrWhatever.Log(Place.X + ", " + Place.Y + " attacked " + e.Tile.X + ", " + e.Tile.Y);
				ActionTaken = true;
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
			if(!ActionTaken && SelectedAction != null) {
				if (Place.GetArea(SelectedAction.Range).Any(t => t == e.Tile)) {
					if (SelectedAction.Apply(e.Tile))
						ActionTaken = true;
					else ConsoleLoggerHandlerOrWhatever.Log("Skill apply failed");
				}
				SelectedAction = null;
			}
			else if (SelectedAction == null && e.Tile.Inhabitant == null && !Moved)
				Move(sender, e);
			else if(!ActionTaken && e.Tile.Inhabitant != null)
				StandardAttack(this, e);
			if (ActionTaken && Moved)
				this.EndTurn();
		}

		public void Move(object sender, TileClickedEventArgs e) {
			if (movementArea.Any(t => t == e.Tile))
				if (e.Tile.Inhabitant == null) {
					Place = e.Tile;
					Moved = true;
				}
		}
		public int MovementPoints;
		public Action<Graphics> Draw;
		public SolidBrush Brush;

		public Being(int team, int mp) {
			Skills = new Skill[0];
			MovementPoints = mp;
			_team = team;
			Brush = new SolidBrush(Color.Green);
			Draw = g => g.FillEllipse(Brush, Place.Rectangle);
			_command += OnCommand;
		}
	}
}