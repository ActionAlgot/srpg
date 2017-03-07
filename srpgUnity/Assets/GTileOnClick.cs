using System;
using UnityEngine;

public class GTileOnClick : MonoBehaviour {
	public event EventHandler OnClick;
	public void OnMouseDown() {
		if (OnClick != null) OnClick(this, EventArgs.Empty);
	}
}
