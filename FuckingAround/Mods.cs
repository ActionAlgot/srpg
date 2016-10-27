using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	public class thing {
		public Dictionary<StatType, Stat> Stats;
		public IEnumerable<NewMod> mods;
		public thing() { }
	}
	
	public abstract class NewMod {
		public StatType TargetStatType { get; protected set; }
		public abstract void Affect(Stat stat);
		public abstract void UnAffect(Stat stat);
		public virtual void Affect(Dictionary<StatType, Stat> statD){
			if (!statD.ContainsKey(TargetStatType)) new Stat(TargetStatType, statD);
			Affect(statD[TargetStatType]);
		}
		public virtual void Unaffect(Dictionary<StatType, Stat> statD){
			UnAffect(statD[TargetStatType]);
		}
	}
	public abstract class SuperStatCompatibleMod : NewMod {
		public abstract void Affect(astat stat);
		public abstract void UnAffect(astat stat);
		public override void Affect(Stat stat) { Affect(stat as astat); }
		public override void UnAffect(Stat stat) { UnAffect(stat as astat); }
	}

	public class AdditionMod : SuperStatCompatibleMod {
		double Value;
		public override void Affect(astat stat) { stat.Base += Value; }
		public override void UnAffect(astat stat) { stat.Base -= Value; }
		public AdditionMod(StatType targetStat, double value) {
			TargetStatType = targetStat;
			Value = value;
		}
	}

	public class MultiplierMod : SuperStatCompatibleMod {
		double Value;
		public override void Affect(astat stat) { stat.Multipliers.Add(Value); }
		public override void UnAffect(astat stat) {
			if (stat.Multipliers.Remove(Value)) return;
			else throw new ArgumentException("No multiplier to be removed was found");
		}

		public MultiplierMod(StatType targetStat, double value) {
			TargetStatType = targetStat;
			Value = value;
		}
	}

	public class ConversionMod : NewMod {
		double Value;
		StatType sourceType;
		private SuperStatCompatibleMod SourceMod;
		private SuperStatCompatibleMod ResultMod;

		private astat Converter(Dictionary<StatType, Stat> std, StatType excluder) {
			if (sourceType.Supports(excluder)) {
				var r = new ComboStat(sourceType);
				ResultMod.Affect(r);	//probably pointless to mod the empty stat, but might as well
				return r;
			}
			if(!std.ContainsKey(sourceType)) new Stat(sourceType, std);
			var stat = std[sourceType].ExcludingStat(excluder);
			SourceMod.UnAffect(stat);
			ResultMod.Affect(stat);
			return stat;
		}

		public override void Affect(Dictionary<StatType, Stat> statD) {
			SourceMod.Affect(statD);
			base.Affect(statD);
		}
		public override void Unaffect(Dictionary<StatType, Stat> statD) {
			SourceMod.Unaffect(statD);
			base.Unaffect(statD);
		}
		public override void Affect(Stat stat) {
			stat.Converters.Add(Converter);
		}
		public override void UnAffect(Stat stat) {
			stat.Converters.Remove(Converter);
		}

		public ConversionMod(StatType targetStat, double value, StatType sourceStat) {
			TargetStatType = targetStat;
			Value = value;
			sourceType = sourceStat;
			SourceMod = new MultiplierMod(sourceStat, 1 - value);
			ResultMod = new MultiplierMod(targetStat, value);
		}
	}

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
					&& m.TargetStat.Supports(stat))
				.Select(m => m.Value);
		}

		private static IEnumerable<double> AdditiveMultiplicationMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.AdditiveMultiply
					&& m.TargetStat.Supports(stat))
				.Select(m => m.Value);
		}

		private static IEnumerable<double> MultiplicationMods(this IEnumerable<Mod> mods, StatType stat) {
			return mods
				.Where(m =>
					   m.ModifyingMethod == ModifyingMethod.Multiply
					&& m.TargetStat.Supports(stat))
				.Select(m => m.Value);
		}

		#region infRecursion safe auxiliary methods
		private static IEnumerable<Mod> ConversionToMods(this IEnumerable<Mod> mods, StatType stat, List<StatType> alreadyDoneStats) {
			alreadyDoneStats.Add(stat);
			return mods
				.Where(m =>
					   m.ModifyingMethod.HasFlag(ModifyingMethod.Convert)
					&& !alreadyDoneStats.Any(s => m.ConversionSource.Supports(s))
					&& m.TargetStat.Supports(stat))
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
			double sum = mods.AdditionMods(stat).Sum();
			sum *= (1 + mods.AdditiveMultiplicationMods(stat).Sum());
			sum *= mods.MultiplicationMods(stat).Aggregate(1.0, (a, b) => a * b);
			return sum;
		}
	}

	[Flags]
	public enum StatType {
		None = 0,
		Strength = 1<<0, Dexterity = 1<<1, Speed = 1<<2, HP = 1<<3, MP = 1<<4,
		Damage = 1<<6, Physical = 1<<7, Fire = 1<<8, Ice = 1<<9, Lightning = 1<<10,
		PhysicalDamage = Damage|Physical, FireDamage = Damage|Fire, IceDamage = Damage|Ice, LightningDamage = Damage|Lightning,
		ChannelingSpeed = 1<<11,
		Resistance = 1<<12, Threshold = 1<<13, Penetration = 1<<14,
		Armour = Resistance|Physical, FireResistance = Resistance|Fire, IceResistance = Resistance|Ice, LightningResistance = Resistance|Lightning,
		ArmourPenetration = Penetration|Physical, FirePenetration = Penetration|Fire,
		Range = 1<<15, AreaOfEffect = 1<<16, Weapon = 1<<17, Spell = 1<<18, WeaponRange = Weapon|Range
		//, Restriction = Weapon|Spell
	}
	public static class StatTypeExtensions {
		public static bool Supports(this StatType stat, StatType target) {
			return target.HasFlag(stat);
		}
	}

	[Flags]
	public enum ModifyingMethod {
		Add = 1<<0, Multiply = 1<<1, Convert = 1<<2,
		AdditiveMultiply = Add|Multiply
	}
}