using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets {
	public class MenuControl : MonoBehaviour {
		private srpg.Game game;

		public void StartBattle() {
			game.TEMP_ForceEnterBattle();
		}

		private void Start() {
			game = GameObject.Find("GameControl")
				.GetComponent<Assets.GameControl>()
				.game;
		}
	}
}
