using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public abstract class CommanderIO {

		private Being _subject;
		protected Being subject {
			get { return _subject; }
			set {
				_subject = value;
				if (SubjectChanged != null)
					SubjectChanged(this, new SubjectChangedArg(subject));
			}
		}
		public class SubjectChangedArg : EventArgs {
			Being Being;
			public SubjectChangedArg(Being b) { Being = b; }
		}

		public void SetSubject(Being being) { subject = being; }
		public event EventHandler<SubjectChangedArg> SubjectChanged;

		public abstract void DisplayMovementArea();
		public abstract void DisplayAvailableSkills();

		public Skill SelectedSkill;

		public void EndTurn() {
			subject.EndTurn();
		}
		public bool Do(Tile t) {
			if (!subject.ActionTaken && SelectedSkill != null) {
				bool done = false;
				if (subject.Perform(SelectedSkill, t)) done = true;
				else ConsoleLoggerHandlerOrWhatever.Log("Skill apply failed");
				SelectedSkill = null;
				return done;
			}
			else if (SelectedSkill == null && t.Inhabitant == null && !subject.Moved)
				return subject.Move(t);
			else if (!subject.ActionTaken && t.Inhabitant != null && t != subject.Place)
				return subject.Perform(subject.Skills.First(), t);  //standard attack
			return false;
		}
	}
}
