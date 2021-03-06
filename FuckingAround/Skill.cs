﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class Skill {	//TODO handle wether 'doer' is using weapon

		public string Name { get; protected set; }
		protected Func<Skill, SkillUser, Tile, bool> _ValidTarget;
		protected Func<object, SkillUser, IEnumerable<Tile>> _Range;
		protected Func<object, SkillUser, Tile, IEnumerable<Tile>> _GetAreaOfEffect;
		protected Action<object, SkillUser, Tile, GameEvent> _Effect;

		public bool ValidTarget(SkillUser su, Tile target) { return _ValidTarget(this, su, target); }
		public IEnumerable<Tile> Range(SkillUser su) {
			//init statset here because this is always first method called on skill
			if (!su.SkillUsageStats.ContainsKey(this)) {
				su.SkillUsageStats[this] = new StatSet();
				su.SkillUsageStats[this].AddSubSet(su.Stats);
				foreach (var m in Mods)
					m.Affect(su.SkillUsageStats[this]);
			}
			return _Range(this, su);
		}
		public IEnumerable<Tile> AoE(SkillUser su, Tile target) { return _GetAreaOfEffect(this, su, target); }
		protected void Effect(SkillUser su, Tile target, GameEvent ge) { _Effect(this, su, target, ge); }

		public IEnumerable<Mod> Mods { get; protected set; }

		internal virtual GameEvent Do(SkillUser doer, Tile target) {
			if(Range(doer).Any(t => t == target) && ValidTarget(doer, target)){
				var ge = new GameEvent();
				foreach (Tile t in AoE(doer, target)) {
					if(t.Inhabitant != null) ge.BeingTargets.Add(t.Inhabitant);
					Effect(doer, t, ge);
				}
				//GameEventLogger.Log(new GameEvent(stuff))
				return ge;
			}
			else return null;
		}

		public Skill(string name,
			Func<Skill, SkillUser, Tile, bool> targetValidator,
			Func<object, SkillUser, IEnumerable<Tile>> rangeGetter,
			Func<object, SkillUser, Tile, IEnumerable<Tile>> aoeGetter,
			Action<object, SkillUser, Tile, GameEvent> effect)
			: this(name, targetValidator, rangeGetter, aoeGetter, effect, new Mod[0]) { }

		public Skill(string name,
			Func<Skill, SkillUser, Tile, bool> targetValidator,
			Func<object, SkillUser, IEnumerable<Tile>> rangeGetter,
			Func<object, SkillUser, Tile, IEnumerable<Tile>> aoeGetter,
			Action<object, SkillUser, Tile, GameEvent> effect,
			IEnumerable<Mod> mods) {
				Name = name;
				_ValidTarget = targetValidator;
				_Range = rangeGetter;
				_GetAreaOfEffect = aoeGetter;
				_Effect = effect;
				Mods = mods.ToList();
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
			public static IEnumerable<Tile> UseWeaponRange(object key, SkillUser su) {
				return ((Being)su).MainHand.Range(key, su);
			}
			public static IEnumerable<Tile> GetFromMods(object key, SkillUser su) {
				return su.Place.GetArea((int)su.SkillUsageStats[key][StatType.Range]);
			}
		}
		private static class AoE {

			public static IEnumerable<Tile> UseWeapon(object key, SkillUser su, Tile target) {
				return ((Being)su).MainHand.AoE(key, su, target);
			}

			public static IEnumerable<Tile> TargetOnly(object key, SkillUser su, Tile target) {
				return new Tile[] { target };
			}
			public static IEnumerable<Tile> FromMods(object key, SkillUser su, Tile target) {
				return target.GetArea((int)su.SkillUsageStats[key][StatType.AreaOfEffect]);
			}
		}
		private static class Effect {
			public static void Damage(object key, SkillUser su, Tile target, GameEvent ge) {
				if (target.Inhabitant != null)
					ge.AddDamageApplication(target.Inhabitant, new Damage(target.Inhabitant, su.SkillUsageStats[key]));
			}

			//public static Func<object, SkillUser, Tile, GameEvent> Channel(Skill skill) {
			//	return (k, su, t) => {
			//		if (t.ChannelingInstance == null) {
			//			t.ChannelingInstance = new ChannelingInstance(su.Battle, su.GetChannelingMods(), skill, t);
			//		} else throw new Exception("bullshit");
			//	};
			//}
			//public static Func<object, SkillUser, Tile, GameEvent> AddModsToChannel(IEnumerable<Mod> mods) {
			//	return (k, su, t) => {
			//		if (t.ChannelingInstance != null) {
			//			foreach (var m in mods)
			//				m.Affect(t.ChannelingInstance.Stats);
			//		} else throw new Exception("bullshit");
			//	};
			//}
			public static Action<object, SkillUser, Tile, GameEvent> AddStatusEffect(Func<Battle, Being, StatSet, StatusEffect> statEffConstr) {
				return (k, su, t, ge) => {
					var target = t.Inhabitant;
					if (target != null) ge.AddStatusEffect(target, statEffConstr(su.Battle, target, su.Stats));
					else throw new Exception("bullshit");
				};
			}

			public static Action<object, SkillUser, Tile, GameEvent> DoWithWeapon(Action<object, SkillUser, Tile, GameEvent> effect) {
				return (k, su, t, ge) => {
					var b = su as Being;
					b.MainHand.AffectEffect(k, su, t, ge, effect);
				};
			}
			public static Action<object, SkillUser, Tile, GameEvent> DoWithWeapons(Action<object, SkillUser, Tile, GameEvent> effect) {
				return (k, su, t, ge) => {
					var b = su as Being;
					b.MainHand.AffectEffect(k, su, t, ge, effect);
					if(b.OffHand is Weapon) ((Weapon)b.OffHand).AffectEffect(k, su, t, ge, effect);
				};
			}
		}
		#endregion 

		public static Skill StandardAttack = new Skill("Standard attack",
			Validation.AnyAliveBeingInArea,
			Range.UseWeaponRange,
			AoE.UseWeapon,
			Effect.DoWithWeapon(Effect.Damage));
		//public static Skill ChannelSpeedUp = new Skill("Channel speedup",
		//	Validation.AnyChannelingInstanceInArea,
		//	Range.GetFromMods,
		//	AoE.TargetOnly,
		//	Effect.AddModsToChannel(new Mod[] { new AdditionMod(StatType.ChannelingSpeed, 3) }),
		//	new Mod[] { 
		//		new AdditionMod(StatType.Range, 6)
		//	});
		public static Skill GrantPhysResistance = new Skill("Physical resistance",
			Validation.AnyAliveBeingInArea,
			Range.GetFromMods,
			AoE.TargetOnly,
			Effect.AddStatusEffect((b, t, ss) => new TimedStatusEffect(b, t, new AdditionMod(StatType.Armour, 1), ss, 20)),
			new Mod[]{
				new AdditionMod(StatType.Range, 6)
			});
		public static Skill Bleed = new Skill("Bleed",
			Validation.AnyAliveBeingInArea,
			Range.GetFromMods,
			AoE.TargetOnly,
			Effect.AddStatusEffect((b, t, ss) => new TimedStatusEffect(b, t, new DamageOverTime(ss, 50, StatType.PhysicalDamage | StatType.DamageOverTime), ss, 100)),
			new Mod[]{
				new AdditionMod(StatType.Range, 6)
			});
		public static Skill Explosion = new Skill("Explosion",
			Validation.AnyAliveBeingInArea,
			Range.GetFromMods,
			AoE.FromMods,
			Effect.Damage,
			new Mod[] {
				new AdditionMod(StatType.Range, 6),
				new AdditionMod(StatType.AreaOfEffect, 2),
				new AdditionMod(StatType.FireDamage|StatType.Spell, 8)
			}
		);

		public static IEnumerable<Skill> Default = new Skill[]{
			StandardAttack,
			//ChannelSpeedUp,
			GrantPhysResistance,
			Explosion,
			Bleed
		};
	}
	
	public interface SkillUser {
		Tile Place { get; }
		//Weapon Weapon { get; }	//I'm a dumb fuck
		Battle Battle { get; }
		StatSet Stats { get; }
		Dictionary<object, StatSet> SkillUsageStats { get; }	
	}
}
