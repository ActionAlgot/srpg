using System;
using srpg;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class UnityCommanderIO : CommanderIO {

	protected GTileSetShit tileset;
	private List<GTileS> ActiveMovementArea;

	public UnityCommanderIO(GTileSetShit ts) {
		tileset = ts;
	}

	public override void DisplayMovementArea() {
			ActiveMovementArea = new List<GTileS>();
		foreach(var t in _GetMovementArea()) {
			var curr = tileset[t.X, t.Y];
			curr.MovementAreaHighlight.SetActive(true);
			ActiveMovementArea.Add(curr);
		}
	}

	public override void DisplayAvailableSkills() {
		throw new NotImplementedException();
	}

	public override void UndisplayMovementArea() {
		foreach (var t in ActiveMovementArea)
			t.MovementAreaHighlight.SetActive(false);
	}

	public override void UndisplayAvailableSkills() {
		throw new NotImplementedException();
	}
}
