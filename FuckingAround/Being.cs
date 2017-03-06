using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class Being : ITurnHaver, SkillUser {

		private List<StatusEffect> StatusEffects = new List<StatusEffect>();
		public void AddStatusEffect(StatusEffect se) {
			StatusEffects.Add(se);
		}
		public void RemoveStatusEffect(StatusEffect se) {
			StatusEffects.Remove(se);
		}

		private List<PassiveSkill> PSkills;
		public void AddPassiveSkill(PassiveSkill passiveSkill) {
			PSkills.Add(passiveSkill);
			foreach (var m in passiveSkill.Mods)
				m.Affect(Stats);
		}

		protected OverTimeApplier OverTimeApplier;
		public void AddDoT(DamageOverTime DoT){
			OverTimeApplier.Add(DoT);
		}
		public void RemoveDoT(DamageOverTime DoT) {
			OverTimeApplier.Remove(DoT);
		}

		public StatSet Stats { get; protected set; }
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

		public Dictionary<object, StatSet> SkillUsageStats { get; protected set; }
		public IEnumerable<Mod> Mods {
			get {
				return PSkills
					.SelectMany(ps => ps.Mods)
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
				_hp = value;
				if (_hp <= 0) {
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
		private event EventHandler<TileClickedEventArgs> _command;

		private int _team;
		public int Team { get { return _team; } }
		public Battle Battle { get; private set; }
		public Skill SelectedAction;

		public Weapon Fist = new Weapon(2);
		public PersonalInventory Inventory { get; private set; }
		public Weapon MainHand { get { return Inventory.MainHand; } }
		public Gear OffHand { get { return Inventory.OffHand; } }

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

		public class MovedArgs : EventArgs{
			public List<Tile> Path { get; protected set; }
			public MovedArgs(List<Tile> path) {
				Path = path;
			}
		}
		public event EventHandler<MovedArgs> MoveStarted;
		public void Move(object sender, TileClickedEventArgs e) {
			if (movementArea.Any(t => t == e.Tile)
					&& e.Tile.Inhabitant == null) {
				
				var path = Place.GetPath(e.Tile, GetTraversalCost).ToList();
				if(MoveStarted != null) MoveStarted(this, new MovedArgs(path));

				Place = e.Tile;
				Moved = true;
			}
		}
		public int MovementPoints;

		public void Die() {
			HP = 0;
			Awaited = 0;

			//TODO don't fucking ToList
			foreach (var se in StatusEffects.ToList())
				se.UnAffect();
			//StatusEffects.Clear();
		}
		public void TakeDamage(StatSet damages) {
			int preHP = HP;
			double total = 0.0;
			foreach (StatType dmgType in StatTypeStuff.DirectDamageTypeApplicationTypes) {
				double dmg = damages.GetStat(dmgType).Value;
				if (dmg != 0) {
					double resist = this[dmgType.AsResistance()].Value;
					double penetration = damages[dmgType.AsPenetration()];
					double threshold = this[dmgType.AsThreshold()].Value;
					dmg *= (1 - (resist - penetration));
					if (Math.Abs(dmg) < threshold) dmg = 0;	//don't negate more than absolute damage
					else dmg -= (dmg < 0 ? -1 : 1) * threshold;	//negate flat amount regardless of negative or positive damage
					ConsoleLoggerHandlerOrWhatever.Log(dmg + " " + dmgType);
					total += dmg;	//apply all at once later to avoid potentially annoying stuff when multitype damage with >100% res which may damage and heal at once
			}	}
			HP -= (int)total;
			ConsoleLoggerHandlerOrWhatever.Log(preHP + " => " + HP);
		}

		public void TakeRawDamage(int dmg) {
			int preHP = HP;
			HP -= dmg;
			ConsoleLoggerHandlerOrWhatever.Log(dmg + " DoT taken");
			ConsoleLoggerHandlerOrWhatever.Log(preHP + " => " + HP);
		}

		public Being(Battle battle, int team, double speed, int mp) {

			Battle = battle;

			Stats = new StatSet();
			SkillUsageStats = new Dictionary<object, StatSet>();

			Inventory = new PersonalInventory(this);
			PSkills = Passives.Default.ToList();

			OverTimeApplier = new OverTimeApplier(Battle, this);

			foreach (var m in Mods)
				m.Affect(Stats);

			_speed = speed;
			Skills = SkillsRepo.Default.ToList();
			MovementPoints = mp;
			_team = team;
			_command += OnCommand;

			HP = MaxHP;

			battle.Add(this);
		}

		public event EventHandler TurnStarted;

		public void StartTurn() {
			if(TurnStarted != null) TurnStarted(this, EventArgs.Empty);
		}
	}
}