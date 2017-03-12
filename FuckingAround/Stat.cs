using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace srpg {

	[Flags]
	public enum StatType {
		None = 0,
		Strength = 1 << 0, Dexterity = 1 << 1, Speed = 1 << 2, HP = 1 << 3, MP = 1 << 4,
		OverTime = 1<<5, DamageOverTime = Damage|OverTime,
		Damage = 1 << 6, Physical = 1 << 7, Fire = 1 << 8, Ice = 1 << 9, Lightning = 1 << 10,
		PhysicalDamage = Damage | Physical, FireDamage = Damage | Fire, IceDamage = Damage | Ice, LightningDamage = Damage | Lightning,
		ChannelingSpeed = 1 << 11,
		Resistance = 1 << 12, Threshold = 1 << 13, Penetration = 1 << 14,
		Armour = Resistance | Physical, FireResistance = Resistance | Fire, IceResistance = Resistance | Ice, LightningResistance = Resistance | Lightning,
		ArmourPenetration = Penetration | Physical, FirePenetration = Penetration | Fire,
		Range = 1 << 15, AreaOfEffect = 1 << 16, Weapon = 1 << 17, Spell = 1 << 18, WeaponRange = Weapon | Range,
		MovementPoints = 1 << 19
	}
	public static class StatTypeStuff {
		public static readonly IEnumerable<StatType> DamageTypes = new StatType[]{
			StatType.PhysicalDamage, StatType.FireDamage, StatType.LightningDamage, StatType.IceDamage };
		public static readonly IEnumerable<StatType> DamageApplicationTypes = new StatType[]{
			StatType.Weapon, StatType.Spell, StatType.OverTime };
		public static readonly IEnumerable<StatType> DirectDamageApplicationTypes = new StatType[]{
			StatType.Weapon, StatType.Spell };
		public static readonly IEnumerable<StatType> DirectDamageTypeApplicationTypes =
			DirectDamageApplicationTypes.SelectMany(da => DamageTypes.Select(dt => da|dt));
	}
	public static class StatTypeExtensions {
		public static bool Supports(this StatType stat, StatType target) {
			return (target&stat) == stat;
		}
		public static StatType AsPenetration(this StatType stat) {
			return stat & ~StatType.Damage & ~StatType.Resistance & ~StatType.Threshold | StatType.Penetration;
		}
		public static StatType AsResistance(this StatType stat) {
			return stat & ~StatType.Damage & ~StatType.Penetration & ~StatType.Threshold | StatType.Resistance;
		}
		public static StatType AsDamage(this StatType stat) {
			return stat & ~StatType.Resistance & ~StatType.Penetration & ~StatType.Threshold | StatType.Damage;
		}
		public static StatType AsThreshold(this StatType stat) {
			return stat & ~StatType.Resistance & ~StatType.Penetration & ~StatType.Damage | StatType.Threshold;
		}
	}

	public abstract class astat {
		public StatType StatType { get; protected set; }
		public virtual double Value { get; protected set; }
		public virtual double Base { get; set; }
		public virtual double AdditiveMultipliers { get; set; }
		public abstract void AddMultiplier(double m);
		public abstract void AddMultipliers(IEnumerable<double> ms);
		public abstract bool RemoveMultiplier(double m);
		public abstract IEnumerable<double> Multipliers { get; }
		//public abstract astat Copy();
	}

	public class Stat : astat {

		public void CreateCopy(StatSet newOwner) {
			var that = new Stat(this.StatType, newOwner);
			that.Base = this.Base;
			that.AdditiveMultipliers = this.AdditiveMultipliers;
			foreach(var m in this.Multipliers)
				that.AddMultiplier(m);
			that.Converters = this.Converters.ToList();
		}

		public Stat(StatType statType, StatSet owner){
			//_multipliers.CollectionChanged += OnMultipliersChanged;
			StatType = statType;
			Owner = owner;
			Owner.AddStat(this);
			if (ValueUpdated != null)
				ValueUpdated(this, new ValueUpdatedEventArgs(StatType));
		}

		public class ValueUpdatedEventArgs : EventArgs {
			public StatType StatType;
			public ValueUpdatedEventArgs(StatType st) {
				StatType = st;
			}
		}

		private void Update(Action setter) {
			setter();
			//if (UpToDate) {
			UpToDate = false;
			if (ValueUpdated != null)
				ValueUpdated(this, new ValueUpdatedEventArgs(StatType));
			//}
		}
		private T Update<T>(Func<T> setter) {
			T r = setter();
			//if (UpToDate) {
			UpToDate = false;
			if (ValueUpdated != null)
				ValueUpdated(this, new ValueUpdatedEventArgs(StatType));
			//}
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


		//private ObservableCollection<double> _multipliers = new ObservableCollection<double>();
		private Collection<double> _multipliers = new Collection<double>();
		public override IEnumerable<double> Multipliers { get { return _multipliers.AsEnumerable(); } }
		public override void AddMultiplier(double m) {
			Update(() => _multipliers.Add(m)); }
		public override void AddMultipliers(IEnumerable<double> ms) {
			Update(() => { foreach (var m in ms) _multipliers.Add(m); }); }
		public override bool RemoveMultiplier(double m) {
			return Update(() => _multipliers.Remove(m));
		}
		//private void OnMultipliersChanged(object sender, NotifyCollectionChangedEventArgs e) {
		//	if (e.Action != NotifyCollectionChangedAction.Move) {
		//		Update(() => { });
		//	}
		//}

		public void RaiseUpdatedEvent() {
			if (UpToDate) {
				UpToDate = false;
				ValueUpdated(this, new ValueUpdatedEventArgs(this.StatType));
			}
		}
		public event EventHandler<ValueUpdatedEventArgs> ValueUpdated;

		public List<Conversion> Converters = new List<Conversion>();
		public IEnumerable<Conversion> ConvertersAndSupportingConverters {
			get { return Converters.Concat(SupportingStats.SelectMany(ss => ss.Converters)); }
		}

		private StatSet Owner;
		private IEnumerable<Stat> SupportingStats { get {
			if (Owner == null) return new Stat[0];
			else return Owner.GetSupporting(StatType)
				.Where(s => s != this);
		} }
		
		public void ApplyConversions(ComboStat target) { ApplyConversions(target, StatType.None); }
		public void ApplyConversions(ComboStat target, StatType excluder) {
			foreach (var c in ConvertersAndSupportingConverters) {
				if (!c.TargetType.Supports(excluder) && !c.SourceType.Supports(excluder)) {
					var st = this.StatType & ~c.TargetType | c.SourceType;	//inherit conversion in sourcetype as well, IE physical weapon damage is to be converted if physical damage is converted
					c.Apply(Owner.GetStat(st).ExcludingStat(excluder | this.StatType), target);
				}
			}
		}
		
		public void Invalidate() { UpToDate = false; }	//Flags the stat for recalculation but does not raise ValueUpdated event
		private bool updt;
		private bool UpToDate {
			get{return updt;}
			set {
				ConsoleLoggerHandlerOrWhatever.Log(this.StatType + " UpToDate: " + updt + " => " + value);
				updt = value;
			}
		}

		public override double Value { get { return FullStat.Value; } }
		private ComboStat _fullStat;
		public ComboStat FullStat {
			get {
				if (!UpToDate) UpdateFullStat();
				return _fullStat;
			}
		}

		protected void UpdateFullStat() {
			var r = new ComboStat(SupportingStats.Cast<astat>(), this);
			ApplyConversions(r);
			_fullStat = r;
			UpToDate = true;
		}
		public ComboStat ExcludingStat(StatType excluder) {

			if (this.StatType.Supports(excluder)) return new ComboStat(this.StatType);

			var r = new ComboStat(SupportingStats.Where(ss => !ss.StatType.Supports(excluder)).Cast<astat>(), this);
			ApplyConversions(r, excluder);
			return r;
		}
		public double LoneValue { get { return Base * (1 + AdditiveMultipliers) * Multipliers.Aggregate(0.0, (a, b) => a * b); } }
		public Stat() {
			//_multipliers.CollectionChanged += OnMultipliersChanged;
		}
	}
}
