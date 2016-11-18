using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class StatusEffect {
		protected Being Target;
		
		public IEnumerable<Mod> Mods { get; private set; }
		public StatusEffect(Being target, IEnumerable<Mod> mods, StatSet ss) {
			Target = target;
			Mods = mods.ToList();
		}
	}
	public class TimedStatusEffect : StatusEffect, ITurnHaver {

		public TimedStatusEffect(Being target, Mod mod, StatSet ss, int EffectTime) : this(target, new Mod[] { mod }, ss, EffectTime) { }
		public TimedStatusEffect(Being target, IEnumerable<Mod> mods, StatSet ss, int EffectTime)
			: base(target, mods, ss) {
			Speed = 100.0 / EffectTime;
			Awaited = 0;
			TurnStarted += (s, e) => {
				Target.RemoveStatusEffect(this);
				Awaited = 0;
				TurnTracker.Remove(this);
			};
			TurnTracker.Add(this);
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
