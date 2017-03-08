using System;
using System.Collections.Generic;
using UnityEngine;
using srpg;

public class BeingToTextHandler : MonoBehaviour {
	private Being Being;
	public void SetBeing(Being being) {
		Being = being;
		GetComponent<UnityEngine.UI.Text>().text = string.Format(
			"{0}: {1}\n{2}: {3}\n{4}: {5}",
			"NAME", Being.Name,
			"HP", (Being.HP + "/" + Being.MaxHP),
			"AWA", Being.Awaited
			);
		Being.HPChanged += SetHealth;
	}
	private void SetHealth(object s, Being.HPChangedEventArgs e) {
		int strindex = 6 + Being.Name.Length + 2 +3;
		GetComponent<UnityEngine.UI.Text>().text =
			GetComponent<UnityEngine.UI.Text>().text.Substring(0, strindex)
			+ e.Curr
			+ GetComponent<UnityEngine.UI.Text>().text.Substring(strindex + 2+(e.Prev%10));
	}
}
