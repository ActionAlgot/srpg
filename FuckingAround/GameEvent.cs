using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {
	public class GameEvent {
		public SkillUser Source;
		public Tile Target;
		public IEnumerable<Being> Targets;
		public Skill skill;

		public override string ToString() {
			return Source.ToString() + " used " + skill.Name + " on Tile:" + Target.ToString() + (Targets.Any() ? " affecting " + string.Join(", ", Targets) : "");
		}
	}
}