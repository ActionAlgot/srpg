using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using srpg;

public class BattleController : MonoBehaviour {

	public GTileSetShit tileset;
	public Button ButtonPrototype;
	public GameObject CommandsPanel;
	public GameObject GBeingPrototype;
	public GameEventDisplayS GEDS;
	public GameObject BeingInfoTextPrototype;
	public GameObject BeingInfoPanel;
	public GBeingSelection beingSelector;

	protected Skill SelectedSkill;
	protected Battle Battle;
	private GameEvent _PendingGameEvent;
	protected GameEvent PendingGameEvent {
		get { return _PendingGameEvent; }
		set {
			_PendingGameEvent = value;
			if (value != null) DisplayGameEvent();
	}	}
	protected Being ActiveBeing {  get { return Battle.activeBeing; } }
	private GameObject ActiveBeingInfo;
	protected List<GTileS> ActiveMovementArea;
	public Dictionary<Being, GameObject> GBeings = new Dictionary<Being, GameObject>();
	public Dictionary<Being, GameObject> BeingInfo = new Dictionary<Being, GameObject>();
	private Dictionary<Skill, GameObject> SkillButtons = new Dictionary<Skill, GameObject>();
	private Transform EndTurnButton;
	
	
	protected void Start() {
		Battle = GameObject.Find("GameControl")
				.GetComponent<Assets.GameControl>()
				.GETBATTLE();

		Battle.BeingMoved += (s, e) => GBeings[s as Being].GetComponent<GBeingMover>().Move(e.Path);
		Battle.BeingTurnStarted += OnActiveBeingChanged;
		Battle.BeingAdded += (s, e) => BeingSetUp(e.Being);

		GEDS.Confirm.onClick.AddListener(ConfirmGameEvent);
		GEDS.Cancel.onClick.AddListener(CancelGameEvent);

		var butt = Instantiate(ButtonPrototype);
		butt.GetComponentInChildren<Text>().text = "End turn";
		butt.onClick.AddListener(EndTurn);
		butt.transform.SetParent(CommandsPanel.transform);
		EndTurnButton = butt.transform;

		beingSelector.Populate(Battle.GetRoster());
		tileset.Build(Battle.TileSet);

		foreach (var b in Battle.Beings)
			BeingSetUp(b);
	}

	protected void OnActiveBeingChanged(object s, EventArgs e) {	//until turn finished/started events work properly
		if (ActiveBeingInfo != null) {
			ActiveBeingInfo.SetActive(false);
			UndisplayAvailableSkills();
			UndisplayMovementArea();
			ActiveBeingInfo = null;
		}
		
		ActiveBeingInfo = BeingInfo[ActiveBeing];
		ActiveBeingInfo.SetActive(true);
		DisplayAvailableSkills();
		DisplayMovementArea();
	}

	private void BeingTileCommand(object s, TileClickedEventArgs e) {
		if (Battle.activeBeing != null) {
			Tile tile = e.Tile;
			if (!ActiveBeing.ActionTaken && SelectedSkill != null)
				PendingGameEvent = ActiveBeing.Perform(SelectedSkill, tile);
			else if (!ActiveBeing.ActionTaken && tile.Inhabitant != null && tile != ActiveBeing.Place)
				PendingGameEvent = ActiveBeing.Perform(ActiveBeing.Skills.First(), tile); //standard attack
			else if (SelectedSkill == null && tile.Inhabitant == null && !ActiveBeing.Moved) {
				ActiveBeing.Move(tile);
				if (ActiveBeing.Moved)	//may be false if turn ended and activebeing changed
					UndisplayMovementArea();
			}
		}
	}

	protected void ConfirmGameEvent() {
		PendingGameEvent.Apply();
		CancelGameEvent();
	}

	protected void CancelGameEvent() {
		PendingGameEvent = null;
		GEDS.gameObject.SetActive(false);
		SelectedSkill = null;
	}

	protected void DisplayGameEvent() {
		GEDS.gameObject.SetActive(true);
		GEDS.SetGameEvent(PendingGameEvent);
	}

	private void CreateSkillButton(Skill skill) {
		var butt = Instantiate(ButtonPrototype);
		butt.GetComponentInChildren<Text>().text = skill.Name;
		butt.onClick.AddListener(() => SelectedSkill = skill);
		butt.transform.SetParent(CommandsPanel.transform);
		SkillButtons[skill] = butt.gameObject;
	}

	public void DisplayAvailableSkills() {
		foreach (var skill in ActiveBeing.Skills) {
			if (!SkillButtons.ContainsKey(skill)) {
				CreateSkillButton(skill);
				EndTurnButton.SetAsLastSibling();
			}
			SkillButtons[skill].SetActive(true);
		}
	}

	public void UndisplayAvailableSkills() {
		foreach (var skill in ActiveBeing.Skills)
			SkillButtons[skill].SetActive(false);
	}

	public void DisplayMovementArea() {
		ActiveMovementArea = new List<GTileS>();
		foreach (var t in ActiveBeing.MovementArea) {
			var curr = tileset[t.X, t.Y];
			curr.MovementAreaHighlight.SetActive(true);
			ActiveMovementArea.Add(curr);
		}
	}

	public void UndisplayMovementArea() {
		foreach (var t in ActiveMovementArea)
			t.MovementAreaHighlight.SetActive(false);
	}

	public GameObject DrawBeing(MetaBeing mb) {
		return Instantiate(GBeingPrototype);
	}
	public GameObject DrawBeing(Being b) {
		return DrawBeing(b.MetaBeing);
	}

	public void AddBeing(MetaBeing mb, int x, int y, int team = 0) {
		Battle.AddMetaBeing(mb, team, x, y);
	}

	private void BeingSetUp(Being b) {
		var gb = DrawBeing(b);
		gb.transform.position = new Vector3(b.Place.X, b.Place.Height*GTileS.HeightMultiplier, b.Place.Y);
		GBeings[b] = gb;

		var binft = Instantiate(BeingInfoTextPrototype);
		var binft2 = binft.GetComponent<BeingToTextHandler>();
		BeingInfo[b] = binft;
		binft.SetActive(false);
		binft2.SetBeing(b);
		binft.transform.SetParent(BeingInfoPanel.transform);
	}

	public void EndTurn() { ActiveBeing.EndTurn(); }

	public void StartBattle() {
		if (Battle.Paused) {
			tileset.Clicked += BeingTileCommand;
			
			Battle.Paused = false;
		}
	}
}
