using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {
	public class GameEvent : EventArgs{
		public SkillUser Source;
		public Tile Target;
		public IEnumerable<Being> Targets;
		public Skill skill;

		public override string ToString() {
			return Source.ToString() + " used " + skill.Name + " on Tile:" + Target.ToString() + (Targets.Any() ? " affecting " + string.Join(", ", Targets.Select(t => t.ToString()).ToArray()) : "");
		}
	}

	public class GameSubEvent {

	}
}