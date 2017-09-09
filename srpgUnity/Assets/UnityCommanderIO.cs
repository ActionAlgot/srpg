using System;
using srpg;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

class UnityCommanderIO : CommanderIO {

	protected GTileSetShit tileset;
	protected GameObject CommandsPanel;
	protected Button ButtonPrototype;
	private Dictionary<Skill, GameObject> SkillButtons = new Dictionary<Skill, GameObject>();
	private Transform EndTurnButton;
	
	private List<GTileS> ActiveMovementArea;

	public UnityCommanderIO(GTileSetShit ts, GameObject commandPanel, Button button){
		tileset = ts;
		CommandsPanel = commandPanel;
		ButtonPrototype = button;

		var butt = UnityEngine.Object.Instantiate(ButtonPrototype);
		butt.GetComponentInChildren<Text>().text = "End turn";
		butt.onClick.AddListener(EndTurn);
		butt.transform.SetParent(CommandsPanel.transform);
		EndTurnButton = butt.transform;

	}

	public override void DisplayMovementArea() {
		ActiveMovementArea = new List<GTileS>();
		foreach(var t in _GetMovementArea()) {
			var curr = tileset[t.X, t.Y];
			curr.MovementAreaHighlight.SetActive(true);
			ActiveMovementArea.Add(curr);
		}
	}

	private void CreateSkillButton(Skill skill) {
		var butt = UnityEngine.Object.Instantiate(ButtonPrototype);
		butt.GetComponentInChildren<Text>().text = skill.Name;
		butt.onClick.AddListener(() => SelectedSkill = skill);
		butt.transform.SetParent(CommandsPanel.transform);
		SkillButtons[skill] = butt.gameObject;
	}

	public override void DisplayAvailableSkills() {
		foreach(var skill in _GetSkills()) {
			if (!SkillButtons.ContainsKey(skill)) {
				CreateSkillButton(skill);
				EndTurnButton.SetAsLastSibling();
			}
			SkillButtons[skill].SetActive(true);
		}
	}

	public override void UndisplayMovementArea() {
		foreach (var t in ActiveMovementArea)
			t.MovementAreaHighlight.SetActive(false);
	}

	public override void UndisplayAvailableSkills() {
		foreach (var skill in _GetSkills())
			SkillButtons[skill].SetActive(false);
	}
}
