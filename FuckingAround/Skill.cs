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
				if (!TargetTilesOnlyAllowed
					&& TargetSelfAllowed
						? AoE.All(t => t.Inhabitant == null)
						: AoE.All(t => t.Inhabitant == null || t.Inhabitant == Doer)
					) return false;
				foreach (var t in AoE) {
					TileEffect(t);
					if (t.Inhabitant != null) BeingEffect(t.Inhabitant);
				}
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
			ConsoleLoggerHandlerOrWhatever.Log(Doer.ToString() + " attacked " + b.ToString());
		}

		public DefaultAttack(Being doer) : base(doer, "Standard attack"){
			TargetTileAllowed = false;
			TargetTilesOnlyAllowed = false;
			TargetSelfAllowed = false;
			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
		/*
		public override bool Apply(Being source, Tile target) {
			if (source.Place.GetArea(source.Weapon.Range).Any(t => t == target) && target.Inhabitant != null) {
				target.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.DarkRed);
				ConsoleLoggerHandlerOrWhatever.Log(source.ToString() + " attacked " + target.Inhabitant.ToString());
				return true;
			}
			else return false;
		}*/
	}

	public class Blackify : Skill {
		protected override void BeingEffect(Being b) {
			b.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
		}

		public Blackify(Being doer) : base(doer, 5, "Blackify"){
			TargetTileAllowed = true;
			TargetTilesOnlyAllowed = false;
			TargetSelfAllowed = true;
			GetAreaOfEffect = GetGetAreOfEffect(2);
		}
		/*
		public override bool Apply(Being source, Tile target) {
			var l = new List<Being>();
			if (target.Inhabitant != null)
				l.Add(target.Inhabitant);
			l.AddRange(target.Adjacent.Where(t => t.Inhabitant != null).Select(t => t.Inhabitant));
			if (l.Any()) {
				var ls = new List<string>();
				foreach (var b in l) {
					ls.Add(b.ToString());
					b.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
				}
				GameEventLogger.Log(new GameEvent { Source = source, Target = target, skill = this, Targets = l });
				return true;
			} else {
				ConsoleLoggerHandlerOrWhatever.Log("No target in area.");
				return false;
			}
		}*/
	}
}
