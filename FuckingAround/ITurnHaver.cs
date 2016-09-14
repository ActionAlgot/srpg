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
		double TimeToWait { get; }
		void Await(double time);
	}
}
