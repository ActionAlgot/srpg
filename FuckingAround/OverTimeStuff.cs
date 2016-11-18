using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	public abstract class OverTime { }

	public class DamageOverTime : OverTime {

		protected double BaseDmg;	//per 100 time units
		public StatSet Damages = new StatSet();

		public DamageOverTime(Being target, double BaseDmg, StatType DmgType, StatSet ss)
			: this(target, new Mod[0], ss, BaseDmg, DmgType) { }

		protected DamageOverTime(Being target, IEnumerable<Mod> mods, StatSet ss, double BaseDmg, StatType DmgType) {

			var asdfg = new StatSet();
			asdfg.AddSubSet(ss);
			new AdditionMod(DmgType, BaseDmg).Affect(asdfg);

			//snapshot
			foreach (var dmgt in StatTypeStuff.DamageTypes.Select(asgf => asgf | StatType.DamageOverTime)) {
				new AdditionMod(dmgt, asdfg[dmgt]).Affect(Damages);
				var pen = ((StatType)(dmgt - StatType.Damage)) | StatType.Penetration;
				new AdditionMod(pen, asdfg[pen]).Affect(Damages);
			}
		}
	}

	public class OverTimeApplier : ITurnHaver {
		protected IEnumerable<DamageOverTime> DoTs;
		protected Being Target;
		//protected healthregen

		private double leftover;
		public double Effect {
			get {
				return DoTs
					.Select(DoT => {
						double r = 0;
						foreach(var DoTT in StatTypeStuff.DamageTypes.Select(asgf => asgf | StatType.DamageOverTime))
							r += DoT.Damages[DoTT] * (1 - (Target[DoTT.AsResistance()].Value - DoT.Damages[DoTT.AsPenetration()]));
						return r;
					})
					.Aggregate(0.0, (a, b) => a + b);
			}
		}

		public event EventHandler TurnStarted;

		public event EventHandler TurnFinished;

		public void StartTurn() {
			if (TurnStarted != null) TurnStarted(this, EventArgs.Empty);
			if (TurnFinished != null) TurnFinished(this, EventArgs.Empty);
		}

		public double Speed {
			get {
				return Target.MaxHP / (Effect * (1.0 / 100.0));
			}
		}

		public double Awaited {
			get { return (((double)(Target.MaxHP - Target.HP) + leftover)/ (double)Target.MaxHP) *100.0; }
		}

		public void Await(double time) {
			double dmg = Effect * (time / 100.0) + leftover;
			leftover = dmg - (int)dmg;
			Target.TakeRawDamage((int)dmg);
		}
	}
}
