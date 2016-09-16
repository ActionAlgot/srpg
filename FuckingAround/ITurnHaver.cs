using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckingAround {

	public interface ITurnHaver {
		event EventHandler TurnFinished;
		double Speed { get; }
		double Awaited { get; }
		void Await(double time);
	}

	public static class ITurnHaverExtensions {
		public static double GetTimeToWait(this ITurnHaver ith) {
			return (100 - ith.Awaited) / ith.Speed;
		}
	}

	public class TurnFuckYouFuckThatFuckEverything {
		private List<ITurnHaver> turnfyuckers;

		public TurnFuckYouFuckThatFuckEverything(){
			turnfyuckers = new List<ITurnHaver>();
		}

		public void Add(ITurnHaver fuck){
			turnfyuckers.Add(fuck);
		}

		public void AddRange(IEnumerable<ITurnHaver> fuckers) {
			turnfyuckers.AddRange(fuckers);
		}

		public void Remove(ITurnHaver fuck){
			turnfyuckers.Remove(fuck);
		}

		private ITurnHaver _currentTurnHaver;
		public ITurnHaver CurrentTurnHaver {
			get {	//automatically forward time if time to do so
				if (_currentTurnHaver == null || _currentTurnHaver.GetTimeToWait() > 0) {
					_currentTurnHaver = turnfyuckers.Aggregate((t1, t2) => t1.GetTimeToWait() <= t2.GetTimeToWait() ? t1 : t2);
					var dsgfsdf = _currentTurnHaver.GetTimeToWait();
					foreach (var t in turnfyuckers) t.Await(dsgfsdf);

					ConsoleLoggerHandlerOrWhatever.Log("_____________");
					foreach (var t in turnfyuckers) ConsoleLoggerHandlerOrWhatever.Log(t.ToString() + " " + t.Awaited + " + " + t.Speed);

				}
				return _currentTurnHaver;
			}
		}

		public IEnumerable<ChannelingInstance> GETCHANNELINGINSTANCES() {
				return turnfyuckers.Where(t => t is ChannelingInstance).Cast<ChannelingInstance>();
		}
	}
}
