using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public abstract class Skill {

		public Skill(SkillUser doer, int range, string name) {
			_name = name;
			Doer = doer;
			__range = range;
		}
		public Skill(SkillUser doer, string name) : this(doer, 0, name) {
			UseWeaponRange = true;
		}

		protected Func<Tile, IEnumerable<Tile>> GetGetAreOfEffect(int AoERange) {
			return t => t.GetArea(AoERange);
		}
		protected Func<Tile, IEnumerable<Tile>> GetAreaOfEffect;
		protected SkillUser Doer;
		protected bool TargetTileAllowed;
		protected bool TargetTilesOnlyAllowed;
		protected bool TargetSelfAllowed;
		protected string _name;
		private bool UseWeaponRange;
		private int __range;
		protected int _range { get { return UseWeaponRange ? Doer.Weapon.Range : __range; } }
		public virtual int Range { get { return _range; } }
		public string Name { get { return _name; } }

		protected virtual void TileEffect(Tile t){}
		protected virtual void BeingEffect(Being b){}

		public virtual bool Do(Tile target) {
			if(!TargetTileAllowed && target.Inhabitant == null) return false;
			if (Doer.Place.GetArea(Range).Any(t => t == target)) {
				var AoE = GetAreaOfEffect(target);
				if (!TargetTilesOnlyAllowed	//also (correctly) returns if AoE is empty
					&& AoE.All(t => t.Inhabitant == null && !(TargetSelfAllowed || t.Inhabitant != Doer))
					) return false;
				foreach (var t in AoE) {
					TileEffect(t);
					if (t.Inhabitant != null && (TargetSelfAllowed || t.Inhabitant != Doer))
						BeingEffect(t.Inhabitant);
				}

				GameEventLogger.Log(new GameEvent {
					Source = Doer,
					skill = this,
					Target = target,
					Targets = AoE
						.Where(t => t.Inhabitant != null && (TargetSelfAllowed || t.Inhabitant != Doer))
						.Select(t => t.Inhabitant)
				});

				return true;
			}
			return false;
		}
	}

	public class DefaultAttack : Skill {
		protected override void TileEffect(Tile t) {
			t.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.DarkRed);
		}
		protected override void BeingEffect(Being b) {
			//ConsoleLoggerHandlerOrWhatever.Log(Doer.ToString() + " attacked " + b.ToString());
		}

		public DefaultAttack(SkillUser doer) : base(doer, "Standard attack"){
			TargetTileAllowed = false;
			TargetTilesOnlyAllowed = false;
			TargetSelfAllowed = false;
			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
	}

	public class Spell : Skill {
		public Spell(SkillUser doer, int range, string name) : base(doer, 5, name) { }
	}

	public class ChannelingInstance : ITurnHaver, SkillUser {
		#region ITurnHaver
		public event EventHandler TurnFinished;
		protected double _speed;
		protected double _awaited;
		public double Speed { get { return _speed; } }
		public double Awaited { get { return _awaited; } }
		public void Await(double time) {
			_awaited += Speed * time;
		}
		#endregion
		public Weapon Weapon { get { return null; } }	//Kill me
		public Tile Place { get; protected set; }
		protected Spell Spell;
		protected Func<Tile> TargetSelector;

		public void Do() {
			Spell.Do(TargetSelector());
			_awaited = 0;	//should just kill self
			_speed = 0;
			if(TurnFinished != null) TurnFinished(this, EventArgs.Empty);
		}

		public ChannelingInstance(Func<SkillUser, Spell> spellMaker, Tile place, Func<Tile> targetSelector) {

			_speed = 10;
			Spell = spellMaker(this);
			Place = place;
			TargetSelector = targetSelector;
		}
	}

	public class ChannelingSpell : Spell {
		protected Spell Spell;
		protected Func<SkillUser, Spell> SpellMaker;
		protected Func<Tile, Func<Tile>> TargetSelector;
		private TurnFuckYouFuckThatFuckEverything ShitTracker;

		protected override void TileEffect(Tile t) {
			var piss = new ChannelingInstance(SpellMaker, t, TargetSelector(t));
			piss.TurnFinished += (s, e) => ShitTracker.Remove(piss);
			ShitTracker.Add(piss);
		}
		/*
		public ChannelingSpell(Being doer, Spell spell, Func<Tile, Func<Tile>> targetSelector, TurnFuckYouFuckThatFuckEverything shitTracker) 
			: base(doer, spell.Range, spell.Name + " channeling") {
			ShitTracker = shitTracker;
			Spell = spell;
			//pell.Doer = this;
			TargetSelector = targetSelector;
			TargetTileAllowed = true;
			TargetTilesOnlyAllowed = true;

			GetAreaOfEffect = GetGetAreOfEffect(1);
		}*/

		public ChannelingSpell(SkillUser doer, Func<SkillUser, Spell> spellMaker, Func<Tile, Func<Tile>> targetSelector, TurnFuckYouFuckThatFuckEverything shitTracker)
			: base(doer, 6, "Channel" + " channeling") {
			ShitTracker = shitTracker;
			SpellMaker = spellMaker;
			//Spell.Doer = this;
			TargetSelector = targetSelector;
			TargetTileAllowed = true;
			TargetTilesOnlyAllowed = true;

			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
	}

	public class Blackify : Spell {
		protected override void BeingEffect(Being b) {
			b.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
		}

		public Blackify(SkillUser doer) : base(doer, 5, "Blackify"){
			TargetTileAllowed = true;
			TargetTilesOnlyAllowed = false;
			TargetSelfAllowed = true;
			GetAreaOfEffect = GetGetAreOfEffect(2);
		}
	}

	public interface SkillUser {
		Tile Place { get; }
		Weapon Weapon { get; }	//I'm a dumb fuck
	}
}