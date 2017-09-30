using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public class Game {
		//public List<Being> Roster;
		//public List<Gear> Inventory;

		private Battle _Battle;
		public Battle Battle {
			get { return _Battle; }
			protected set {
				_Battle = value;
				WorldIO.EnterBattle();
			}
		}
		private WorldIO _WorldIO;
		public WorldIO WorldIO {
			get { return _WorldIO; }
			set {
				_WorldIO = value;
				_WorldIO.SetGame(this);
			}
		}

		public void TEMP_ForceEnterBattle() {
			Battle = new Battle();
		}
	}
}
