using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleWriter : MonoBehaviour {
	private Text TextObj;
	void Start () {
		TextObj = GetComponent<Text>();
		TextObj.text = "";
		var TextRectTrans = TextObj.GetComponent<RectTransform>();
		TextRectTrans.sizeDelta = new Vector2(TextRectTrans.rect.width, 0);
		srpg.ConsoleLoggerHandlerOrWhatever.OnLog += (s, e) => {
			TextObj.text += "\n" + e.text;
			TextRectTrans.sizeDelta = new Vector2(TextRectTrans.rect.width, TextObj.preferredHeight);
		};
	}
}
