using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using srpg;

public class GBeingMover : MonoBehaviour {
	public static float MoveSpeed = 5;

	private List<Tile> Path;
	private int PathIndex;

	public void Move(List<Tile> path) {
		Path = path;
		SetRotation();
	}

	void Update() {
		if (Path != null)
			_move();
	}

	private void SetRotation() {
		if (PathIndex + 1 >= Path.Count) return;
		Vector3 v = transform.rotation.eulerAngles;
		switch (CardinalUtilities.GetMovementCardinal(Path[PathIndex], Path[PathIndex + 1])) {
			case Cardinal.North:
				transform.rotation = Quaternion.Euler(v.x, 0, v.z);
				break;
			case Cardinal.East:
				transform.rotation = Quaternion.Euler(v.x, 90, v.z);
				break;
			case Cardinal.South:
				transform.rotation = Quaternion.Euler(v.x, 180, v.z);
				break;
			case Cardinal.West:
				transform.rotation = Quaternion.Euler(v.x, 270, v.z);
				break;
		}
	}

	private void _move() {
		float move = MoveSpeed * Time.deltaTime;
		var position = transform.position;
		while (true) {
			if (PathIndex + 1 >= Path.Count) {
				transform.position = position;
				Path = null;	//stop moving
				PathIndex = 0;
				return;
			}
			int xDiff = Path[PathIndex].X - Path[PathIndex + 1].X;
			if (xDiff != 0) {
				if (xDiff < 0) {
					position.x += move;
					if (position.x >= Path[PathIndex + 1].X) {
						move = position.x - Path[PathIndex + 1].X;
						position.x = Path[PathIndex + 1].X;
						PathIndex++;
						SetRotation();
						continue;
					}
				} else {
					position.x -= move;
					if (position.x <= Path[PathIndex + 1].X) {
						move = -(position.x - Path[PathIndex + 1].X);
						position.x = Path[PathIndex + 1].X;
						PathIndex++;
						SetRotation();
						continue;
					}
				}
			} else {
				int yDiff = Path[PathIndex].Y - Path[PathIndex + 1].Y;
				if (yDiff < 0) {
					position.z += move;
					if (position.z >= Path[PathIndex + 1].Y) {
						move = position.z - Path[PathIndex + 1].Y;
						position.z = Path[PathIndex + 1].Y;
						PathIndex++;
						SetRotation();
						continue;
					}
				} else {
					position.z -= move;
					if (position.z <= Path[PathIndex + 1].Y) {
						move = -(position.z - Path[PathIndex + 1].Y);
						position.z = Path[PathIndex + 1].Y;
						PathIndex++;
						SetRotation();
						continue;
			}	}	}
			transform.position = position;
			break;
		}
	}
}
