using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using System.Linq;

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
			DrawBeing(b);
		}

		battle.BeingAdded += (s, e) => DrawBeing(e.Being);

		var stf = new SkillTreeFiller(SkillTreeshit.Basic);
		var av = stf.Available.ToList();
		stf.Take(av[2]);
		av = stf.Available.ToList();
		stf.Take(av[2]);

		foreach (Mod m in stf.Taken.SelectMany(sn => sn.Mods))
			Debug.Log(m);

		var ben = new Being(battle, 1, "being0", 5, 6, stf);
		
		//b.AddPassiveSkill(Passives.All[3]);
		//b.AddPassiveSkill(Passives.All[4]);
		ben.Inventory[0] = new Spear(12);
		new Being(battle, 1, "being1", 7, 8);
		new Being(battle, 2, "being2", 9, 10);

		battle.Paused = false;
	}

	private void DrawBeing(Being b) {
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
	
	// Update is called once per frame
	void Update () {
		
	}
}
