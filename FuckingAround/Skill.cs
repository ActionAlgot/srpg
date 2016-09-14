using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public abstract class Skill {

		public Skill(Being doer, int range, string name) {
			_name = name;
			Doer = doer;
			__range = range;
		}
		public Skill(Being doer, string name) : this(doer, 0, name) {
			UseWeaponRange = true;
		}

		protected Func<Tile, IEnumerable<Tile>> GetGetAreOfEffect(int AoERange) {
			return t => t.GetArea(AoERange);
		}
		protected Func<Tile, IEnumerable<Tile>> GetAreaOfEffect;
		protected Being Doer;
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

		public DefaultAttack(Being doer) : base(doer, "Standard attack"){
			TargetTileAllowed = false;
			TargetTilesOnlyAllowed = false;
			TargetSelfAllowed = false;
			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
	}

	public class Spell : Skill {
		public Spell(Being doer, int range, string name) : base(doer, 5, name) { }
	}

	public class ChannelingInstance : ITurnHaver {
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
		protected Tile Place;
		protected Spell Spell;
		protected Func<Tile> TargetSelector;
		public void Do() {
			Spell.Do(TargetSelector());
		}

		public ChannelingInstance(Spell spell, Tile place) {
			Spell = spell;
			Place = place;
		}
	}

	public class ChannelingSpell : Spell {
		protected Spell Spell;

		protected override void TileEffect(Tile t) {
			new ChannelingInstance(Spell, t);
		}
	}

	public class Blackify : Spell {
		protected override void BeingEffect(Being b) {
			b.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
		}

		public Blackify(Being doer) : base(doer, 5, "Blackify"){
			TargetTileAllowed = true;
			TargetTilesOnlyAllowed = false;
			TargetSelfAllowed = true;
			GetAreaOfEffect = GetGetAreOfEffect(2);
		}
	}
}