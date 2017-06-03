using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using srpg;

public class GSkillMenu : MonoBehaviour {
	public GameObject GSkillNode;
	public GameObject Path;

	public SkillTree NonGSkilltree;
	
	private Dictionary<SkillNode, GameObject> nodeDic = new Dictionary<SkillNode, GameObject>();
	/*private List<Vector3> placements = new List<Vector3> {
		new Vector3(-20, -20),
		new Vector3(20, 60),
		new Vector3(-60, 60),
		new Vector3(-100, -20),
		new Vector3(-140, -100),
		new Vector3(-180, -20),
		new Vector3(60, 140)
	};*/

	public void Start() {
		//for (int i = 0; i < placements.Count; i++)	//I'm a fun loving and not autistic guy apparently
		//	placements[i] += new Vector3((Random.value * 20) - 10, (Random.value * 20) - 10);
		Build();
	}

	public void Build(SkillTree skillTree) {

		DESTROYALLCHILDREN();

		NonGSkilltree = skillTree;

		var nodes = skillTree.AllNodes.ToList();
		var fuckUnity = new List<GameObject>();

		var paths = new Dictionary<SkillTreePath, int>();
		GameObject gO;
		for (int i = 0; i < nodes.Count; i++) {
			gO = BuildNode(nodes[i], new Vector3(nodes[i].X, nodes[i].Y));
			nodeDic[nodes[i]] = gO;
			fuckUnity.Add(gO);
			foreach (var p in nodes[i].Paths)
				if (paths.ContainsKey(p))
					DrawPath(
						new Vector3(nodes[paths[p]].X, nodes[paths[p]].Y),
						new Vector3(nodes[i].X, nodes[i].Y));
				else paths[p] = i;
		}
		foreach (var go in fuckUnity)	//draw on top of path objects
			go.transform.SetAsLastSibling();
	}

	public void Build() {
		Build(SkillTreeshit.Basic);
	}

	private GameObject BuildNode(SkillNode node, Vector3 placement) {
		var sn = Instantiate(GSkillNode);

		string hoverInfo;
		if (node.Mods.Any())
			hoverInfo = node.Mods
				.Select(m => m.ToString())
				.Aggregate((s0, s1) => s0 + "\n" + s1);
		else hoverInfo = "NULL";

		var textObj = sn.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>();
		textObj.text = hoverInfo;

		textObj.GetComponent<RectTransform>().sizeDelta =
			new Vector2(textObj.preferredWidth, textObj.preferredHeight);
		sn.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta =
			textObj.GetComponent<RectTransform>().sizeDelta;
		sn.transform.SetParent(this.transform);
		sn.GetComponent<RectTransform>().localPosition = placement;
		return sn;
	}

	private void DrawPath(Vector3 p0, Vector3 p1) {
		var path = Instantiate(Path);
		path.transform.SetParent(this.transform);
		path.transform.localPosition = p0;
		var prt = path.GetComponent<RectTransform>();
		prt.sizeDelta = new Vector2(prt.sizeDelta.x, Vector3.Distance(p0, p1));

		var diference = p1 - p0;
		float sign = (p1.y < p0.y) ? -1.0f : 1.0f;
		var angle = Vector2.Angle(Vector2.right, diference) * sign;

		path.transform.rotation = Quaternion.Euler(0, 0, angle -90);
	}

	public void SetSkillTreeFiller(SkillTreeFiller stf) {
		UnloadSkillTreeFiller();
		foreach (var n in stf.Taken)
			nodeDic[n].transform.GetChild(1).gameObject.GetComponent<Text>().text = "1";
		setAvailable(stf);
	}
	private void setAvailable(SkillTreeFiller stf) {
		foreach (var n in stf.Available) {
			var sn = nodeDic[n];
			sn.GetComponentInChildren<Text>().text = "0";
			UnityEngine.Events.UnityAction f = () => {
				stf.Take(n);
				sn.transform.GetChild(1).gameObject.GetComponent<Text>().text = "1";
				sn.GetComponent<Button>().onClick.RemoveAllListeners();
				setAvailable(stf);
			};
			sn.GetComponent<Button>().onClick.AddListener(f);
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
