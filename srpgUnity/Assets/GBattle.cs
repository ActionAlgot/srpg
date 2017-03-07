using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;

public class GBattle : MonoBehaviour {

	public GameObject GTile;
	public GameObject GBeing;

	// Use this for initialization
	public void Start () {
		var battle = new Battle();
		foreach (var t in battle.TileSet.AsEnumerable()) {
			var c = Instantiate(GTile);
			var old = c.transform.position;
			c.GetComponent<GTileOnClick>().OnClick += (s, e) => battle.TileSet.SelectTile(t);
			c.transform.position = new Vector3(t.X, old.y - 0.5f, t.Y);
			
		}
		foreach(var b in battle.Beings) {
			var gb = Instantiate(GBeing);
			var old = gb.transform.position;
			b.MoveStarted += (s, e) => gb.GetComponent<GBeingMover>().Move(e.Path);
			gb.transform.position = new Vector3(b.Place.X, old.y, b.Place.Y);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
