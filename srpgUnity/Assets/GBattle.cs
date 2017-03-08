using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;

public class GBattle : MonoBehaviour {

	public GameObject GTile;
	public GameObject GBeing;
	public GameObject BeingInfoPanel;
	public GameObject BeingInfoText;
	public GameObject BeingCommandPanel;
	public GameObject CommandMenuHandler;

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

			var binft = Instantiate(BeingInfoText);
			var binft2 = binft.GetComponent<BeingToTextHandler>();
			binft.transform.position += new Vector3(0, 100000, 0);
			binft2.SetBeing(b);
			b.TurnStarted += (s, e) => binft.transform.position -= new Vector3(0, 100000, 0);
			b.TurnFinished += (s, e) => binft.transform.position += new Vector3(0, 100000, 0);
			binft.transform.SetParent(BeingInfoPanel.transform, false);

			var cmh = Instantiate(CommandMenuHandler);
			cmh.transform.SetParent(BeingCommandPanel.transform, false);
			cmh.GetComponent<CommanMenuHandler>().CreateItems(b);
			cmh.transform.position -= new Vector3(0, 100000, 0);
			b.TurnStarted += (s, e) => cmh.transform.position += new Vector3(0, 100000, 0);
			b.TurnFinished += (s, e) => cmh.transform.position -= new Vector3(0, 100000, 0);
		}
		var trash = battle.activeBeing;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
