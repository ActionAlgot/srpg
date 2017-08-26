using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SkilltreeLoader : MonoBehaviour {

	public GSkillMenu skillTree;
	public GameObject button;

	private string filesDir = @"Assets\Skilltrees";
	private float height = -10;
	
	void Start () {
		var newButton = Instantiate(button);
		newButton.GetComponent<Button>().onClick.AddListener(
			() => skillTree.Build(new srpg.SkillTree(new srpg.SkillNode(0, 0))));
		newButton.GetComponentInChildren<Text>().text = "Create NEW";
		newButton.transform.SetParent(this.transform, false);
		newButton.transform.position += new Vector3(0, height);
		height -= newButton.GetComponent<RectTransform>().rect.height;

		var formatter = new BinaryFormatter();
		foreach(var stFilePath in Directory.GetFiles(filesDir)) {
			newButton = Instantiate(button);
			newButton.GetComponent<Button>().onClick.AddListener(() => {
				Stream stream = File.Open(stFilePath, FileMode.Open);
				skillTree.Build((srpg.SkillTree)formatter.Deserialize(stream));
				stream.Close();
			});
			newButton.GetComponentInChildren<Text>().text = "Load " + stFilePath.Substring(filesDir.Length+1);
			newButton.transform.SetParent(this.transform, false);
			newButton.transform.position += new Vector3(0, height);
			height -= newButton.GetComponent<RectTransform>().rect.height;
		}
	}
}
