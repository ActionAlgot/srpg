using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using srpg;

public class CommanMenuHandler : MonoBehaviour {
	public UnityEngine.UI.Button button;
	public void CreateItems(Being being) {
		float height = 0;
		UnityEngine.UI.Button butt;
		foreach (var skill in being.Skills) {
			butt = Instantiate(button);
			butt.transform.position -= Vector3.up * height;
			height += butt.GetComponent<RectTransform>().rect.height;
			butt.GetComponentInChildren<UnityEngine.UI.Text>().text = skill.Name;
			butt.onClick.AddListener(() => being.SelectedAction = skill);
			butt.transform.SetParent(transform, false);
		}
		butt = Instantiate(button);
		butt.transform.position -= Vector3.up * height;
		butt.GetComponentInChildren<UnityEngine.UI.Text>().text = "End turn";
		butt.onClick.AddListener(being.EndTurn);
		butt.transform.SetParent(transform, false);
	}
}
