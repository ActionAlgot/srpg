using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using srpg;

public class GBeingMover : MonoBehaviour {
	public static float MoveSpeed = 1;

	private List<Tile> Path;
	private int PathIndex;

	public void Move(List<Tile> path) {
		Path = path;
	}

	void Update() {
		if (Path != null)
			_move();
	}

	private void _move() {
		float move = MoveSpeed * Time.deltaTime;
		var position = transform.position;
		while (true) {
			if (PathIndex + 1 >= Path.Count) {
				Path = null;
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
						continue;
					}
				} else {
					position.x -= move;
					if (position.x <= Path[PathIndex + 1].X) {
						move = -(position.x - Path[PathIndex + 1].X);
						position.x = Path[PathIndex + 1].X;
						PathIndex++;
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
						continue;
					}
				} else {
					position.z -= move;
					if (position.z <= Path[PathIndex + 1].Y) {
						move = -(position.z - Path[PathIndex + 1].Y);
						position.z = Path[PathIndex + 1].Y;
						PathIndex++;
						continue;
			}	}	}
			transform.position = position;
			break;
		}
	}
}
