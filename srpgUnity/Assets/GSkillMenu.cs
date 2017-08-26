using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using srpg;

public class GSkillMenu : MonoBehaviour {
	public GameObject GSkillNode;
	public GameObject Path;

	public SkillTree NonGSkilltree;
	
	private Dictionary<SkillNode, GameObject> nodeDic = new Dictionary<SkillNode, GameObject>();

	public void Start() {
		Build();
	}

	public class NodeSelectEventArgs : EventArgs {
		public SkillNode SkillNode;
		public GameObject GSkillNode;
		public NodeSelectEventArgs(SkillNode skillNode, GameObject gSkillNode) {
			SkillNode = skillNode;
			GSkillNode = gSkillNode;
		}
	}
	public event EventHandler<NodeSelectEventArgs> NodeSelected;

	public void Build(SkillTree skillTree) {

		DESTROYALLCHILDREN();

		NonGSkilltree = skillTree;

		var nodes = skillTree.AllNodes.ToList();

		var paths = new Dictionary<SkillTreePath, int>();
		GameObject gO;
		for (int i = 0; i < nodes.Count; i++) {
			gO = BuildNode(nodes[i]);
			nodeDic[nodes[i]] = gO;
			foreach (var p in nodes[i].Paths)
				if (paths.ContainsKey(p))
					DrawPath(
						new Vector3(nodes[paths[p]].X, nodes[paths[p]].Y),
						new Vector3(nodes[i].X, nodes[i].Y));
				else paths[p] = i;
		}
	}

	public void Build() {
		Build(SkillTreeshit.Basic);
	}

	public void SetHoverInfo(GameObject gnode, SkillNode node) {
		string hoverInfo;
		if (node.Mods.Any())
			hoverInfo = node.Mods
				.Select(m => m.ToString())
				.Aggregate((s0, s1) => s0 + "\n" + s1);
		else hoverInfo = "NULL";
		var textObj = gnode.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>();
		textObj.text = hoverInfo;

		textObj.GetComponent<RectTransform>().sizeDelta =	//nested content size fitters are borked
			new Vector2(textObj.preferredWidth, textObj.preferredHeight);
		gnode.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta =
			textObj.GetComponent<RectTransform>().sizeDelta;
	}

	public GameObject BuildNode(SkillNode node) {
		var sn = Instantiate(GSkillNode);

		SetHoverInfo(sn, node);

		
		sn.transform.SetParent(this.transform);
		sn.GetComponent<RectTransform>().localPosition = new Vector3(node.X, node.Y);

		sn.GetComponent<Button>().onClick.AddListener(() => NodeSelected(this, new NodeSelectEventArgs(node, sn)));

		return sn;
	}

	public void DrawPath(Vector3 p0, Vector3 p1) {
		var path = Instantiate(Path);
		path.transform.SetParent(this.transform);
		path.transform.localPosition = p0;
		var prt = path.GetComponent<RectTransform>();
		prt.sizeDelta = new Vector2(prt.sizeDelta.x, Vector3.Distance(p0, p1));

		var diference = p1 - p0;
		float sign = (p1.y < p0.y) ? -1.0f : 1.0f;
		var angle = Vector2.Angle(Vector2.right, diference) * sign;

		path.transform.rotation = Quaternion.Euler(0, 0, angle -90);
		path.transform.SetAsFirstSibling();
	}

	public void SetSkillTreeFiller(SkillTreeFiller stf) {
		UnloadSkillTreeFiller();
		foreach (var n in stf.Taken)
			nodeDic[n].transform.GetChild(1).gameObject.GetComponent<Text>().text = "1";
		SetAvailable(stf);
	}
	private void SetAvailable(SkillTreeFiller stf) {
		foreach (var n in stf.Available) {
			var sn = nodeDic[n];
			sn.GetComponentInChildren<Text>().text = "0";
			var f = new UnityEngine.Events.UnityAction[1];	//Shenanigans for lambda self reference
			f[0] = () => {
				stf.Take(n);
				sn.transform.GetChild(1).gameObject.GetComponent<Text>().text = "1";
				sn.GetComponent<Button>().onClick.RemoveListener(f[0]);
				SetAvailable(stf);
			};
			sn.GetComponent<Button>().onClick.AddListener(f[0]);
		}
	}
	public void UnloadSkillTreeFiller() {
		foreach(var gsn in nodeDic.Values) {
			gsn.GetComponentInChildren<Text>().text = "!";
			gsn.GetComponent<Button>().onClick.RemoveAllListeners();
		}
	}

	private void DESTROYALLCHILDREN() {
		var l = new List<GameObject>();
		foreach (Transform c in transform) l.Add(c.gameObject);
		foreach (var c in l) Destroy(c);
	}
}
