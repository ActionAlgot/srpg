using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using System;

public class GBeingSelection : MonoBehaviour {

	public BattleController gbattle;
	public GameObject beingSelectDisplay;
	public GSkillMenu skilltree;

	private GameObject GBeingToAdd;
	private MetaBeing MetaBeingToAdd;
	private Dictionary<MetaBeing, GameObject> panels = new Dictionary<MetaBeing, GameObject>();

	public void Populate(IEnumerable<MetaBeing> mBeings) {
		foreach(var mb in mBeings) {
			var gO = Instantiate(beingSelectDisplay);
			var paneltem = gO.GetComponent<BeingSelectable>();

			paneltem.Name.text = mb.Name;
			paneltem.Add.onClick.AddListener(() => BeginBeingAdd(mb));
			paneltem.Edit.onClick.AddListener(() => skilltree.SetSkillTreeFiller(mb.SkillTreeFilling));
			gO.transform.SetParent(this.transform, false);

			panels[mb] = gO;
		}
	}

	private void BeginBeingAdd(MetaBeing mb) {
		MetaBeingToAdd = mb;
		GBeingToAdd = gbattle.DrawBeing(mb);
		panels[mb].SetActive(false);

		gbattle.tileset.TileHoverEnter += OnTileHoverEnter;
		gbattle.tileset.Clicked += OnTileClicked;
	}

	private void OnTileHoverEnter(object s, TileClickedEventArgs e) {
		GBeingToAdd.transform.position = new Vector3(e.Tile.X, e.Tile.Height * GTileS.HeightMultiplier, e.Tile.Y);
	}

	private void OnTileClicked(object s, TileClickedEventArgs e) {
		gbattle.AddBeing(MetaBeingToAdd, e.Tile.X, e.Tile.Y);
		var mb = MetaBeingToAdd;
		Cancel();
		panels[mb].SetActive(false);
	}

	private void Cancel() {
		if (MetaBeingToAdd != null) {
			Destroy(GBeingToAdd);
			panels[MetaBeingToAdd].SetActive(true);
			MetaBeingToAdd = null;

			gbattle.tileset.TileHoverEnter -= OnTileHoverEnter;
			gbattle.tileset.Clicked -= OnTileClicked;
		}
	}
}
