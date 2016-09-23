using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Mod {
		public Stat TargetStat;
		public ModifyingMethod ModifyingMethod;
		public double Value;

		public Mod(Stat targetStat, ModifyingMethod modifyingmethod, double value) {
			TargetStat = targetStat;
			ModifyingMethod = modifyingmethod;
			Value = value;
		}

		public override string ToString() {
			return string.Format("{0} {1} by {2}",
				(Value > 0 ? "Increases " : "Decreases "),
				TargetStat,
				(ModifyingMethod == ModifyingMethod.Add ? Value.ToString() : (Value*100 + "%")));
		}
	}

	public class EquipmentMod : Mod {
		public bool Global;
		public EquipmentMod(Stat targetStat, ModifyingMethod modifyingmethod, double value, bool global)
			: base(targetStat, modifyingmethod, value) {
			Global = global;
		}
	}

	public static class ModEnumerableExtension {

		public static IEnumerable<double> AdditionMods(this IEnumerable<Mod> mods, Stat stat){
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Add
					&& m.TargetStat == stat)
				.Select(m => m.Value);
		}

		public static IEnumerable<double> MultiplicationMods(this IEnumerable<Mod> mods, Stat stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Multiply
					&& m.TargetStat == stat)
				.Select(m => m.Value);
		}

		public static double GetStat(this IEnumerable<Mod> mods, Stat stat){
			double addedShit = mods.AdditionMods(stat).Sum();
			return addedShit + addedShit * mods.MultiplicationMods(stat).Sum();
		}
	}

	public enum Stat {
		Strength, Speed, HP, MP, Armour,
		PhysicalDamage, FireDamage, IceDamage, LightningDamage, SpellDamage, MeeleeDamage,
		ChannelingSpeed,
		FireResistance, IceResistance, LightningResistance,
		ElementalDamage = FireDamage | IceDamage | LightningDamage,
		ElementalResistance = FireResistance | IceResistance | LightningResistance
	}

	public enum ModifyingMethod {
		Add, Multiply
	}
}
