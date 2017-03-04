using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srpg {
	public static class ConsoleLoggerHandlerOrWhatever {
		public static event EventHandler<string> OnLog;
		public static void Log(string input) {
			if (OnLog != null) {
				OnLog(null, input);
			}
		}
	}
}
