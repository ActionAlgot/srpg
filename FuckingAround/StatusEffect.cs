using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class StatusEffect {
		protected Being Target;

		public Action<Being> Affect { get; protected set; }
		public Action<Being> UnAffect { get; protected set; }
		
		public StatusEffect(Being target, IEnumerable<Mod> mods, StatSet ss) {
			Affect = b => {
				foreach (var m in mods)
					m.Affect(b.Stats); };
			UnAffect = b => {
				foreach (var m in mods)
					m.Unaffect(b.Stats); };
			Target = target;
		}

		public StatusEffect(Being Target, DamageOverTime DoT, StatSet ss) {
			Affect = b => b.AddDoT(DoT);
			UnAffect = b => b.RemoveDoT(DoT);
		}
	}
	public class TimedStatusEffect : StatusEffect, ITurnHaver {
		private void initStuff(int EffectTime) {
			Speed = 100.0 / EffectTime;
			Awaited = 0;
			TurnStarted += (s, e) => {
				Target.RemoveStatusEffect(this);
				Awaited = 0;
				TurnTracker.Remove(this);
			};
			TurnTracker.Add(this);
		}
		public TimedStatusEffect(Being target, Mod mod, StatSet ss, int EffectTime) : this(target, new Mod[] { mod }, ss, EffectTime) { }
		public TimedStatusEffect(Being target, IEnumerable<Mod> mods, StatSet ss, int EffectTime)
			: base(target, mods, ss) {
				initStuff(EffectTime);
		}
		public TimedStatusEffect(Being target, DamageOverTime DoT, StatSet ss, int EffectTime)
			: base(target, DoT, ss) {
			initStuff(EffectTime);
		}

		public event EventHandler TurnFinished;

		public double Speed {
			get;
			protected set;
		}

		public double Awaited {
			get;
			protected set;
		}

		public void Await(double time) {
			Awaited += time * Speed;
		}

		public event EventHandler TurnStarted;
		public void StartTurn() {
			TurnStarted(this, EventArgs.Empty);
			if(TurnFinished != null) TurnFinished(this, EventArgs.Empty);
		}
	}

}
