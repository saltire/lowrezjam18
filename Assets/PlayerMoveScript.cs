using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour {
	public float walkSpeed = 1;
	public float jumpSpeed = 7;
	public float gravity = 1;
	public float margin = 3;

	float hspeed = 0;
	float vspeed = 0;

	SpriteRenderer sprite;

	void Start() {
		sprite = GetComponentInChildren<SpriteRenderer>();
	}

	void FixedUpdate() {
		hspeed = Input.GetAxis("Horizontal") * walkSpeed;

		if (transform.position.y == 0 && Input.GetAxis("Jump") > 0) {
			vspeed += jumpSpeed;
		}
		vspeed -= gravity;

		hspeed = Mathf.Min(hspeed, 32 - margin - transform.position.x);
		hspeed = Mathf.Max(hspeed, -32 + margin - transform.position.x);
		vspeed = Mathf.Max(vspeed, -transform.position.y);

		transform.position += new Vector3(hspeed, vspeed, 0);

		sprite.transform.position = new Vector3(
			Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
	}
}
