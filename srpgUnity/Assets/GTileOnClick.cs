using System;
using UnityEngine;

public class GTileOnClick : MonoBehaviour {
	public event EventHandler OnClick;
	public event EventHandler OnMouseHoverEnter;
	public event EventHandler OnMouseHoverExit;
	public void OnMouseDown() {
		if (OnClick != null) OnClick(this, EventArgs.Empty);
	}
	public void OnMouseEnter() {
		if (OnMouseHoverEnter != null) OnMouseHoverEnter(this, EventArgs.Empty);
	}
	public void OnMouseExit() {
		if (OnMouseHoverExit != null) OnMouseHoverExit(this, EventArgs.Empty);
	}
}
