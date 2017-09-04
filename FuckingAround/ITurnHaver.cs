using System;
using System.Collections.Generic;
using System.Linq;

namespace srpg {

	public interface ITurnHaver {
		event EventHandler TurnStarted;
		event EventHandler TurnFinished;
		void StartTurn();
		double Speed { get; }
		double Awaited { get; }
		void Await(double time);
	}

	public static class ITurnHaverExtensions {
		public static double GetTimeToWait(this ITurnHaver ith) {
			return (100 - ith.Awaited) / ith.Speed;
		}
	}

	public class TurnTracker {
		private void _TurnStarted(object s, EventArgs e) {
			if(TurnStarted != null) TurnStarted(s, e); }
		private void _TurnFinished(object s, EventArgs e) {
			if (TurnFinished != null) TurnFinished(s, e); }
		public event EventHandler TurnStarted;
		public static event EventHandler TurnFinished;

		private List<ITurnHaver> TurnHavers = new List<ITurnHaver>();

		private bool _paused = true;
		public bool Paused {
			get { return _paused; }
			set {
				_paused = value;
				ForwardTime();
			}
		}

		public TurnTracker() {
			TurnFinished += (s, e) => {
				CurrentTurnHaver = null;
				ForwardTime();
			};
		}

		public void Add(ITurnHaver fuck){
			if (!enumerating) {
				TurnHavers.Add(fuck);
				fuck.TurnStarted += _TurnStarted;
				fuck.TurnFinished += TurnFinished;
			}
			else doAfterEnumerating.Enqueue(() => Add(fuck));
		}

		public void AddRange(IEnumerable<ITurnHaver> fuckers) {
			if (!enumerating) {
				TurnHavers.AddRange(fuckers);
				foreach(var fuck in fuckers) {
					fuck.TurnStarted += _TurnStarted;
					fuck.TurnFinished += TurnFinished;
				}
			}
			else doAfterEnumerating.Enqueue(() => AddRange(fuckers));
		}

		public void Remove(ITurnHaver fuck){
			if (!enumerating) {
				TurnHavers.Remove(fuck);
				fuck.TurnStarted -= _TurnStarted;
				fuck.TurnFinished -= TurnFinished;
			}
			else doAfterEnumerating.Enqueue(() => Remove(fuck));
		}
		private Queue<Action> doAfterEnumerating = new Queue<Action>();
		private bool enumerating = false;
		public ITurnHaver CurrentTurnHaver { get; private set; }

		private bool looping = false;
		private void loop() {	//if not done like this autoending ITHs will recursively call each other
			looping = true;
			ITurnHaver lastITH = CurrentTurnHaver;
			lastITH.StartTurn();
			while (lastITH != CurrentTurnHaver)	//this means if lastITHs turn ended immediately
				CurrentTurnHaver.StartTurn();
			looping = false;
		}

		private void ForwardTime() {
			if (CurrentTurnHaver == null || CurrentTurnHaver.GetTimeToWait() > 0) {
				if (Paused) return;
				if (TurnHavers.Any() == false)
					return;

				CurrentTurnHaver = TurnHavers.Aggregate((t1, t2) => t1.GetTimeToWait() <= t2.GetTimeToWait() ? t1 : t2);
				var dsgfsdf = CurrentTurnHaver.GetTimeToWait();

				enumerating = true;
				foreach (var t in TurnHavers)
					t.Await(dsgfsdf);
				enumerating = false;
				while (doAfterEnumerating.Any())
					doAfterEnumerating.Dequeue()();

				ConsoleLoggerHandlerOrWhatever.Log("_____________");
				foreach (var t in TurnHavers) ConsoleLoggerHandlerOrWhatever.Log(t.ToString() + " " + t.Awaited + " + " + t.Speed);

				if (!looping)
					loop();	//this calls StartTurn
			}
		}
	}
}