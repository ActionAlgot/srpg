using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using srpg;

public class GTileSetShit : MonoBehaviour {

	public GameObject GTile;
	private GTileS[,] tiles;

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
		tiles = new GTileS[ts.XLength, ts.YLength];

		foreach(var t in ts.AsEnumerable()) {
			var c = Instantiate(GTile);

			tiles[t.X, t.Y] = c.GetComponent<GTileS>();

			var old = c.transform.localScale;
			var blarg = c.GetComponent<GTileOnClick>();
			blarg.OnMouseHoverEnter += (s, e) => TileHoverEntered(s, new TileClickedEventArgs(t));
			blarg.OnMouseHoverExit += (s, e) => TileHoverExited(s, new TileClickedEventArgs(t));
			blarg.OnClick += (s, e) => Click(s, new TileClickedEventArgs(t));
			c.transform.SetParent(this.transform);
			c.transform.localScale = new Vector3(old.x, old.y * (1 + t.Height * GTileS.HeightMultiplier), old.z);
			old = c.transform.position;
			c.transform.localPosition = new Vector3(t.X, old.y + t.Height * GTileS.HeightMultiplier - c.transform.localScale.y/2, t.Y);
		}
	}

	public GTileS this[int x, int y] {
		get { return tiles[x, y]; }
	}
}
