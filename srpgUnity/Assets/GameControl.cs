using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;

namespace Assets {
public class GameControl : MonoBehaviour {
		private Game game;
		public Battle GETBATTLE() {
			return game.Battle;
		}
		// Use this for initialization
		void Start () {
			DontDestroyOnLoad(this.gameObject);
		}

		public void SetIO(WorldIO IO) {
			game.WorldIO = IO;
		}
		public void SetIO(BattleIO IO) {
			game.Battle.SetIO(IO);
		}

		private void Awake() {
			if(game == null ) game = new Game();
		}
	}
}
