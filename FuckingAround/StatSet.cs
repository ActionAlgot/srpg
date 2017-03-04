using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srpg {
	public class StatSet {
		private Dictionary<StatType, Stat> MainSet = new Dictionary<StatType, Stat>();
		private List<StatSet> SubSets = new List<StatSet>();

		public StatSet() {
			ValueUpdated += OnValueUpdated;
		}

		public void AddSubSet(StatSet ss) {
			SubSets.Add(ss);
			ss.ValueUpdated += this.ValueUpdated;
		}

		public void AddStat(Stat stat) {
			if (!MainSet.ContainsKey(stat.StatType)) MainSet[stat.StatType] = stat;
			else /*if (MainSet[stat.StatType] != stat)*/ throw new ArgumentException("StatSet already contains a stat of StatType " + stat.StatType);
			stat.ValueUpdated += this.ValueUpdated;
		}

		public IEnumerable<Stat> GetSupporting(StatType st) {
			return MainSet.Values.Where(s => s.StatType.Supports(st))
				.Concat(SubSets.SelectMany(ss => ss.GetSupporting(st)));
		}
		public IEnumerable<Stat> GetSupporting(StatType st, StatType excluding) {
			return MainSet.Values.Where(s => s.StatType.Supports(st) && !s.StatType.Supports(excluding))
				.Concat(SubSets.SelectMany(ss => ss.GetSupporting(st, excluding)));
		}

		public event EventHandler<Stat.ValueUpdatedEventArgs> ValueUpdated;
		private void OnValueUpdated(object sender, Stat.ValueUpdatedEventArgs e) {
			var st = ((Stat)sender).StatType;
			foreach (var s in MainSet.Values)
				if (s.ConvertersAndSupportingConverters.Any(c => st.Supports(c.SourceType)))
					s.RaiseUpdatedEvent();
			foreach (var s in MainSet.Values)
				if (st.Supports(s.StatType))
					s.Invalidate();
		}

		public Stat GetStat(StatType st) {
			if (!MainSet.ContainsKey(st))
				new Stat(st, this);
			return MainSet[st];
		}

		public double this[StatType statType] {
			get {
				if (!MainSet.ContainsKey(statType))
					new Stat(statType, this);
				return MainSet[statType].Value;
			}
		}

		public void Dispose() {	//really should implement iDisposable yada yada
			foreach(var ss in SubSets)
				ss.ValueUpdated -= this.ValueUpdated;
		}

		private IEnumerable<StatType> GetInstantiatedStatTypes() {
			return MainSet.Keys.Union(SubSets.SelectMany(ss => ss.GetInstantiatedStatTypes()));
		}

		public IEnumerable<Stat> AsEnumerableStats() {
			return GetInstantiatedStatTypes().Select(st => GetStat(st));
		}
	}
}
