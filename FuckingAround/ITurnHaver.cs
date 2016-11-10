using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

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

	public static class TurnTracker {
		private static List<ITurnHaver> TurnHavers = new List<ITurnHaver>();

		public static void Add(ITurnHaver fuck){
			TurnHavers.Add(fuck);
		}

		public static void AddRange(IEnumerable<ITurnHaver> fuckers) {
			TurnHavers.AddRange(fuckers);
		}

		public static void Remove(ITurnHaver fuck){
			TurnHavers.Remove(fuck);
		}

		private static ITurnHaver _currentTurnHaver;
		public static ITurnHaver CurrentTurnHaver {
			get {	//automatically forward time if time to do so
				if (_currentTurnHaver == null || _currentTurnHaver.GetTimeToWait() > 0) {
					_currentTurnHaver = TurnHavers.Aggregate((t1, t2) => t1.GetTimeToWait() <= t2.GetTimeToWait() ? t1 : t2);
					var dsgfsdf = _currentTurnHaver.GetTimeToWait();
					foreach (var t in TurnHavers) t.Await(dsgfsdf);

					ConsoleLoggerHandlerOrWhatever.Log("_____________");
					foreach (var t in TurnHavers) ConsoleLoggerHandlerOrWhatever.Log(t.ToString() + " " + t.Awaited + " + " + t.Speed);

					_currentTurnHaver.StartTurn();
				}
				return _currentTurnHaver;
			}
		}

		public static IEnumerable<ChannelingInstance> GETCHANNELINGINSTANCES() {
			return TurnHavers.Where(t => t is ChannelingInstance).Cast<ChannelingInstance>();
		}
	}
}
