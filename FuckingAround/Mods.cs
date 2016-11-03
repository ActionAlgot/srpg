using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	
	public abstract class Mod {
		public StatType TargetStatType { get; protected set; }
		public abstract void Affect(Stat stat);
		public abstract void UnAffect(Stat stat);
		public virtual void Affect(StatSet statD){
			Affect(statD.GetStat(TargetStatType));
		}
		public virtual void Unaffect(StatSet statD){
			UnAffect(statD.GetStat(TargetStatType));
		}
	}
	public abstract class SuperStatCompatibleMod : Mod {
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

	public class AdditiveMultiplierMod : SuperStatCompatibleMod {
		double Value;
		public override void Affect(astat stat) { stat.AdditiveMultipliers += Value; }
		public override void UnAffect(astat stat) { stat.AdditiveMultipliers -= Value; }
		public AdditiveMultiplierMod(StatType targetStat, double value) {
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

	public class ConversionMod : Mod {
		double Value;
		StatType sourceType;
		private SuperStatCompatibleMod SourceMod;
		private SuperStatCompatibleMod ResultMod;

		private astat Converter(StatSet ss, StatType excluder) {
			if (sourceType.Supports(excluder)) {
				var r = new ComboStat(sourceType);
				ResultMod.Affect(r);	//probably pointless to mod the empty stat, but might as well do it
				return r;
			}
			var stat = ss.GetStat(sourceType).ExcludingStat(excluder);
			SourceMod.UnAffect(stat);
			ResultMod.Affect(stat);
			return stat;
		}

		public override void Affect(StatSet statD) {
			SourceMod.Affect(statD);
			base.Affect(statD);
		}
		public override void Unaffect(StatSet statD) {
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
}