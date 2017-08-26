using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using srpg;

public class SkilltreeEditorController : MonoBehaviour {

	public GSkillMenu skilltree;
	public GameObject NodeEditor;
	public Button PathAdder;
	public InputField XPosInput;
	public InputField YPosInput;
	public GameObject ModList;
	public Button ModSelectItem;
	public GameObject RemoveModButton;

	public GameObject ModEditorPanel;
	public GameObject DoubleEditor;
	public GameObject StatTypeEditor;
	public GameObject ModTypeSelection;
	public GameObject ValueModEditor;
	public GameObject InterstatularModEditor;

	private SkillNode selectedNode;
	private SkillTreePath selectedPath;
	private Mod selectedMod;
	private bool creatingPath = false;
	private GameObject selectedGNode;
	private List<GameObject> specificModSelectItems = new List<GameObject>();
	private GameObject activeModSelectItem;
	private StatType[] StatTypeOptions = (StatType[])Enum.GetValues(typeof(StatType));

	private void Start() {
		skilltree.NodeSelected += OnNodeSelected;
		XPosInput.onValueChanged.AddListener(NodeXSet);
		YPosInput.onValueChanged.AddListener(NodeYSet);

		var NewModButton = Instantiate(ModSelectItem);
		NewModButton.GetComponentInChildren<Text>().text = "New MOD";
		NewModButton.onClick.AddListener(CreateNewMod);
		NewModButton.transform.SetParent(ModList.transform);

		foreach (var dd in ModEditorPanel.GetComponentsInChildren<Dropdown>()) {
			dd.options = StatTypeOptions.Select(st => new Dropdown.OptionData(st.ToString())).ToList();
			dd.value = Array.IndexOf(StatTypeOptions, StatType.None);
		}
	}
	
	public void RemoveMod() {
		selectedNode.RemoveMod(selectedMod);
		Destroy(activeModSelectItem);
		ModEditorPanel.SetActive(false);
		skilltree.SetHoverInfo(selectedGNode, selectedNode);
	}

	private void OnNodeSelected(object s, GSkillMenu.NodeSelectEventArgs e) {
		if (creatingPath == false) {
			SetNodeSelected(e.SkillNode, e.GSkillNode);
		}
		else {
			if (selectedNode == null) {
				selectedNode = e.SkillNode;
				selectedGNode = e.GSkillNode;
			}
			else {
				((HashSet<SkillTreePath>)skilltree.NonGSkilltree.AllPaths)
					.Add(new SkillTreePath(selectedNode, e.SkillNode));
				skilltree.DrawPath(
					new Vector3(selectedNode.X, selectedNode.Y),
					new Vector3(e.SkillNode.X, e.SkillNode.Y));
				OnAddPathClicked();	//exits path adding mode
			}
		}
	}

	private void OnModEdited() {
		skilltree.SetHoverInfo(selectedGNode, selectedNode);
		activeModSelectItem.GetComponentInChildren<Text>().text = selectedMod.ToString();
	}

	private void CreateModSelectItem(Mod mod) {
		var mb = Instantiate(ModSelectItem);
		mb.GetComponentInChildren<Text>().text = mod.ToString();
		mb.onClick.AddListener(() => {
			activeModSelectItem = mb.gameObject;
			OnModSelected(mod);
		});
		mb.transform.SetParent(ModList.transform);

		specificModSelectItems.Add(mb.gameObject);
	}

	public void CreateAdditionMod() { OnModCreated(new AdditionMod(StatType.None, 0)); }
	public void CreateAdditiveMultipliernMod() { OnModCreated(new AdditiveMultiplierMod(StatType.None, 0)); }
	public void CreateMultiplierMod() { OnModCreated(new MultiplierMod(StatType.None, 0)); }
	public void CreateConversionMod() { OnModCreated(new ConversionMod(StatType.None, 0, StatType.None)); }
	public void CreateGainMod() { OnModCreated(new GainMod(StatType.None, 0, StatType.None)); }
	public void CreateConversionToAdditiveMultiplierMod() { OnModCreated(new ConversionToAdditiveMultiplierMod(StatType.None, 0, StatType.None)); }

	private void CreateNewMod() {
		ModTypeSelection.SetActive(true);
		ModEditorPanel.SetActive(false);
	}

	private void OnModCreated(Mod mod) {
		ModTypeSelection.SetActive(false);
		ModEditorPanel.SetActive(true);
		selectedNode.AddMod(mod);
		CreateModSelectItem(mod);
		OnModSelected(mod);
		skilltree.SetHoverInfo(selectedGNode, selectedNode);
	}

	private void DestroyModEditorChildren() {
		var l = new List<GameObject>();
		foreach (Transform c in ModEditorPanel.transform) l.Add(c.gameObject);

		foreach (var c in l) Destroy(c);
	}

