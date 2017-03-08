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
	public int findOccuranceAmount(string src, string wanted) {
		int r = 0;
		bool found;
		for(int i = 0; i < src.Length-wanted.Length; i++) {
			found = true;
			for (int j = 0; j < wanted.Length; j++) {
				if (src[i + j] != wanted[j]) {
					found = false;
					break;
				}
			}
			if (found) r++;
		}
		return r;
	}
}
