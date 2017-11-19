using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class Being : ITurnHaver, SkillUser {

		public string Name { get; private set;}
		public MetaBeing MetaBeing { get; private set; }

		public Cardinal Direction;

		public SkillTree SkillTree { get { return SkillTreeshit.Basic; } }
		public SkillTreeFiller SkillTreeFilling;

		private List<StatusEffect> StatusEffects = new List<StatusEffect>();
		public void AddStatusEffect(StatusEffect se) {
			StatusEffects.Add(se);
		}
		public void RemoveStatusEffect(StatusEffect se) {
			se.UnAffect();
			StatusEffects.Remove(se);
		}

		protected OverTimeApplier OverTimeApplier;
		public void AddDoT(DamageOverTime DoT){
			OverTimeApplier.Add(DoT);
		}
		public void RemoveDoT(DamageOverTime DoT) {
			OverTimeApplier.Remove(DoT);
		}

		public StatSet Stats { get; protected set; } = new StatSet();
		public Stat this[StatType st] { get { return Stats.GetStat(st); } }

		public StatSet MainHandStats { get; protected set; }
		public StatSet OffHandStats { get; protected set; }
		public void OnMainHandChanged(object s, PersonalInventory.WeaponSetEventArgs e) {
			if (e.Previous != null)
				foreach (var m in e.Previous.GlobalMods)
					m.Unaffect(MainHandStats);
			if (e.New != null)
				foreach (var m in e.New.GlobalMods)
					m.Affect(MainHandStats);
		}
		public void OnOffHandChanged(object s, PersonalInventory.WeaponSetEventArgs e) {
			if (e.Previous != null)
				foreach (var m in e.Previous.GlobalMods)
					m.Unaffect(OffHandStats);
			if (e.New != null)
				foreach (var m in e.New.GlobalMods)
					m.Affect(OffHandStats);
		}

		public Dictionary<object, StatSet> SkillUsageStats { get; protected set; } = new Dictionary<object, StatSet>();
		public IEnumerable<Mod> Mods {
			get {
				return SkillTreeFilling.Taken
					.SelectMany(node => node.Mods)
					.Concat(Inventory
						.Where(g => g != null)
						.SelectMany(g => g.GlobalMods))
					/*.Concat(StatusEffects.SelectMany(se => se.Mods))*/;
			}
		}

		public bool IsAlive { get { return HP > 0; } }

		public int MaxHP { get { return (int)this[StatType.HP].Value; } }
		private int _hp;
		public int HP {
			get { return _hp; }
			protected set {
				bool wasAlive = this.IsAlive;
				int prev = HP;
				_hp = value;
				if (_hp <= 0) {
					_hp = 0;
					if(wasAlive) this.Die();
				}
				else if (_hp > MaxHP) _hp = MaxHP;
				//if (!wasAlive && IsAlive) Ressurected event?

				if (prev != HP && HPChanged != null)
					HPChanged(this, new HPChangedEventArgs(prev, HP));
			}
		}
		public class HPChangedEventArgs : EventArgs {
			public int Prev;
			public int Curr;
			public HPChangedEventArgs(int prev, int curr) {
				Prev = prev; Curr = curr;
			}
		}
		public EventHandler<HPChangedEventArgs> HPChanged;

		protected double _speed;
		
		public double Speed { get { return IsAlive ? this[StatType.Speed].Value : 0; } }
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

		private int _team;
		public int Team { get { return _team; } }
		public Battle Battle { get; private set; }

		public Weapon Fist = new Weapon(2);
		public PersonalInventory Inventory { get; private set; }
		public Weapon MainHand { get { return Inventory.MainHand; } }
		public Gear OffHand { get { return Inventory.OffHand; } }

		private bool _ActionTaken = false;
		private bool _Moved = false;
		public bool ActionTaken {
			get { return _ActionTaken; }
			/*protected*/ set { //TODO make Actiontaken private
				_ActionTaken = value;
				if (ActionTaken && Moved) EndTurn();
		}	}
		public bool Moved {
			get { return _Moved; }
			protected set {
				_Moved = value;
				if (ActionTaken && Moved) EndTurn();
			}
		}
		public void EndTurn() {
			Awaited = 0;
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
		public IEnumerable<Tile> MovementArea {
			get { return PathFinder.GetTraversalArea(Place, this); }
		}

		public GameEvent Perform(Skill skill, Tile target) {
			var ge = skill.Do(this, target);
			if (ge != null) {
				ge.PostApplication += (s, e) => ActionTaken = true;
			}
			return ge;
		}

		public class MovedArgs : EventArgs{
			public List<Tile> Path { get; protected set; }
			public MovedArgs(List<Tile> path) {
				Path = path;
			}
		}
		public event EventHandler<MovedArgs> MoveStarted;

		public bool Move(Tile destination) {
			if (MovementArea.Any(t => t == destination)
					&& destination.Inhabitant == null) {

				var path = PathFinder.GetPath(Place, destination, this).ToList();
				if (MoveStarted != null) MoveStarted(this, new MovedArgs(path));

				Place = destination;
				Direction = CardinalUtilities.GetMovementCardinal(path[path.Count - 2], path[path.Count - 1]);
				Moved = true;

				return true;
			}
			return false;
		}

		public int MovementPoints { get { return (int)this[StatType.MovementPoints].Value; } }

		public void Die() {
			HP = 0;
			Awaited = 0;

			//TODO don't fucking ToList
			foreach (var se in StatusEffects.ToList())
				se.UnAffect();
			//StatusEffects.Clear();
		}

		public void TakeRawDamage(int dmg) {
			int preHP = HP;
			HP -= dmg;
			ConsoleLoggerHandlerOrWhatever.Log(dmg + " Damage taken");
			ConsoleLoggerHandlerOrWhatever.Log(preHP + " => " + HP);
		}

		public void AddToBattle(Battle battle, int x, int y) {
			if (Battle != null)
				throw new ArgumentException("I don't know what the fuck I'm doing");
			Battle = battle;
			OverTimeApplier = new OverTimeApplier(Battle, this);
			Place = Battle.TileSet[x, y];
			Battle.Add(this);
		}

		public Being(MetaBeing meta, int team, Battle battle, int x, int y) {
			MetaBeing = meta;
			Name = MetaBeing.Name;
			SkillTreeFilling = MetaBeing.SkillTreeFilling;
			Fist = MetaBeing.Fist;
			Inventory = MetaBeing.Inventory;

			_team = team;
			
			AddToBattle(battle, x, y);

			foreach (var m in Mods)
				m.Affect(Stats);

			HP = MaxHP;
			Skills = MetaBeing.Skills;
		}

		public event EventHandler TurnStarted;
		public void StartTurn() {
			if(TurnStarted != null) TurnStarted(this, EventArgs.Empty);
		}
	}
}