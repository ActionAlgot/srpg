using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class Stat : astat{

		public Stat(StatType statType, Dictionary<StatType, Stat> owner){
			StatType = statType;
			Owner = owner;
			if (!Owner.ContainsKey(StatType)) Owner[StatType] = this;
			else if (Owner[StatType] != this) throw new ArgumentException("'owner' already contains a stat of StatType" + StatType);
		}

		public class ValueUpdatedEventArgs : EventArgs {
			public double PrevValue;
			public ValueUpdatedEventArgs(double prevVal) {
				PrevValue = prevVal;
			}
		}

		private void Recalc() {
			LoneValue = Base * (1 + AdditiveMultipliers) * Multipliers.Aggregate(0.0, (a, b) => a * b);
		}

		private void Update(Action setter) {
			var prevVal = LoneValue;
			setter();
			Recalc();
			if (ValueUpdated != null)
				ValueUpdated(this, new ValueUpdatedEventArgs(prevVal));
		}
		private T Update<T>(Func<T> setter) {
			var prevVal = LoneValue;
			T r = setter();
			Recalc();
			if (ValueUpdated != null)
				ValueUpdated(this, new ValueUpdatedEventArgs(prevVal));
			return r;
		}

		private double _base;
		public override double Base {
			get { return _base; }
			set { Update(() => _base = value); }
		}
		private double _additiveMultipliers;
		public override double AdditiveMultipliers {
			get { return _additiveMultipliers; }
			set { Update(() => _additiveMultipliers = value); }
		}


		private ObservableCollection<double> _multipliers = new ObservableCollection<double>();
		public override ICollection<double> Multipliers { get { return _multipliers; } }
		private void OnMultipliersChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action != NotifyCollectionChangedAction.Move) {
				Update(() => { });
			}
		}

		public event EventHandler<ValueUpdatedEventArgs> ValueUpdated;

		public List<Func<Dictionary<StatType, Stat>, StatType, astat>> Converters =
			new List<Func<Dictionary<StatType, Stat>, StatType, astat>>();
		public IEnumerable<Func<Dictionary<StatType, Stat>, StatType, astat>> ConvertersAndSupportingConverters {
			get { return Converters.Concat(SupportingStats.SelectMany(ss => ss.Converters)); }
		}

		private Dictionary<StatType, Stat> Owner;
		private IEnumerable<Stat> SupportingStats { get {
			if (Owner == null) return new Stat[0];
			else return Owner
				.Select(kv => kv.Value)
				.Where(s => s.StatType.Supports(this.StatType) && s != this);
		} }

		public IEnumerable<astat> GetConversions() {
			return ConvertersAndSupportingConverters.Select(c => c(Owner, StatType));
		}
		public IEnumerable<astat> GetConversionsExcluding(StatType excluder) {
			return ConvertersAndSupportingConverters
				.Select(c => c(Owner, excluder | this.StatType))
				.Where(s => !s.StatType.Supports(excluder));
		}

		public override double Value { get { return FullStat.Value; } }
		public ComboStat FullStat {
			get {
				var r = new ComboStat(SupportingStats, this);
				foreach (var cs in GetConversions())
					if (cs.StatType.Supports(this.StatType)) throw new ArgumentException(String.Format("Illegal conversion: {0} to {1}", cs.StatType, this.StatType));
					else if (this.StatType.Supports(cs.StatType)) throw new ArgumentException(String.Format("Illegal conversion: {0} to {1}", this.StatType, cs.StatType));
					else r.AddComponent(cs);
				return r;
			}
		}
		public ComboStat ExcludingStat(StatType excluder) {
			var r = new ComboStat(SupportingStats.Where(ss => !ss.StatType.Supports(excluder)), this);
			foreach (var cs in GetConversionsExcluding(excluder))
				if (cs.StatType.Supports(this.StatType)) throw new ArgumentException(String.Format("Illegal conversion: {0} to {1}", cs.StatType, this.StatType));
				else if (this.StatType.Supports(cs.StatType)) throw new ArgumentException(String.Format("Illegal conversion: {0} to {1}", this.StatType, cs.StatType));
				else r.AddComponent(cs);
			return r;
		}
		public double LoneValue { get; private set; }
		public Stat() {
			_multipliers.CollectionChanged += OnMultipliersChanged;
		}
	}

	public abstract class astat {
		public StatType StatType { get; protected set; }
		public virtual double Value { get; protected set; }
		public virtual double Base { get; set; }
		public virtual double AdditiveMultipliers { get; set; }
		public abstract ICollection<double> Multipliers { get; }
	}
}
