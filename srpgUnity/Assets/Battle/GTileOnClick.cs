using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GTileOnClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
	public event EventHandler OnClick;
	public event EventHandler OnMouseHoverEnter;
	public event EventHandler OnMouseHoverExit;

	public void OnPointerClick(PointerEventData eventData) {
		if (OnClick != null) OnClick(this, EventArgs.Empty);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		if (OnMouseHoverEnter != null) OnMouseHoverEnter(this, EventArgs.Empty);
	}

	public void OnPointerExit(PointerEventData eventData) {
		if (OnMouseHoverExit != null) OnMouseHoverExit(this, EventArgs.Empty);
	}
}
