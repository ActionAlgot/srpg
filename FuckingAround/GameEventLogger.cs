using System;
using System.Collections.Generic;

namespace srpg {
	public static class GameEventLogger {
		private static List<GameEvent> log = new List<GameEvent>();
		public static event EventHandler<GameEvent> OnNewLog;
		public static void Log(GameEvent gameEvent) {
			log.Add(gameEvent);
			if (OnNewLog != null) OnNewLog(null, gameEvent);
		}
	}
}
