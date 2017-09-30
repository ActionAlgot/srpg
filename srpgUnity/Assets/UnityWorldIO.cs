using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using srpg;
using UnityEngine.SceneManagement;

namespace Assets {
	public class UnityWorldIO : WorldIO {
		public override void EnterBattle() {
			SceneManager.LoadScene(1);
		}
	}
}
