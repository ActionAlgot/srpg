using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Skill {	//TODO handle wether 'doer' is using weapon

		public string Name { get; protected set; }
		protected Func<Skill, SkillUser, Tile, bool> _ValidTarget { get; set; }
		protected Func<Skill, SkillUser, IEnumerable<Tile>> _Range { get; set; }
		protected Func<Skill, SkillUser, Tile, IEnumerable<Tile>> _GetAreaOfEffect { get; set; }
		protected Action<Skill, SkillUser, Tile> _Effect { get; set; }

		public bool ValidTarget(SkillUser su, Tile target) { return _ValidTarget(this, su, target); }
		public IEnumerable<Tile> Range(SkillUser su) { return _Range(this, su); }
		public IEnumerable<Tile> AoE(SkillUser su, Tile target) { return _GetAreaOfEffect(this, su, target); }
		public void Apply(SkillUser su, Tile target) { _Effect(this, su, target); }
		//public void Apply(SkillUser su, Tile target, IEnumerable<Mod> mods) {
		//	_Effect(this.GetModdedInstance(mods), su, target);
		//}

		public IEnumerable<Mod> Mods { get; protected set; }

		public virtual bool Do(SkillUser doer, Tile target) {
			if(ValidTarget(doer, target) && Range(doer).Any(t => t == target)){
				foreach (Tile t in _GetAreaOfEffect(this, doer, target))
					_Effect(this, doer, t);
				//GameEventLogger.Log(new GameEvent(stuff))
				return true;
			}
			else return false;
		}
		
		public Skill GetModdedInstance(IEnumerable<Mod> mods) {
			Skill copy = this.MemberwiseClone() as Skill;
			copy.Mods = Mods.Concat(mods).ToList();
			return copy;
		}

		public Skill(string name,
			Func<Skill, SkillUser, Tile, bool> targetValidator,
			Func<Skill, SkillUser, IEnumerable<Tile>> rangeGetter,
			Func<Skill, SkillUser, Tile, IEnumerable<Tile>> aoeGetter,
			Action<Skill, SkillUser, Tile> effect)
			: this(name, targetValidator, rangeGetter, aoeGetter, effect, new Mod[0]) { }

		public Skill(string name,
			Func<Skill, SkillUser, Tile, bool> targetValidator,
			Func<Skill, SkillUser, IEnumerable<Tile>> rangeGetter,
			Func<Skill, SkillUser, Tile, IEnumerable<Tile>> aoeGetter,
			Action<Skill, SkillUser, Tile> effect,
			IEnumerable<Mod> mods) {
				Name = name;
				_ValidTarget = targetValidator;
				_Range = rangeGetter;
				_GetAreaOfEffect = aoeGetter;
				_Effect = effect;
				Mods = mods.ToList();
		}
		public StatSet GetStatSet(SkillUser su) {
			if (!su.OtherStats.ContainsKey(this)) {
				su.OtherStats[this] = new StatSet();
				su.OtherStats[this].AddSubSet(su.Stats);
				foreach (var m in this.Mods)
					m.Affect(su.OtherStats[this]);
			}
			return su.OtherStats[this];
		}

		public double GetStat(StatType st, SkillUser su) {
			return GetStatSet(su)[st];
		}
	}

	public static class SkillsRepo {
		#region subFuncs
		private static class Validation {
			public static bool AnyAliveBeingInArea(Skill skill, SkillUser su, Tile target) {
				return skill.AoE(su, target).Any(t2 => t2.Inhabitant != null && t2.Inhabitant.IsAlive);
			}
			public static bool AliveBeingIsTarget(Skill skill, SkillUser su, Tile target) {
				return target.Inhabitant != null && target.Inhabitant.IsAlive;
			}
			public static bool NoChannelingInstance(Skill skill, SkillUser su, Tile target) {
				return target.ChannelingInstance == null;
			}
			public static bool AnyChannelingInstanceInArea(Skill skill, SkillUser su, Tile target){
				return skill.AoE(su, target).Any(t2 => t2.ChannelingInstance != null);
			}
		}
		private static class Range {
			public static IEnumerable<Tile> UseWeaponRange(Skill skill, SkillUser su){
				return ((Being)su).MainHand.Range(skill, su);
			}
			public static IEnumerable<Tile> GetFromMods(Skill skill, SkillUser su) {
				return su.Place.GetArea((int)skill.GetStat(StatType.Range, su));
			}
		}
		private static class AoE {

			public static IEnumerable<Tile> UseWeapon(Skill skill, SkillUser su, Tile target) {
				return ((Being)su).MainHand.AoE(skill, su, target);
			}

			public static IEnumerable<Tile> TargetOnly(Skill skill, SkillUser su, Tile target) {
				return new Tile[] { target };
			}
			public static IEnumerable<Tile> FromMods(Skill skill, SkillUser su, Tile target) {
				return target.GetArea((int)skill.GetStat(StatType.AreaOfEffect, su));
			}
		}
		private static class Effect {
			public static void Damage(Skill skill, SkillUser su, Tile target) {
				if(target.Inhabitant != null) target.Inhabitant.TakeDamage(skill.GetStatSet(su));
			}

			public static Action<Skill, SkillUser, Tile> Channel(Skill skill) {
				return (s, su, t) => {
					if (t.ChannelingInstance == null) {
						t.ChannelingInstance = new ChannelingInstance(su.GetChannelingMods(), skill, t);
						TurnTracker.Add(t.ChannelingInstance);
					} else throw new Exception("bullshit");
				};
			}
			public static Action<Skill, SkillUser, Tile> AddModsToChannel(IEnumerable<Mod> mods) {
				return (s, su, t) => {
					if(t.ChannelingInstance != null){
						foreach(var m in mods)
							m.Affect(t.ChannelingInstance.Stats);
					} else throw new Exception("bullshit");
				};
			}

			public static Action<Skill, SkillUser, Tile> DoWithWeapon(Action<Skill, SkillUser, Tile> effect) {
				return (s, su, t) => {
					var b = su as Being;
					b.MainHand.Affect(s, su, t, effect);
				};
			}
		}
		#endregion 

		public static Skill StandardAttack = new Skill("Standard attack",
			Validation.AnyAliveBeingInArea,
			Range.UseWeaponRange,
			AoE.UseWeapon,
			Effect.DoWithWeapon(Effect.Damage));
		public static Skill Blackify = new Skill("Blackify",
			Validation.AnyAliveBeingInArea,
			Range.GetFromMods,
			AoE.FromMods,
			(s, su, t) => { if(t.Inhabitant != null) t.Inhabitant.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);},
			new Mod[]{
				new AdditionMod(StatType.Range, 6),
				new AdditionMod(StatType.AreaOfEffect, 2)
			});
		public static Skill BlackifyChannel = new Skill("Blackify channeling",
			Validation.NoChannelingInstance,
			Range.GetFromMods,
			AoE.TargetOnly,
			Effect.Channel(Blackify),
			new Mod[]{
				new AdditionMod(StatType.Range, 6)
			});
		public static Skill ChannelSpeedUp = new Skill("Channel speedup",
			Validation.AnyChannelingInstanceInArea,
			Range.GetFromMods,
			AoE.TargetOnly,
			Effect.AddModsToChannel(new Mod[] { new AdditionMod(StatType.None, 3) }),
			new Mod[] { 
				new AdditionMod(StatType.Range, 6)
			});

		public static IEnumerable<Skill> Default = new Skill[]{
			StandardAttack,
			Blackify,
			BlackifyChannel,
			ChannelSpeedUp
		};
	}
	
	public interface SkillUser {
		Tile Place { get; }
		//Weapon Weapon { get; }	//I'm a dumb fuck
		StatSet Stats { get; }
		Dictionary<object, StatSet> OtherStats { get; }	
	}
}
