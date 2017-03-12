using System;

namespace srpg {
	
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
		public override string ToString() {
			return String.Format(
				"{0} additional {1}.",
				Value,
				TargetStatType
			);
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
		public override string ToString() {
			return string.Format(
				"{0}% additional {1}",
				Value * 100,
				TargetStatType
			);
		}
	}

	public class MultiplierMod : SuperStatCompatibleMod {
		double Value;
		public override void Affect(astat stat) { stat.AddMultiplier(Value); }
		public override void UnAffect(astat stat) {
			if (stat.RemoveMultiplier(Value)) return;
			else throw new ArgumentException("No multiplier to be removed was found");
		}

		public MultiplierMod(StatType targetStat, double value) {
			TargetStatType = targetStat;
			Value = value;
		}

		public override string ToString() {
			return string.Format(
				"{0}% more {1}",
				Value * 100,
				TargetStatType
			);
		}
	}

	public class Conversion{
		public StatType SourceType;
		public StatType TargetType;
		private Func<astat, Action<ComboStat>> _converter;

		public Conversion(StatType sourceType, StatType targetType, Func<astat, Action<ComboStat>> converter) {
			SourceType = sourceType;
			TargetType = targetType;
			_converter = converter;
		}

		public void Apply(astat source, ComboStat target) {
			_converter(source)(target);
		}
	}

	public abstract class InterStatularMod : Mod {
		public double Effectiveness { get; protected set; }
		public StatType SourceType { get; protected set; }
		public Conversion Conversion { get; protected set; }
	}

	public class ConversionMod : InterStatularMod {
		private SuperStatCompatibleMod SourceMod;
		private SuperStatCompatibleMod ResultMod;

		private Action<ComboStat> Converter(astat source) {
			SourceMod.UnAffect(source);
			ResultMod.Affect(source);
			return stat => stat.AddComponent(source);
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
			stat.Converters.Add(Conversion);
		}
		public override void UnAffect(Stat stat) {
			stat.Converters.Remove(Conversion);
		}

		public ConversionMod(StatType targetStat, double value, StatType sourceStat) {
			TargetStatType = targetStat;
			Effectiveness = value;
			SourceType = sourceStat;
			SourceMod = new MultiplierMod(sourceStat, 1 - value);
			ResultMod = new MultiplierMod(targetStat, value);

			Conversion = new Conversion(sourceStat, TargetStatType, Converter);
		}

		public override string ToString() {
			return string.Format(
				"{0}% of {1} converted to {2}",
				Effectiveness * 100,
				SourceType,
				TargetStatType
			);
		}
	}

	public class ConversionToAdditiveMultiplierMod : InterStatularMod {
		public ConversionToAdditiveMultiplierMod(StatType targetStat, double value, StatType sourceStat) {
			TargetStatType = targetStat;
			Effectiveness = value;
			SourceType = sourceStat;
			Conversion = new Conversion(SourceType, TargetStatType, source => target => target.AdditiveMultipliers += Effectiveness * source.Value);
		}

		public override void Affect(Stat stat) {
			stat.Converters.Add(Conversion);
		}
		public override void UnAffect(Stat stat) {
			stat.Converters.Remove(Conversion);
		}

		public override string ToString() {
			return string.Format(
				"{0}% additional {1} for each point in {2}",
				Effectiveness * 100,
				TargetStatType,
				SourceType
			);
		}
	}

	public class GainMod : InterStatularMod {
		private SuperStatCompatibleMod targetMod;

		public GainMod(StatType targetStat, double value, StatType sourceStat) {
			TargetStatType = targetStat;
			Effectiveness = value;
			SourceType = sourceStat;

			targetMod = new MultiplierMod(TargetStatType, Effectiveness);

			Conversion = new Conversion(SourceType, TargetStatType, source => target => {
				targetMod.Affect(source);
				target.AddComponent(source);
			});
		}

		public override void Affect(Stat stat) {
			stat.Converters.Add(Conversion);
		}
		public override void UnAffect(Stat stat) {
			stat.Converters.Remove(Conversion);
		}

		public override string ToString() {
			return string.Format(
				"{0}% of {1} gained as {2}",
				Effectiveness * 100,
				SourceType,
				TargetStatType
			);
		}
	}
}