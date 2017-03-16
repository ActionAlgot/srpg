using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using srpg;

public class GTileSetShit : MonoBehaviour {

	public GameObject GTile;

	public event EventHandler<TileClickedEventArgs> TileHoverEnter;
	public event EventHandler<TileClickedEventArgs> TileHoverExit;
	public event EventHandler<TileClickedEventArgs> Clicked;

	private void TileHoverEntered(object s, TileClickedEventArgs e) {
		if (TileHoverEnter != null) TileHoverEnter(s, e);	}
	private void TileHoverExited(object s, TileClickedEventArgs e) {
		if (TileHoverExit != null) TileHoverExit(s, e);	}
	private void Click(object s, TileClickedEventArgs e) {
		if (Clicked != null) Clicked(s, e);	}

	public void Build(TileSet ts) {
		foreach(var t in ts.AsEnumerable()) {
			var c = Instantiate(GTile);
			var old = c.transform.position;
			var blarg = c.GetComponent<GTileOnClick>();
			blarg.OnMouseHoverEnter += (s, e) => TileHoverEntered(s, new TileClickedEventArgs(t));
			blarg.OnMouseHoverExit += (s, e) => TileHoverExited(s, new TileClickedEventArgs(t));
			blarg.OnClick += (s, e) => Click(s, new TileClickedEventArgs(t));
			c.transform.SetParent(this.transform);
			c.transform.localPosition = new Vector3(t.X, old.y - 0.5f, t.Y);
		}
	}
}
