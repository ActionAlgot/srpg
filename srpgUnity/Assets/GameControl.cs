using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using UnityEngine.SceneManagement;

namespace Assets {
public class GameControl : MonoBehaviour {
		public Game game;
		public Battle GETBATTLE() {
			return game.Battle;
		}
		// Use this for initialization
		void Start () {
			DontDestroyOnLoad(this.gameObject);

			game.BattleEntered += (s, e) => SceneManager.LoadScene(1);
		}

		private void Awake() {
			game = new Game();
		}
	}
}
