using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets {
	public class MenuControl : MonoBehaviour {
		protected UnityWorldIO IO;

		public void StartBattle() {
			IO.TEMP_ForceEnterBattle();
		}

		private void Start() {
			IO = new UnityWorldIO();
			GameObject.Find("GameControl")
				.GetComponent<GameControl>()
				.SetIO(IO);
		}
	}
}
