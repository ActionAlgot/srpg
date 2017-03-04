using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class StatusEffect {
		protected Being Target;

		private Action<Being> _affect;
		private Action<Being> _unAffect;

		protected virtual void Affect() {
			_affect(Target);	}
		public virtual void UnAffect() {
			_unAffect(Target);
		}
		
		private StatusEffect(Being target, Action<Being> affect, Action<Being> unAffect, StatSet ss) {
			Target = target;
			_affect = affect;
			_unAffect = unAffect;

			Affect();
			Target.AddStatusEffect(this);
		}

		public StatusEffect(Being target, IEnumerable<Mod> mods, StatSet ss)
			: this(
				target,
				b => { foreach (var m in mods) m.Affect(b.Stats); },
				b => { foreach (var m in mods) m.Unaffect(b.Stats); },
				ss
			) { }

		public StatusEffect(Being target, DamageOverTime DoT, StatSet ss)
			: this(
				target,
				b => b.AddDoT(DoT),
				b => b.RemoveDoT(DoT),
				ss
			) { }
	}


	public class TimedStatusEffect : StatusEffect, ITurnHaver {
		private Battle _Battle;
		private void initStuff(int EffectTime) {
			Speed = 100.0 / EffectTime;
			Awaited = 0;
			_Battle.Add(this);
		}

		public override void UnAffect() {
			base.UnAffect();
			Awaited = 0;
			_Battle.Remove(this);
		}

		protected override void Affect(){
			base.Affect();
			TurnStarted += (s, e) => Target.RemoveStatusEffect(this);
		}

		public TimedStatusEffect(Battle battle, Being target, Mod mod, StatSet ss, int EffectTime) : this(battle, target, new Mod[] { mod }, ss, EffectTime) { }
		public TimedStatusEffect(Battle battle, Being target, IEnumerable<Mod> mods, StatSet ss, int EffectTime)
			: base(target, mods, ss) {
			_Battle = battle;	
			initStuff(EffectTime);
		}
		public TimedStatusEffect(Battle battle, Being target, DamageOverTime DoT, StatSet ss, int EffectTime)
			: base(target, DoT, ss) {
			_Battle = battle;
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
