using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using srpg;
using System;

public class GBeingMover : MonoBehaviour {
	public static float MoveSpeed = 4;

	private List<Tile> Path;
	private int PathIndex;
	private Tile CurrentTile;
	private Tile NextTile;

	private Cardinal moveDir;

	private bool jumping;
	private Func<float, float> jumpFunc;

	void Update() {
		if (Path != null)
			_move();
	}

	public void Move(List<Tile> path) {
		Path = path;
		CurrentTile = Path[PathIndex];
		NextTile = Path[PathIndex + 1];
		SetMoveDir();
		SetRotation();
		MaybeBeginJump();
	}

	private void SetMoveDir() {
		int xDiff = CurrentTile.X - NextTile.X;
		if (xDiff != 0)
			if (xDiff < 0) moveDir = Cardinal.East;
			else moveDir = Cardinal.West;
		else {
			int yDiff = CurrentTile.Y - NextTile.Y;
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

	private bool IsLongJump(Tile s, Tile d) {
		switch (moveDir) {
			case Cardinal.North:
				return d.Y - s.Y == 2;
			case Cardinal.East:
				return d.X - s.X == 2;
			case Cardinal.South:
				return d.Y - s.Y == -2;
			case Cardinal.West:
				return d.X - s.X == -2;
		}
		throw new ArgumentException("Unhandled cardinal");
	}

	private void MaybeBeginJump() {
		if (CurrentTile.Height != NextTile.Height || IsLongJump(CurrentTile, NextTile))
			_BeginJump();
	}
	private void _BeginJump() {
		float s = CurrentTile.Height;
		float e = NextTile.Height;
		float d = e-s;

		jumping = true;
		//TODO make a sensible jumping curve
		if (d > 0) jumpFunc = x => (s + (float)Math.Pow(x, (2.0f * d)) + (d + 1.0f) * x) * GTileS.HeightMultiplier;
		else if (IsLongJump(CurrentTile, NextTile))
			jumpFunc = x => (s + (-d + 1) * -(float)Math.Pow((x / 2), 2.0f) + (x / 2)) * GTileS.HeightMultiplier;
		else jumpFunc = x => (s + (-d + 1) * -(float)Math.Pow(x, 2.0f) + x) * GTileS.HeightMultiplier;
	}

	private void SetVert(ref Vector3 position) {
		float distMoved = 0;
		switch (moveDir) {
			case Cardinal.North:
				distMoved = position.z - CurrentTile.Y;
				break;
			case Cardinal.East:
				distMoved = position.x - CurrentTile.X;
				break;
			case Cardinal.South:
				distMoved = CurrentTile.Y - position.z;
				break;
			case Cardinal.West:
				distMoved = CurrentTile.X - position.x;
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
					if(position.z >= NextTile.Y) {
						incPathIndex = true;
						move = position.z - NextTile.Y;
					}
					break;
				case Cardinal.East:
					position.x += move;
					if (position.x >= NextTile.X) {
						incPathIndex = true;
						move = position.x - NextTile.X;
					}
					break;
				case Cardinal.South:
					position.z -= move;
					if (position.z <= NextTile.Y) {
						incPathIndex = true;
						move = -(position.z - NextTile.Y);
					}
					break;
				case Cardinal.West:
					position.x -= move;
					if (position.x <= NextTile.X) {
						incPathIndex = true;
						move = -(position.x - NextTile.X);
					}
					break;
			}

			if (incPathIndex) {
				PathIndex++;
				CurrentTile = NextTile;
				position.x = CurrentTile.X;
				position.z = CurrentTile.Y;
				jumping = false;
				position.y = CurrentTile.Height * GTileS.HeightMultiplier;

				if (PathIndex + 1 >= Path.Count) {
					transform.position = position;
					Path = null;    //stop moving
					PathIndex = 0;
					return;
				}

				NextTile = Path[PathIndex + 1];
				
				SetMoveDir();
				SetRotation();

				MaybeBeginJump();
				continue;
			}

			if(jumping) SetVert(ref position);
			transform.position = position;
			break;
		}
	}
}
