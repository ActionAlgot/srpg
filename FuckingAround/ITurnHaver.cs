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
}
