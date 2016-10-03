using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	public class Mod {
		public StatType TargetStat;
		public ModifyingMethod ModifyingMethod;
		public double Value;
		public StatType ConversionSource;

		public Mod(StatType targetStat, ModifyingMethod modifyingmethod, double value) {
			TargetStat = targetStat;
			ModifyingMethod = modifyingmethod;
			Value = value;
		}

		public override string ToString() {
			return string.Format("{0} {1} by {2}",
				(Value > 0 ? "Increases " : "Decreases "),
				TargetStat,
				ModifyingMethod == ModifyingMethod.Add ? Value.ToString() : (Value*100 + "%"));
		}
	}

	public class EquipmentMod : Mod {
		public bool Global;	//Affect stats outside of the equipment itself
		public EquipmentMod(StatType targetStat, ModifyingMethod modifyingmethod, double value, bool global)
			: base(targetStat, modifyingmethod, value) {
			Global = global;
		}
	}

	public static class ModEnumerableExtension {

		private static IEnumerable<double> AdditionMods(this IEnumerable<Mod> mods, StatType stat){
			return mods
				.Where(m => 
					   m.ModifyingMethod == ModifyingMethod.Add
					&& m.TargetStat.HasFlag(stat))
				.Select(m => m.Value);
		}

		private static IEnumerable<double> AdditiveMultiplicationMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.AdditiveMultiply
					&& m.TargetStat.HasFlag(stat))
				.Select(m => m.Value);
		}

		private static IEnumerable<double> MultiplicationMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Multiply
					&& m.TargetStat.HasFlag(stat))
				.Select(m => m.Value);
		}

		#region infRecursion safe auxiliary methods
		private static IEnumerable<Mod> ConversionToMods(this IEnumerable<Mod> mods, StatType stat, List<StatType> alreadyDoneStats) {
			alreadyDoneStats.Add(stat);
			return mods
				.Where(m =>
					   m.ModifyingMethod.HasFlag(ModifyingMethod.Convert)
					&& !alreadyDoneStats.Any(s => s == m.ConversionSource)
					&& m.TargetStat.HasFlag(stat))
				.Select(m => new Mod(
					m.TargetStat,
					(ModifyingMethod)(m.ModifyingMethod - ModifyingMethod.Convert),
					m.Value * mods.GetStat(m.ConversionSource, alreadyDoneStats.ToList())));
		}
		private static double GetStat(this IEnumerable<Mod> mods, StatType stat, List<StatType> alreadyDoneStats) {
			mods = mods.Concat(mods.ConversionToMods(stat, alreadyDoneStats.ToList()));
			return mods.MultiplicationMods(stat).Aggregate(
				mods.AdditionMods(stat).Sum() * (1 + mods.AdditiveMultiplicationMods(stat).Sum()),
				(a, b) => a * b);
		}
		#endregion

		private static IEnumerable<Mod> ConversionToMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod.HasFlag(ModifyingMethod.Convert)
					&& m.TargetStat.HasFlag(stat))
				.Select(m => new Mod(
					m.TargetStat,
					(ModifyingMethod)(m.ModifyingMethod - ModifyingMethod.Convert),
					m.Value * mods.GetStat(m.ConversionSource, new List<StatType>(){stat})));	//TODO don't recurse
		}

		public static double GetStat(this IEnumerable<Mod> mods, StatType stat){
			mods = mods.Concat(mods.ConversionToMods(stat));
			return mods.MultiplicationMods(stat)
				.Aggregate(
					mods.AdditionMods(stat).Sum() * (1 + mods.AdditiveMultiplicationMods(stat).Sum()),
					(a, b) => a * b);
		}
	}

	[Flags]
	public enum StatType {
		Strength = 1<<0, Dexterity = 1<<1, Speed = 1<<2, HP = 1<<3, MP = 1<<4,
		Damage = 1<<6, Physical = 1<<7, Fire = 1<<8, Ice = 1<<9, Lightning = 1<<10,
		PhysicalDamage = Damage|Physical, FireDamage = Damage|Fire, IceDamage = Damage|Ice, LightningDamage = Damage|Lightning,
		ChannelingSpeed = 1<<11,
		Resistance = 1<<12, Threshold = 1<<13, Penetration = 1<<14,
		Armour = Resistance|Physical, FireResistance = Resistance|Fire, IceResistance = Resistance|Ice, LightningResistance = Resistance|Lightning,
		ArmourPenetration = Penetration|Physical, FirePenetration = Penetration|Fire,
		Range = 1<<15, AreaOfEffect = 1<<16
	}

	[Flags]
	public enum ModifyingMethod {
		Add = 1<<0, Multiply = 1<<1, Convert = 1<<2,
		AdditiveMultiply = Add|Multiply
	}
}