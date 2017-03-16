using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using System;

public class GBeingSelection : MonoBehaviour {

	public GBattle gamecontroller;
	public GameObject beingSelectDisplay;
	public GSkillMenu skilltree;

	private IEnumerable<Being> LoadBeings() {
		return new Being[] {
			new Being(1, "being0"),
			new Being(1, "being1"),
			new Being(2, "being2")
		};
	}

	public void Populate() {
		float h = 0;
		foreach(var b in LoadBeings()) {
			var gO = Instantiate(beingSelectDisplay);
			var shit = gO.GetComponent<BeingSelectable>();

			shit.Name.text = b.Name;
			shit.Add.onClick.AddListener(() => {
				var gb = gamecontroller.DrawBeing(b);
				EventHandler<TileClickedEventArgs> hoverF = (s, e) =>
					gb.transform.position = new Vector3(e.Tile.X, gb.transform.position.y, e.Tile.Y);
				gamecontroller.GTileSet.TileHoverEnter += hoverF;
				gamecontroller.TileCLickHappening = (s, e) => {
					gamecontroller.GTileSet.TileHoverEnter -= hoverF;
					b.Place = e.Tile;
					gamecontroller.AddBeing(b, gb);
					gamecontroller.TileCLickHappening = null;
					Destroy(gO);
				};
			});
			shit.Edit.onClick.AddListener(() => skilltree.SetSkillTreeFiller(b.SkillTreeFilling));
			gO.transform.SetParent(this.transform, false);
			gO.transform.position -= Vector3.up * h;
			h += gO.GetComponent<RectTransform>().rect.height;
		}
	}
}
