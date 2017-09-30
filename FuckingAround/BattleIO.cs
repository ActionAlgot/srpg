using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace srpg {
	public abstract class BattleIO {

		protected BattleIO() {
			PreSubjectChanged += OnPreSubjectChanged;
			PostSubjectChanged += OnPostSubjectChanged;
			PostSkillChanged += OnPostSkillChanged;
		}

		private bool newTurnStarted = false;	//for use in 'Do'-method in which subject may change while running

		private Being _subject;
		private Being subject {
			get { return _subject; }
			set {
				newTurnStarted = true;
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

		public void SetSubject(Being being) { subject = being; }
		public event EventHandler PreSubjectChanged;
		public event EventHandler<SubjectChangedArg> PostSubjectChanged;

		private void OnPreSubjectChanged(object s, EventArgs e) {
			SelectedSkill = null;
			_UnDisplayMovementArea();
			_UndisplayAvailableSkills();
		}
		private void OnPostSubjectChanged(object s, EventArgs e) {
			_DisplayMovementArea();
			_DisplayAvailableSkills();
		}

		protected IEnumerable<Tile> _GetMovementArea() {
			return subject.MovementArea;
		}
		protected IEnumerable<Skill> _GetSkills() {
			return subject.Skills;
		}

		public abstract void DisplayMovementArea();
		private void _DisplayMovementArea() { if (subject != null) DisplayMovementArea(); }
		public abstract void UndisplayMovementArea();
		private void _UnDisplayMovementArea() { if (subject != null) UndisplayMovementArea(); }
		public abstract void DisplayAvailableSkills();
		private void _DisplayAvailableSkills() { if (subject != null) DisplayAvailableSkills(); }
		public abstract  void UndisplayAvailableSkills();
		private void _UndisplayAvailableSkills() { if (subject != null) UndisplayAvailableSkills(); }

		public event EventHandler PreSkillChanged;
		public event EventHandler PostSkillChanged;

		private void OnPostSkillChanged(object s, EventArgs e) {
			if (SelectedSkill != null) _UnDisplayMovementArea();
			else if (newTurnStarted == false) _DisplayMovementArea();
		}

		protected GameEvent PendingGameEvent;
		protected abstract void DisplayGameEvent();

		private Skill _selectedSkill;
		protected Skill SelectedSkill {
			get { return _selectedSkill; }
			set {
				if (PreSkillChanged != null) PreSkillChanged(this, new EventArgs());
				_selectedSkill = value;
				if (PostSkillChanged != null) PostSkillChanged(this, new EventArgs());
			}
		}

		protected virtual void CancelGameEvent() {
			PendingGameEvent = null;
		}
		protected virtual void ConfirmGameEvent() {
			PendingGameEvent.Apply();
			PendingGameEvent = null;
			SelectedSkill = null;

			newTurnStarted = false;
			subject.ActionTaken = true; //TODO make Actiontaken private
			if (!newTurnStarted) {
				_UndisplayAvailableSkills();
				_DisplayMovementArea();
			}
		}

		//TODO properly tell subclass to show endturn option
		protected void EndTurn() {
			subject.EndTurn();
		}

		protected bool TryUseSkill(Skill skill, Tile target) {
			var ge = skill.Do(subject, target);
			if (ge != null) {
				PendingGameEvent = ge;
				DisplayGameEvent();
				return true;
			}
			return false;
		}

		public bool Do(Tile t) {

			if (PendingGameEvent != null) return false;

			if (subject != null) {
				if (!subject.ActionTaken && SelectedSkill != null)
					return TryUseSkill(SelectedSkill, t);
				else if (!subject.ActionTaken && t.Inhabitant != null && t != subject.Place)
					return TryUseSkill(subject.Skills.First(), t); //standard attack
				else if (SelectedSkill == null && t.Inhabitant == null && !subject.Moved) {
					newTurnStarted = false;
					if (subject.Move(t)) {
						if (!newTurnStarted)
							_UnDisplayMovementArea();
						return true;
					}
				}
			}
			return false;
		}
	}
}
