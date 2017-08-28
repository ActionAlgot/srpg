using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using System;

public class GBeingMover : MonoBehaviour {
	public static float MoveSpeed = 5;

	private List<Tile> Path;
	private int PathIndex;

	private Cardinal moveDir;

	private bool jumping;
	private Func<float, float> jumpFunc;

	void Update() {
		if (Path != null)
			_move();
	}

	public void Move(List<Tile> path) {
		Path = path;
		SetMoveDir();
		SetRotation();
		if (Path[PathIndex].Height < Path[PathIndex + 1].Height)
			BeginJump();
	}

	private void SetMoveDir() {
		int xDiff = Path[PathIndex].X - Path[PathIndex + 1].X;
		if (xDiff != 0)
			if (xDiff < 0) moveDir = Cardinal.East;
			else moveDir = Cardinal.West;
		else {
			int yDiff = Path[PathIndex].Y - Path[PathIndex + 1].Y;
			if (yDiff < 0) moveDir = Cardinal.North;
			else moveDir = Cardinal.South;
		}
	}


	private void SetRotation() {
		Vector3 v = transform.rotation.eulerAngles;
		switch (moveDir) {
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

	private void BeginJump() {
		float s = Path[PathIndex].Height;
		float e = Path[PathIndex + 1].Height;
		float d = e-s;

		jumping = true;
		//TODO make a sensible jumping curve
		jumpFunc = n => (s + (float)Math.Pow(n, (2.0f * d)) + (d + 1.0f) *n) * GTileS.HeightMultiplier;
	}

	private void SetVert(ref Vector3 position) {
		float distMoved = 0;
		switch (moveDir) {
			case Cardinal.North:
				distMoved = position.z - Path[PathIndex].Y;
				break;
			case Cardinal.East:
				distMoved = position.x - Path[PathIndex].X;
				break;
			case Cardinal.South:
				distMoved = Path[PathIndex].Y - position.z;
				break;
			case Cardinal.West:
				distMoved = Path[PathIndex].X - position.x;
				break;
		}
		position.y = jumpFunc(distMoved);
	}


	private void _move() {
		float move = MoveSpeed * Time.deltaTime;
		var position = transform.position;
		while (true) {
			bool incPathIndex = false;
			switch (moveDir) {
				case Cardinal.North:
					position.z += move;
					if(position.z >= Path[PathIndex + 1].Y) {
						incPathIndex = true;
						move = position.z - Path[PathIndex + 1].Y;
					}
					break;
				case Cardinal.East:
					position.x += move;
					if (position.x >= Path[PathIndex + 1].X) {
						incPathIndex = true;
						move = position.x - Path[PathIndex + 1].X;
					}
					break;
				case Cardinal.South:
					position.z -= move;
					if (position.z <= Path[PathIndex + 1].Y) {
						incPathIndex = true;
						move = -(position.z - Path[PathIndex + 1].Y);
					}
					break;
				case Cardinal.West:
					position.x -= move;
					if (position.x <= Path[PathIndex + 1].X) {
						incPathIndex = true;
						move = -(position.x - Path[PathIndex + 1].X);
					}
					break;
			}

			if (incPathIndex) {
				PathIndex++;
				position.x = Path[PathIndex].X;
				position.z = Path[PathIndex].Y;
				jumping = false;
				position.y = Path[PathIndex].Height * GTileS.HeightMultiplier;

				if (PathIndex + 1 >= Path.Count) {
					transform.position = position;
					Path = null;    //stop moving
					PathIndex = 0;
					return;
				}

				SetMoveDir();
				SetRotation();

				if (Path[PathIndex].Height < Path[PathIndex + 1].Height)
					BeginJump();
				continue;
			}

			if(jumping) SetVert(ref position);
			transform.position = position;
			break;
		}
	}
}
