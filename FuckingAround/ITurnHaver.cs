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

		//lmao can't fucking connect eventhandler and keep shit dynamic
		private void _TurnStarted(object s, EventArgs e) {
			if(TurnStarted != null) TurnStarted(s, e); }
		private void _TurnFinished(object s, EventArgs e) {
			if (TurnFinished != null) TurnFinished(s, e); }
		public event EventHandler TurnStarted;
		public static event EventHandler TurnFinished;

		private List<ITurnHaver> TurnHavers = new List<ITurnHaver>();

		public TurnTracker() {
			TurnFinished += (s, e) => { var imDumb = CurrentTurnHaver; };
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
		private ITurnHaver _currentTurnHaver;
		public ITurnHaver CurrentTurnHaver {
			get {	//automatically forward time if time to do so
				if (_currentTurnHaver == null || _currentTurnHaver.GetTimeToWait() > 0) {
					_currentTurnHaver = TurnHavers.Aggregate((t1, t2) => t1.GetTimeToWait() <= t2.GetTimeToWait() ? t1 : t2);
					var dsgfsdf = _currentTurnHaver.GetTimeToWait();

					enumerating = true;
					foreach (var t in TurnHavers)
						t.Await(dsgfsdf);
					enumerating = false;
					while (doAfterEnumerating.Any())
						doAfterEnumerating.Dequeue()();

					ConsoleLoggerHandlerOrWhatever.Log("_____________");
					foreach (var t in TurnHavers) ConsoleLoggerHandlerOrWhatever.Log(t.ToString() + " " + t.Awaited + " + " + t.Speed);
					
					_currentTurnHaver.StartTurn();
				}
				return _currentTurnHaver;
			}
		}
	}
}