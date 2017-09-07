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
				if (PreSubjectChanged != null)
					PreSubjectChanged(this, new EventArgs());
				_subject = value;
				if (PostSubjectChanged != null)
					PostSubjectChanged(this, new SubjectChangedArg(subject));
			}
		}
		public class SubjectChangedArg : EventArgs {
			Being Being;
			public SubjectChangedArg(Being b) { Being = b; }
		}

		public void SetSubject(Being being) { subject = being; DisplayMovementArea(); }
		public event EventHandler PreSubjectChanged;
		public event EventHandler<SubjectChangedArg> PostSubjectChanged;

		private void OnPreSubjectChanged(object s, EventArgs e) {
			_UnDisplayMovementArea(); }
		private void OnPostSubjectChanged(object s, EventArgs e) {
			_DisplayMovementArea(); }

		protected IEnumerable<Tile> _GetMovementArea() {
			return subject.MovementArea;
		}
		public abstract void DisplayMovementArea();
		private void _DisplayMovementArea() { if (subject != null) DisplayMovementArea(); }
		public abstract void UndisplayMovementArea();
		private void _UnDisplayMovementArea() { if (subject != null) UndisplayMovementArea(); }
		public abstract void DisplayAvailableSkills();
		private void _DisplayAvailableSkills() { if (subject != null) DisplayAvailableSkills(); }
		public abstract void UndisplayAvailableSkills();
		private void _UndisplayAvailableSkills() { if (subject != null) UndisplayAvailableSkills(); }

		public Skill SelectedSkill;

		public void EndTurn() {
			subject.EndTurn();
		}
		public bool Do(Tile t) {
			if (subject != null) {
				if (!subject.ActionTaken && SelectedSkill != null) {
					bool done = false;
					if (subject.Perform(SelectedSkill, t)) done = true;
					else ConsoleLoggerHandlerOrWhatever.Log("Skill apply failed");
					SelectedSkill = null;
					return done;
				}
				else if (SelectedSkill == null && t.Inhabitant == null && !subject.Moved) {
					if (subject.Move(t)) {
						_UnDisplayMovementArea();
						return true;
					}
					return false;
				}
				else if (!subject.ActionTaken && t.Inhabitant != null && t != subject.Place)
					return subject.Perform(subject.Skills.First(), t);  //standard attack
				return false;
			}
			else return false;
		}
	}
}