	public void OnModTargetStatTypeEdit(int n) {
		selectedMod.TargetStatType = StatTypeOptions[n];
		OnModEdited();
	}
	public void OnModValueEdit(string s) {
		((ValueMod)selectedMod).Value = double.Parse(s);
		OnModEdited();
	}
	public void OnModEffectivenessEdit(string s) {
		((InterStatularMod)selectedMod).Effectiveness = double.Parse(s);
		OnModEdited();
	}
	public void OnModSourceStatTypeEdit(int n) {
		((InterStatularMod)selectedMod).SourceType = StatTypeOptions[n];
		OnModEdited();
	}

	private void ValueModEdit() {
		var mod = (ValueMod)selectedMod;

		var valueEd = Instantiate(DoubleEditor);
		var inp = valueEd.GetComponentInChildren<InputField>();
		inp.text = mod.Value.ToString();
		inp.onValueChanged.AddListener(s => {
			mod.Value = double.Parse(s);
			OnModEdited();
		});
		valueEd.GetComponentInChildren<Text>().text = "Value";
		valueEd.transform.SetParent(ModEditorPanel.transform);
	}

	private void InterStatularModEdit() {
		var mod = (InterStatularMod)selectedMod;

		var EffectivenessEditor = Instantiate(DoubleEditor);
		var inp = EffectivenessEditor.GetComponentInChildren<InputField>();
		inp.text = mod.Effectiveness.ToString();
		inp.onValueChanged.AddListener(s => {
			mod.Effectiveness = double.Parse(s);
			OnModEdited();
		});
		EffectivenessEditor.GetComponentInChildren<Text>().text = "Value";
		EffectivenessEditor.transform.SetParent(ModEditorPanel.transform);

		var go = Instantiate(StatTypeEditor);
		go.GetComponentInChildren<Text>().text = "SourceStat";
		go.transform.SetParent(ModEditorPanel.transform);
		var dd = go.GetComponentInChildren<Dropdown>();
		dd.options = StatTypeOptions.Select(st => new Dropdown.OptionData(st.ToString())).ToList();
		dd.value = Array.IndexOf(StatTypeOptions, mod.SourceType);
		dd.onValueChanged.AddListener(n => {
			mod.SourceType = StatTypeOptions[n];
			OnModEdited();
		});
	}

	private void OnModSelected(Mod mod) {
		ModTypeSelection.SetActive(false);
		ModEditorPanel.SetActive(true);
		//DestroyModEditorChildren();
		selectedMod = mod;

		//var go = Instantiate(StatTypeEditor);
		//go.GetComponentInChildren<Text>().text = "TargetStat";
		//go.transform.SetParent(ModEditorPanel.transform);
		//dd.onValueChanged.AddListener(n => {
		//	selectedMod.TargetStatType = StatTypeOptions[n];
		//	OnModEdited();
		//});

		if (mod is ValueMod) {
			InterstatularModEditor.SetActive(false);
			ValueModEditor.SetActive(true);
			SetValueModEditorValues();
		}//ValueModEdit();
		else if (mod is InterStatularMod) {
			ValueModEditor.SetActive(false);
			InterstatularModEditor.SetActive(true);
		}//InterStatularModEdit();

		//RemoveModButton.transform.SetAsLastSibling();
	}

	private void SetValueModEditorValues() {

		ValueModEditor.transform.GetChild(0).GetComponentInChildren<Dropdown>().value = Array.IndexOf(StatTypeOptions, selectedMod.TargetStatType);
		ValueModEditor.transform.GetChild(1).GetComponentInChildren<InputField>().text = ((ValueMod)selectedMod).Value.ToString();
	}

	private void SetNodeSelected(SkillNode sn, GameObject gsn) {

		NodeEditor.SetActive(true);

		selectedGNode = gsn;
		selectedNode = sn;

		//DestroyModEditorChildren();

		NodeEditor.SetActive(true);
		XPosInput.text = selectedNode.X.ToString();
		YPosInput.text = selectedNode.Y.ToString();

		foreach (var go in specificModSelectItems)
			Destroy(go);
		specificModSelectItems.Clear();
		foreach (var m in sn.Mods)
			CreateModSelectItem(m);
	}

	public void OnPathSelected() {
		throw new NotImplementedException();
	}

	public void OnAddNodeClicked() {
		var newN = new SkillNode(0, 0);
		skilltree.NonGSkilltree.AddNode(newN);
		SetNodeSelected(newN, skilltree.BuildNode(newN));
	}

	public void OnAddPathClicked() {
		if (!creatingPath) {
			creatingPath = true;
			selectedNode = null;
			PathAdder.GetComponentInChildren<Text>().text = "CANCEL PATH";
		} else {
			creatingPath = false;
			selectedNode = null;
			PathAdder.GetComponentInChildren<Text>().text = "ADD PATH";
		}
	}
	
	public void NodeXSet(string x) {
		int X;
		int.TryParse(x, out X);
		selectedNode.X = X;
		var goPos = selectedGNode.transform.localPosition;
		goPos.x = X;
		selectedGNode.transform.localPosition = goPos;
	}
	public void NodeYSet(string y) {
		int Y;
		int.TryParse(y, out Y);
		selectedNode.Y = Y;
		var goPos = selectedGNode.transform.localPosition;
		goPos.y = Y;
		selectedGNode.transform.localPosition = goPos;
	}
}
