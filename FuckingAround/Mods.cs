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

		public static IEnumerable<double> AdditionMods(this IEnumerable<Mod> mods, StatType stat){
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Add
					&& m.TargetStat == stat)
				.Select(m => m.Value);
		}

		public static IEnumerable<double> MultiplicationMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Multiply
					&& m.TargetStat == stat)
				.Select(m => m.Value);
		}

		#region infRecursion safe auxiliary methods
		private static IEnumerable<Mod> ConversionToMods(this IEnumerable<Mod> mods, StatType stat, List<StatType> alreadyDoneStats) {
			alreadyDoneStats.Add(stat);
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Convert
					&& !alreadyDoneStats.Any(s => s == m.ConversionSource)
					&& m.TargetStat == stat)
				.Select(m => new Mod(m.TargetStat, ModifyingMethod.Add, m.Value * mods.GetStat(m.ConversionSource, alreadyDoneStats)));
		}
		private static double GetStat(this IEnumerable<Mod> mods, StatType stat, List<StatType> alreadyDoneStats) {
			mods = mods.Concat(mods.ConversionToMods(stat, alreadyDoneStats));
			double addedShit = mods.AdditionMods(stat).Sum();
			return addedShit + addedShit * mods.MultiplicationMods(stat).Sum();
		}
		#endregion

		public static IEnumerable<Mod> ConversionToMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Convert
					&& m.TargetStat == stat)
				.Select(m => new Mod(
					m.TargetStat,
					ModifyingMethod.Add,
					m.Value * mods.GetStat(m.ConversionSource, new List<StatType>(){stat})));	//TODO don't recurse
		}

		public static double GetStat(this IEnumerable<Mod> mods, StatType stat){
			mods = mods.Concat(mods.ConversionToMods(stat));
			return mods.AdditionMods(stat).Sum() * (1 + mods.MultiplicationMods(stat).Sum());
		}
	}

	public enum StatType {
		Strength, Speed, HP, MP, Armour,
		PhysicalDamage, FireDamage, IceDamage, LightningDamage, SpellDamage, MeeleeDamage,
		ChannelingSpeed,
		FireResistance, IceResistance, LightningResistance,
		ElementalDamage = FireDamage | IceDamage | LightningDamage,
		ElementalResistance = FireResistance | IceResistance | LightningResistance
	}

	public enum ModifyingMethod {
		Add, Multiply, Convert
	}
}
