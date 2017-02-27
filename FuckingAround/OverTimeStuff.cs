using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	public abstract class OverTime { }

	public class DamageOverTime : OverTime {

		protected double BaseDmg;	//per 100 time units
		public StatSet Damages;


		public DamageOverTime(StatSet ss, double BaseDmg, StatType DmgType)
			: this(ss, BaseDmg, DmgType, true) { }

		public DamageOverTime(StatSet ss, double BaseDmg, StatType DmgType, bool snapshotStats) {
			var asdfg = new StatSet();
			asdfg.AddSubSet(ss);
			new AdditionMod(DmgType, BaseDmg).Affect(asdfg);

			if (snapshotStats) {
				Damages = new StatSet();

				var DamageTypes = StatTypeStuff.DamageTypes
					.Select(asgf => asgf | StatType.DamageOverTime);

				foreach (var dmgt in DamageTypes) {
					var dmg = asdfg[dmgt];
					if (dmg != 0) {
						new AdditionMod(dmgt, asdfg[dmgt])
							.Affect(Damages);
						StatType pen = dmgt.AsPenetration();
						new AdditionMod(pen, asdfg[pen])
							.Affect(Damages);
					}
				}
			} else {
				Damages = asdfg;
			}
		}
	}

	public class OverTimeApplier : ITurnHaver {
		protected List<DamageOverTime> DoTs = new List<DamageOverTime>();
		protected Being Target;
		//protected healthregen

		public void Add(DamageOverTime DoT) {
			DoTs.Add(DoT);
		}
		public void Remove(DamageOverTime DoT) {
			DoTs.Remove(DoT);
			DoT.Damages.Dispose();
		}

		private double leftover;
		public double Effect {
			get {
				return DoTs
					.Select(DoT => {
						double r = 0;
						foreach (var DoTT in StatTypeStuff.DamageTypes.Select(asgf => asgf | StatType.DamageOverTime)) {
							var dmg = DoT.Damages[DoTT];
							if(dmg != 0)
								r += dmg * (1 - (Target[DoTT.AsResistance()].Value - DoT.Damages[DoTT.AsPenetration()]));
						}
						return r;
					})
					.Aggregate(0.0, (a, b) => a + b);
			}
		}

		public OverTimeApplier(Being target) {
			Target = target;
			TurnTracker.Add(this);
		}

		public event EventHandler TurnStarted;

		public event EventHandler TurnFinished;

		public void StartTurn() {
			if (TurnStarted != null) TurnStarted(this, EventArgs.Empty);
			if (TurnFinished != null) TurnFinished(this, EventArgs.Empty);
		}

		public double Speed {
			get {
				return (Effect / 100.0) / (Target.MaxHP / 100.0);
			}
		}

		public double Awaited {
			get { return Target.IsAlive ? (((double)(Target.MaxHP - Target.HP) + leftover)/ (double)Target.MaxHP) *100.0 : 0; }
		}

		public void Await(double time) {
			double dmg = Effect * (time / 100.0) + leftover;
			leftover = dmg - (int)dmg;
			if((int)dmg != 0) Target.TakeRawDamage((int)dmg);
		}
	}
}
