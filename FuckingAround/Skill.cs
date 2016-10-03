using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Skill {

		public string Name { get; protected set; }
		public Func<Skill, SkillUser, Tile, bool> _ValidTarget { get; protected set; }
		public Func<Skill, SkillUser, IEnumerable<Tile>> _Range { get; protected set; }
		public Func<Skill, SkillUser, Tile, IEnumerable<Tile>> _GetAreaOfEffect { get; protected set; }
		public Action<Skill, SkillUser, Tile> _Effect { get; protected set; }

		public IEnumerable<Tile> Range(SkillUser su) {
			return _Range(this, su);
		}

		public IEnumerable<Mod> Mods { get; protected set; }

		public virtual bool Do(SkillUser doer, Tile target) {
			if(_ValidTarget(this, doer, target) && _Range(this, doer).Any(t => t == target)){
				foreach (Tile t in _GetAreaOfEffect(this, doer, target)) {
					_Effect(this, doer, t);

					//GameEventLogger.Log(new GameEvent(stuff))
				}
				return true;
			}
			else return false;
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
	}

	public static class SkillsRepo {
		#region subFuncs
		private static class Validation {
			public static bool AnyAliveBeingInArea(Skill skill, SkillUser su, Tile target) {
				return skill._GetAreaOfEffect(skill, su, target).Any(t2 => t2.Inhabitant != null && t2.Inhabitant.IsAlive);
			}
			public static bool AliveBeingIsTarget(Skill skill, SkillUser su, Tile target) {
				return target.Inhabitant != null && target.Inhabitant.IsAlive;
			}
			public static bool NoChannelingInstance(Skill skill, SkillUser su, Tile target) {
				return target.ChannelingInstance == null;
			}
			public static bool AnyChannelingInstanceInArea(Skill skill, SkillUser su, Tile target){
				return skill._GetAreaOfEffect(skill, su, target).Any(t2 => t2.ChannelingInstance != null);
			}
		}
		private static class Range {
			public static IEnumerable<Tile> UseWeaponRange(Skill skill, SkillUser su){
				return su.Place.GetArea(su.Weapon.Range);
			}
			public static IEnumerable<Tile> GetFromMods(Skill skill, SkillUser su) {
				return su.Place.GetArea((int)skill.Mods.Concat(su.Mods).GetStat(StatType.Range));
			}
		}
		private static class AoE {
			public static IEnumerable<Tile> TargetOnly(Skill skill, SkillUser su, Tile target) {
				return new Tile[] { target };
			}
			public static IEnumerable<Tile> FromMods(Skill skill, SkillUser su, Tile target) {
				return target.GetArea((int)skill.Mods.Concat(su.Mods).GetStat(StatType.AreaOfEffect));
			}
		}
		private static class Effect {
			public static void WeaponDamage(Skill skill, SkillUser su, Tile target) {
				if(target.Inhabitant != null) target.Inhabitant.TakeDamage(skill.Mods.Concat(su.Mods).Concat(su.Weapon.Mods));
			}
			public static Action<Skill, SkillUser, Tile> Channel(Skill skill) {
				return (s, su, t) => {
					if (t.ChannelingInstance == null) {
						t.ChannelingInstance = new ChannelingInstance(su.Mods, skill, t);
						TurnTracker.Add(t.ChannelingInstance);
					} else throw new Exception("bullshit");
				};
			}
			public static Action<Skill, SkillUser, Tile> AddModsToChannel(IEnumerable<Mod> mods) {
				return (s, su, t) => {
					if(t.ChannelingInstance != null){
						t.ChannelingInstance.AddMods(mods);
					} else throw new Exception("bullshit");
				};
			}
		}
		#endregion 

		public static Skill StandardAttack = new Skill("Standard attack",
				Validation.AnyAliveBeingInArea,
				Range.UseWeaponRange,
				AoE.TargetOnly,
				Effect.WeaponDamage);
		public static Skill Blackify = new Skill("Blackify",
				Validation.AnyAliveBeingInArea,
				Range.GetFromMods,
				AoE.FromMods,
				(s, su, t) => { if(t.Inhabitant != null) t.Inhabitant.Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);},
				new Mod[]{
					new Mod(StatType.Range, ModifyingMethod.Add, 6),
					new Mod(StatType.AreaOfEffect, ModifyingMethod.Add, 2)
				});
		public static Skill BlackifyChannel = new Skill("Blackify channeling",
				Validation.NoChannelingInstance,
				Range.GetFromMods,
				AoE.TargetOnly,
				Effect.Channel(Blackify),
				new Mod[]{
					new Mod(StatType.Range, ModifyingMethod.Add, 6)
				});
		public static Skill ChannelSpeedUp = new Skill("Channel speedup",
			Validation.AnyChannelingInstanceInArea,
			Range.GetFromMods,
			AoE.TargetOnly,
			Effect.AddModsToChannel(new Mod[] { new Mod(StatType.ChannelingSpeed, ModifyingMethod.Add, 3) }),
			new Mod[] { 
				new Mod(StatType.Range, ModifyingMethod.Add, 6)
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
		Weapon Weapon { get; }	//I'm a dumb fuck

		IEnumerable<Mod> Mods { get; }	
	}
}
