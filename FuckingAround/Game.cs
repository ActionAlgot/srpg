using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srpg {
	public class Game {
		public List<MetaBeing> Roster = new List<MetaBeing>() {
			new MetaBeing("being0"), new MetaBeing("being1"), new MetaBeing("being2")
		};
		//public List<Gear> Inventory;

		private Battle _Battle;
		public Battle Battle {
			get { return _Battle; }
			protected set {
				_Battle = value;
				if (BattleEntered != null) BattleEntered(this, EventArgs.Empty);
			}
		}

		public event EventHandler BattleEntered;

		public void TEMP_ForceEnterBattle() {
			Battle = new Battle(this);
		}

		public Game() {
			foreach (var mb in Roster)
				mb.AddSkills(SkillsRepo.Default);
		}
	}
}
