using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	[Flags]
	public enum StatType {
		None = 0,
		Strength = 1 << 0, Dexterity = 1 << 1, Speed = 1 << 2, HP = 1 << 3, MP = 1 << 4,
		Damage = 1 << 6, Physical = 1 << 7, Fire = 1 << 8, Ice = 1 << 9, Lightning = 1 << 10,
		PhysicalDamage = Damage | Physical, FireDamage = Damage | Fire, IceDamage = Damage | Ice, LightningDamage = Damage | Lightning,
		ChannelingSpeed = 1 << 11,
		Resistance = 1 << 12, Threshold = 1 << 13, Penetration = 1 << 14,
		Armour = Resistance | Physical, FireResistance = Resistance | Fire, IceResistance = Resistance | Ice, LightningResistance = Resistance | Lightning,
		ArmourPenetration = Penetration | Physical, FirePenetration = Penetration | Fire,
		Range = 1 << 15, AreaOfEffect = 1 << 16, Weapon = 1 << 17, Spell = 1 << 18, WeaponRange = Weapon | Range
		//, Restriction = Weapon|Spell
	}
	public static class StatTypeExtensions {
		public static bool Supports(this StatType stat, StatType target) {
			return target.HasFlag(stat);
		}
	}

	public static class StatDictionaryExtensions {
		public static Stat GetStat(this Dictionary<StatType, Stat> std, StatType statType) {
			if (!std.ContainsKey(statType)) new Stat(statType, std);
			return std[statType];
		}
		public static Dictionary<StatType, Stat> Copy(this Dictionary<StatType, Stat> std) {
			var r = new Dictionary<StatType, Stat>();
			foreach (var s in std.Values)
				s.CreateCopy(r);
			return r;
		}
	}

	public class Stat : astat{

		public void CreateCopy(Dictionary<StatType, Stat> newOwner) {
			var that = new Stat(this.StatType, newOwner);
			that.Base = this.Base;
			that.AdditiveMultipliers = this.AdditiveMultipliers;
			foreach(var m in this.Multipliers)
				that.Multipliers.Add(m);
			that.Converters = this.Converters.ToList();
		}

		public Stat(StatType statType, Dictionary<StatType, Stat> owner){
			StatType = statType;
			Owner = owner;
			if (!Owner.ContainsKey(StatType)) Owner[StatType] = this;
			else if (Owner[StatType] != this) throw new ArgumentException("'owner' already contains a stat of StatType" + StatType);
			_multipliers.CollectionChanged += OnMultipliersChanged;
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
