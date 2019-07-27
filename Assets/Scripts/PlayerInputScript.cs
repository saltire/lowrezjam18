using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputScript : MonoBehaviour {
	public bool player2;

	public string moveHorizontal { get; private set; }
	public string moveVertical { get; private set; }
	public string aimHorizontal { get; private set; }
	public string aimVertical { get; private set; }
	public string jump { get; private set; }
	public string charge { get; private set; }

	void Start() {
		string suffix = player2 ? " p2" : "";
		moveHorizontal = "Horizontal Move" + suffix;
		moveVertical = "Vertical Move" + suffix;
		aimHorizontal = "Horizontal Aim" + suffix;
		aimVertical = "Vertical Aim" + suffix;
		jump = "Jump" + suffix;
		charge = "Charge" + suffix;
	}

	public Vector2 GetMoveInput() {
		return new Vector2(Input.GetAxis(moveHorizontal), Input.GetAxis(moveVertical));
	}

	public Vector2 GetAimInput() {
		return new Vector2(Input.GetAxis(aimHorizontal), Input.GetAxis(aimVertical));
	}
}
