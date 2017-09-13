using System;
using System.Collections.Generic;

namespace srpg {
	public class StatusEffect {
		protected Being Target;
		private bool DESTROY = false;
		private bool ADDED = false;
		
		private List<Mod> _mods = new List<Mod>();
		private List<DamageOverTime> _dots = new List<DamageOverTime>();

		public virtual void Affect() {
			if (!ADDED) {
				ADDED = true;
				foreach (var m in _mods)
					m.Affect(Target.Stats);
				foreach (var dot in _dots)
					Target.AddDoT(dot);
				Target.AddStatusEffect(this);
			}
		}
		public virtual void UnAffect() {
			if (!DESTROY) {
				DESTROY = true;
				foreach (var m in _mods)
					m.Unaffect(Target.Stats);
				foreach (var dot in _dots)
					Target.RemoveDoT(dot);
				Target.RemoveStatusEffect(this);
			}
		}

		public StatusEffect(Being target, IEnumerable<Mod> mods, IEnumerable<DamageOverTime> dots, StatSet ss) {
			Target = target;
			_mods.AddRange(mods);
			_dots.AddRange(dots);
		}

		public StatusEffect(Being target, IEnumerable<Mod> mods, StatSet ss)
			: this(
				target,
				mods,
				new List<DamageOverTime>(),
				ss
			) { }

		public StatusEffect(Being target, DamageOverTime DoT, StatSet ss)
			: this(
				target,
				new List<Mod>(),
				new List<DamageOverTime>() { DoT },
				ss
			) { }
	}


	public class TimedStatusEffect : StatusEffect, ITurnHaver {
		private Battle _Battle;
		private void initStuff(int EffectTime) {
			Speed = 100.0 / EffectTime;
			Awaited = 0;
		}

		public override void UnAffect() {
			base.UnAffect();
			Awaited = 0;
			_Battle.Remove(this);
		}

		public override void Affect(){
			base.Affect();
			TurnStarted += (s, e) => UnAffect();
			_Battle.Add(this);
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
