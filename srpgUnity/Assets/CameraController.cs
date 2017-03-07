using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	
	public float ScrollSpeed;
	public float EdgeScrollThickness;
	public float ZoomSpeed;
	public float RotateSpeed;
	private Transform centerTransform;	//Camera center

	void Start() {
		centerTransform = transform.parent;
		transform.LookAt(centerTransform);
	}
	
	// Update is called once per frame
	void Update () {

		var zoom = Input.mouseScrollDelta.y * ZoomSpeed * Time.deltaTime;
		GetComponent<Camera>().orthographicSize += zoom;

		if (Input.GetMouseButtonDown(1))
			Cursor.lockState = CursorLockMode.Locked;

		if (!Input.GetMouseButton(1)) {
			// Do camera movement by edge scrolling
			var mPosX = Input.mousePosition.x;
			var mPosY = Input.mousePosition.y;
			float scroll = ScrollSpeed * Time.deltaTime;

			if (mPosX < EdgeScrollThickness)
				centerTransform.position += (Quaternion.Euler(0, centerTransform.rotation.eulerAngles.y, 0) * (Vector3.left * scroll));
			else if (mPosX >= Screen.width - EdgeScrollThickness)
				centerTransform.position += (Quaternion.Euler(0, centerTransform.rotation.eulerAngles.y, 0) * (Vector3.right * scroll));
			if (mPosY < EdgeScrollThickness)
				centerTransform.position += (Quaternion.Euler(0, centerTransform.rotation.eulerAngles.y, 0) * (Vector3.back * scroll));
			else if (mPosY >= Screen.height - EdgeScrollThickness)
				centerTransform.position += (Quaternion.Euler(0, centerTransform.rotation.eulerAngles.y, 0) * (Vector3.forward * scroll));

		} else {
			//Move camera by moving mouse
			float x = Input.GetAxis("Mouse X");
			float y = -Input.GetAxis("Mouse Y");

			var prevRot = centerTransform.rotation.eulerAngles;
			centerTransform.rotation = Quaternion.Euler(Mathf.Clamp(prevRot.x + y, 15, 90), prevRot.y + x, prevRot.z);
		}
		if (Input.GetMouseButtonUp(1))
			Cursor.lockState = CursorLockMode.None;
	}
}
