using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FuckingAround {
	public class Being : ITurnHaver, SkillUser {

		private List<PassiveSkill> PSkills;
		public void AddPassiveSkill(PassiveSkill passiveSkill) {
			PSkills.Add(passiveSkill);
		}
		public IEnumerable<Mod> Mods {
			get {
				return PSkills
					.SelectMany(ps => ps.Mods); /*
					.Concat(Equipment.SelectMany(eq => eq.UserMods))
					.Concat(DebuffsAndBuffs.SelectMany(thing => thing.VictimMods))
					*/
			}
		}

		public bool IsAlive { get { return HP > 0; } }

		public int MaxHP { get { return (int)Mods.GetStat(StatType.HP); } }
		private int _hp;
		public int HP {
			get { return _hp; }
			protected set {
				bool wasAlive = this.IsAlive;
				_hp = value;
				if (_hp < 0) {
					_hp = 0;
					if(wasAlive) this.Die();
				}
				else if (_hp > MaxHP) _hp = MaxHP;
				/*
				if (!wasAlive && IsAlive) {
					//Ressurected event?
				}
				*/
			}
		}

		protected double _speed;
		
		public double Speed { get { return IsAlive ? Mods.GetStat(StatType.Speed) : 0; } }
		public double Awaited { get; protected set; }

		public void Await(double time) {
			Awaited += Speed * time;
		}

		private IEnumerable<Skill> _skills;
		public IEnumerable<Skill> Skills {
			get { return _skills; }
			set { _skills = value; }	//For debugging only
		}

		public event EventHandler TurnFinished;
		private event EventHandler<TileClickedEventArgs> _command;

		public Weapon Unarmed = new Weapon { Range = 2, Mods = new List<Mod>(){ new Mod(StatType.PhysicalDamage, ModifyingMethod.Add, 2)} };

		private int _team;
		public int Team { get { return _team; } }
		public Skill SelectedAction;

		private Weapon _weapon;
		public Weapon Weapon {
			get { return _weapon ?? Unarmed; }
			set { _weapon = value; }
		}

		public int GetTraversalCost(Tile t) {
			if (t.Inhabitant != null && t.Inhabitant.IsAlive) {
				if (t.Inhabitant.Team == this.Team)
					return t.TraverseCost;
				else return -1;
			}
			return t.TraverseCost;
		}

		public bool ActionTaken { get; protected set; }
		public bool Moved { get; protected set; }
		public void EndTurn() {
			Awaited = 0;
			SelectedAction = null;
			ActionTaken = false;
			Moved = false;
			if(TurnFinished != null) TurnFinished(this, EventArgs.Empty);
		}

		public override string ToString(){
			return "(X: " + Place.X + ", Y: " + Place.Y + ")";
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
			if(_command != null) _command(s, e);
		}

		public void OnCommand(object sender, TileClickedEventArgs e) {
			if (!ActionTaken && SelectedAction != null) {
				if (SelectedAction.Do(this, e.Tile))
					ActionTaken = true;
				else ConsoleLoggerHandlerOrWhatever.Log("Skill apply failed");
				SelectedAction = null;
			} else if (SelectedAction == null && e.Tile.Inhabitant == null && !Moved)
				Move(sender, e);
			else if (!ActionTaken && e.Tile.Inhabitant != null)
				if (Skills.First().Do(this, e.Tile))
					ActionTaken = true;
			if (ActionTaken && Moved)
				this.EndTurn();
		}


		public bool Moving;
		public int PathIndex;
		public List<Tile> Path;
		private Rectangle MovingRect;


		public void GraphicMove(int n) {
			if (PathIndex + 1 >= Path.Count) {
				Moving = false;
				if (MoveFinished != null)
					MoveFinished(this, EventArgs.Empty);
				return;
			}

			int xDiff = Path[PathIndex].X - Path[PathIndex + 1].X;
			if (xDiff != 0) {
				if (xDiff < 0) {
					MovingRect.X += n;
					if (MovingRect.X >= Path[PathIndex + 1].Rectangle.X) {
						MovingRect.X = Path[PathIndex + 1].Rectangle.X;
						PathIndex++;
						GraphicMove(MovingRect.X - Path[PathIndex].Rectangle.X);
					}
				} else {
					MovingRect.X -= n;
					if (MovingRect.X <= Path[PathIndex + 1].Rectangle.X) {
						MovingRect.X = Path[PathIndex + 1].Rectangle.X;
						PathIndex++;
						GraphicMove(MovingRect.X - Path[PathIndex].Rectangle.X);
					}
				}
			} else {
				int yDiff = Path[PathIndex].Y - Path[PathIndex + 1].Y;
				if (yDiff < 0) {
					MovingRect.Y += n;
					if (MovingRect.Y >= Path[PathIndex + 1].Rectangle.Y) {
						MovingRect.Y = Path[PathIndex + 1].Rectangle.Y;
						PathIndex++;
						GraphicMove(MovingRect.Y - Path[PathIndex].Rectangle.Y);
					}
				} else {
					MovingRect.Y -= n;
					if (MovingRect.Y <= Path[PathIndex + 1].Rectangle.Y) {
						MovingRect.Y = Path[PathIndex + 1].Rectangle.Y;
						PathIndex++;
						GraphicMove(MovingRect.Y - Path[PathIndex].Rectangle.Y);
					}
				}
			}
		}

		public event EventHandler MoveFinished;
		public event EventHandler MoveStarted;
		public void Move(object sender, TileClickedEventArgs e) {
			if (movementArea.Any(t => t == e.Tile)
					&& e.Tile.Inhabitant == null) {

				Moving = true;
				MovingRect = Place.Rectangle;
				PathIndex = 0;
				Path = Place.GetPath(e.Tile, GetTraversalCost).ToList();
				if (MoveStarted != null) MoveStarted(this, EventArgs.Empty);

				Place = e.Tile;
				Moved = true;
			}
		}
		public int MovementPoints;
		public Action<Graphics> Draw;
		public SolidBrush Brush;

		public void Die() {
			HP = 0;
			Awaited = 0;
			Brush = new SolidBrush(Color.DarkOrange);
		}
		public void TakeDamage(IEnumerable<Mod> mods) {
			int preHP = HP;
			int total = 0;
			foreach (StatType dmg in Enum.GetValues(typeof(StatType))) {
				if ((dmg & StatType.Damage) == StatType.Damage && dmg != StatType.Damage) {
					int crap = (int)mods.GetStat(dmg);
					if (crap != 0) {
						double resist = this.Mods.GetStat((StatType)(dmg - StatType.Damage) | StatType.Resistance);
						crap = crap - (int)(crap * resist);
						ConsoleLoggerHandlerOrWhatever.Log(crap + " " + dmg);
						total += crap;	//apply all at once to avoid potentially annoying stuff when multitype damage with >100% res damages and heals at once
					}
				}
			}
			HP -= total;
			ConsoleLoggerHandlerOrWhatever.Log(preHP + " => " + HP);
		}

		public Being(int team, double speed, int mp) {

			PSkills = Passives.Default.ToList();

			_speed = speed;
			Skills = SkillsRepo.Default.ToList();
			MovementPoints = mp;
			_team = team;
			Brush = new SolidBrush(Color.Green);
			Draw = g => {
				if (Moving) g.FillEllipse(Brush, MovingRect);
				else g.FillEllipse(Brush, Place.Rectangle);
			};
			_command += OnCommand;

			HP = MaxHP;
		}
	}
}