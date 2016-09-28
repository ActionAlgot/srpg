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
		protected string _name;
		private bool UseWeaponRange;
		private int __range;
		protected int _range { get { return UseWeaponRange ? Doer.Weapon.Range : __range; } }
		public virtual int Range { get { return _range; } }
		public string Name { get { return _name; } }

		protected bool TargetTileAllowed;
		protected bool TargetSelfAllowed;
		protected bool MustTargetChannelingInstance;
		protected bool MustTargetBeing;

		protected virtual void TileEffect(Tile t) { }
		protected virtual void BeingEffect(Being b) { }
		protected virtual void ChannelingEffect(ChannelingInstance ci) { }

		public virtual bool Do(Tile target) {
			if(!TargetTileAllowed && target.Inhabitant == null && target.ChannelingInstance == null) return false;
			if (Doer.Place.GetArea(Range).Any(t => t == target)) {
				IEnumerable<Tile> AoE = GetAreaOfEffect(target);
				if (   (MustTargetChannelingInstance ? AoE.All(t => t.ChannelingInstance == null) : false)	//also (correctly) returns if AoE is empty
					|| (MustTargetBeing ? AoE.All(t => (t.Inhabitant == null || (!TargetSelfAllowed && t.Inhabitant == Doer))) : false))
					return false;
				foreach (Tile t in AoE) {
					TileEffect(t);
					if (t.Inhabitant != null && (TargetSelfAllowed || t.Inhabitant != Doer))
						BeingEffect(t.Inhabitant);
					if (t.ChannelingInstance != null)
						ChannelingEffect(t.ChannelingInstance);
				}

				GameEventLogger.Log( new GameEvent {
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
			b.TakeDamage(Doer.Mods.Concat(Doer.Weapon.Mods));
			//ConsoleLoggerHandlerOrWhatever.Log(Doer.ToString() + " attacked " + b.ToString());
		}

		public DefaultAttack(SkillUser doer) : base(doer, "Standard attack"){
			TargetTileAllowed = false;
			MustTargetBeing = false;
			TargetSelfAllowed = false;
			GetAreaOfEffect = GetGetAreOfEffect(1);
		}
	}

	public abstract class Spell : Skill {
		public Spell(SkillUser doer, int range, string name) : base(doer, range, name) { }
		public Spell GetAsChanneled(ChannelingInstance CI) {
			Spell copy = (Spell)this.MemberwiseClone();	//new this(CI, _range, Name);

			/*
			foreach (var fuck in Doer.Mods) {
				if (!CI.Mods.ContainsKey(fuck.Key)) CI.Mods[fuck.Key] = new List<Func<double, double>>();
				CI.Mods[fuck.Key].AddRange(fuck.Value);
			}
			*/
			
			copy.Doer = CI;
			return copy;
		}
	}


	public class Blackify : Spell {
		protected override void BeingEffect(Being b) {
			b.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
		}

		public Blackify(SkillUser doer) : base(doer, 5, "Blackify"){
			TargetTileAllowed = true;
			MustTargetBeing = true;
			TargetSelfAllowed = true;
			GetAreaOfEffect = GetGetAreOfEffect(2);
		}
	}

	public interface SkillUser {
		Tile Place { get; }
		Weapon Weapon { get; }	//I'm a dumb fuck

		//take base value and return double to be added on top of it 
		//Funcs must not use internal ref values
		IEnumerable<Mod> Mods { get; }	
	}
}