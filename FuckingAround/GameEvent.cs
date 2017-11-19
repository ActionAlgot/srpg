using System;
using System.Collections.Generic;
using System.Linq;


namespace srpg {
	public class GameEvent {
		public SkillUser Source;
		public Tile Target;
		public List<Being> BeingTargets = new List<Being>();
		public Skill skill;
		public event EventHandler<EventArgs> PostApplication;
		public class Applications {
			public List<Damage> damages = new List<Damage>();
			public List<StatusEffect> statusEffects = new List<StatusEffect>();
		}
		public Dictionary<Being, Applications> applications = new Dictionary<Being, Applications>();

		internal void AddDamageApplication(Being target, Damage da) {
			if (!applications.ContainsKey(target))
				applications[target] = new Applications();
			applications[target].damages.Add(da);
		}
		internal void AddStatusEffect(Being target, StatusEffect se) {
			if (!applications.ContainsKey(target))
				applications[target] = new Applications();
			applications[target].statusEffects.Add(se);
		}

		public void Apply() {
			foreach (var b in BeingTargets) {
				var a = applications[b];
				foreach (var dmg in a.damages)
					b.TakeRawDamage(dmg.Value);
				foreach (var se in a.statusEffects)
					se.Affect();
			}
			if (PostApplication != null)
				PostApplication(this, EventArgs.Empty);
		}

		public override string ToString() {
			return Source.ToString() + " used " + skill.Name + " on Tile:" + Target.ToString() + (BeingTargets.Any() ? " affecting " + string.Join(", ", BeingTargets.Select(t => t.ToString()).ToArray()) : "");
		}
	}

}