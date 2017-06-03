using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSkilltreeS : MonoBehaviour {
	public GSkillMenu skillTree;
	public UnityEngine.UI.InputField filenameInput;

	private void Start() {
		filenameInput.onEndEdit.AddListener(OnInputEnd);
	}

	public void OnInputEnd(string filename) {
		if (Input.GetKey("enter") || Input.GetKey("return")) {	//Unity is stupid and fires 'onEndEdit' when you click outside the input field
			filenameInput.gameObject.SetActive(false);
			Save(filename);
		}
	}

	private void Save(string filename) {
		FileStream stream = File.Create(@"Assets\Skilltrees\" + filename);
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(stream, skillTree.NonGSkilltree);
	}
}
