using System;

namespace srpg {
	public static class ConsoleLoggerHandlerOrWhatever {
		public class LogEventArgs : EventArgs{
			public string text;
			public LogEventArgs(string txt) {
				text = txt;
			}
		}
		public static event EventHandler<LogEventArgs> OnLog;
		public static void Log(string input) {
			if (OnLog != null) {
				OnLog(null, new LogEventArgs(input));
			}
		}
	}
}
