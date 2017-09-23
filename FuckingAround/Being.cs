using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class Being : ITurnHaver, SkillUser {

		public string Name { get; private set;}

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

		public bool Perform(Skill skill, Tile target) {
			var ge = skill.Do(this, target);
			if (ge != null) {
				ge.Apply();
				ActionTaken = true;
				return true;
			}
			else return false;
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
		//public void TakeDamage(StatSet damages) {
		//	int preHP = HP;
		//	double total = 0.0;
		//	foreach (StatType dmgType in StatTypeStuff.DirectDamageTypeApplicationTypes) {
		//		double dmg = damages.GetStat(dmgType).Value;
		//		if (dmg != 0) {
		//			double resist = this[dmgType.AsResistance()].Value;
		//			double penetration = damages[dmgType.AsPenetration()];
		//			double threshold = this[dmgType.AsThreshold()].Value;
		//			dmg *= (1 - (resist - penetration));
		//			if (Math.Abs(dmg) < threshold) dmg = 0;	//don't negate more than absolute damage
		//			else dmg -= (dmg < 0 ? -1 : 1) * threshold;	//negate flat amount regardless of negative or positive damage
		//			ConsoleLoggerHandlerOrWhatever.Log(dmg + " " + dmgType);
		//			total += dmg;	//apply all at once later to avoid potentially annoying stuff when multitype damage with >100% res which may damage and heal at once
		//	}	}
		//	HP -= (int)total;
		//	ConsoleLoggerHandlerOrWhatever.Log(preHP + " => " + HP);
		//}

		public void TakeRawDamage(int dmg) {
			int preHP = HP;
			HP -= dmg;
			ConsoleLoggerHandlerOrWhatever.Log(dmg + " DoT taken");
			ConsoleLoggerHandlerOrWhatever.Log(preHP + " => " + HP);
		}

		public void AddToBattle(Battle battle, int x, int y) {
			if (Battle != null)
				throw new ArgumentException("I don't know what the fuck I'm doing");
			Battle = battle;
			OverTimeApplier = new OverTimeApplier(Battle, this);
			HP = MaxHP;
			Place = Battle.TileSet[x, y];
			Battle.Add(this);
		}

		public Being(int team, string name) {
			Name = name;

			Inventory = new PersonalInventory(this);

			if (SkillTreeFilling == null)
				SkillTreeFilling = new SkillTreeFiller(SkillTree);
			foreach (var m in Mods)
				m.Affect(Stats);


			Skills = SkillsRepo.Default.ToList();
			_team = team;

			HP = MaxHP;
		}
		public Being(int team, string name, SkillTreeFiller stf) 
			: this(team, name) {
			SkillTreeFilling = stf;
		}
		public Being(Battle battle, int team, string name, int x, int y)
			:this(team, name) {
			AddToBattle(battle, x, y);
		}
		public Being(Battle battle, int team, string name, int x, int y, SkillTreeFiller stf) 
			: this(team, name, stf) {
			AddToBattle(battle, x, y);
		}

		public event EventHandler TurnStarted;
		public void StartTurn() {
			if(TurnStarted != null) TurnStarted(this, EventArgs.Empty);
		}
	}
}