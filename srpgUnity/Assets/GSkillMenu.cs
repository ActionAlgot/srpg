using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using srpg;

public class GSkillMenu : MonoBehaviour {
	public GameObject GSkillNode;
	public GameObject Path;

	private Dictionary<SkillTreePath, int> paths = new Dictionary<SkillTreePath, int>();
	private List<Vector3> placements = new List<Vector3> {
		new Vector3(-20, -20),
		new Vector3(20, 60),
		new Vector3(-60, 60),
		new Vector3(-100, -20),
		new Vector3(-140, -100),
		new Vector3(-180, -20),
		new Vector3(60, 140)
	};

	public void Start() {
		for (int i = 0; i < placements.Count; i++)	//I'm a fun loving and not autistic guy apparently
			placements[i] += new Vector3((Random.value * 20) - 10, (Random.value * 20) - 10);
		Build();
	}

	public void Build() {
		var st = SkillTreeshit.Basic;
		var nodes = st.AllNodes.ToList();
		var fuckUnity = new List<GameObject>();

		for (int i = 0; i < nodes.Count; i++) {
			fuckUnity.Add(BuildNode(nodes[i], placements[i]));
			foreach (var p in nodes[i].Paths)
				if (paths.ContainsKey(p))
					DrawPath(placements[paths[p]], placements[i]);
				else paths[p] = i;
		}
		foreach (var go in fuckUnity)	//draw on top of path objects
			go.transform.SetAsLastSibling();
	}

	private GameObject BuildNode(SkillNode node, Vector3 placement) {
		var sn = Instantiate(GSkillNode);

		string hoverInfo = node.Mods
			.Select(m => m.ToString() + "\n")
			.Aggregate((s0, s1) => s0 + s1);
		if (hoverInfo.Length != 0)
			hoverInfo = hoverInfo.Substring(0, hoverInfo.Length - 1);

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
}
