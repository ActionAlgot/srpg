using System.Collections;
using System.Collections.Generic;
using srpg;
using UnityEngine;
using UnityEngine.UI;

public class GameEventDisplayS : MonoBehaviour {

	public GameObject TargetDisplayPanel;
	public GameObject TargetDisplayPrototype;
	private int TL = 0;
	private int TI = 0;
	private GameObject[] TargetDisplays = new GameObject[0];
	public Button TargetDisplayPrev;
	public Button TargetDisplayNext;
	public Transform EffectDisplayPanel;
	public GameObject[][] EffectDisplays = new GameObject[0][];
	public GameObject DamageDisplayPrototype;
	public GameObject StatusEffectDisplayPrototype;
	public Button Cancel;
	public Button Confirm;

	private void Reset() {
		TI = 0;

		foreach (var go in TargetDisplays)
			Destroy(go);

		foreach (var asdf in EffectDisplays)
			foreach(var go in asdf)
				Destroy(go);	
	}

	private void TargetChange(bool set) {
		TargetDisplays[TI].SetActive(set);
		foreach(var go in EffectDisplays[TI])
			go.SetActive(set);
	}

	private void NextTarget() {
		TargetChange(false);
		TI += 1;
		if (TI >= TL) TI = 0;
		TargetChange(true);
	}
	private void PrevTarget() {
		TargetChange(false);
		TI -= 1;
		if (TI < 0) TI = TL-1;
		TargetChange(true);
	}

	private void Start() {
		TargetDisplayPrev.onClick.AddListener(PrevTarget);
		TargetDisplayNext.onClick.AddListener(NextTarget);
	}

	public void SetGameEvent(GameEvent ge) {
		Reset();

		TL = ge.BeingTargets.Count;
		TargetDisplays = new GameObject[TL];
		EffectDisplays = new GameObject[TL][];
		for (int i = 0; i < TL; i++) {
			var t = ge.BeingTargets[i];

			TargetDisplays[i] = CreateTargetDisplay(t);
			TargetDisplays[i].transform.SetParent(TargetDisplayPanel.transform);

			var appls = ge.applications[t];
			EffectDisplays[i] = new GameObject[appls.damages.Count + appls.statusEffects.Count];
			int j = 0;
			foreach (var dmg in appls.damages) {
				var obj = CreateDamageDisplay(dmg);
				obj.transform.SetParent(EffectDisplayPanel);
				obj.SetActive(false);
				EffectDisplays[i][j] = obj;
				j++;
			}
			foreach (var se in appls.statusEffects) {
				var obj = CreateStatusEffectDisplay(se);
				obj.transform.SetParent(EffectDisplayPanel);
				obj.SetActive(false);
				EffectDisplays[i][j] = obj;
				j++;
			}
		}
		TargetChange(true);
	}
	private GameObject CreateTargetDisplay(Being b) {
		var go = Instantiate(TargetDisplayPrototype);
		go.transform.GetChild(0).GetComponent<Text>().text = b.Name;
		go.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0} / {1}", b.HP, b.MaxHP); 

		return go;
	}
	private GameObject CreateDamageDisplay(Damage dmg) {
		return Instantiate(DamageDisplayPrototype);
	}
	private GameObject CreateStatusEffectDisplay(StatusEffect se) {
		return Instantiate(StatusEffectDisplayPrototype);
	}
}
