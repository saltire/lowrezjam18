using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum sides { BOTTOM, LEFT, TOP, RIGHT };

public class PlayerMoveScript : MonoBehaviour {
	public float walkSpeed = 1;
	public float jumpSpeed = 7;
	public float gravity = 1;
	public int frameThickness = 1;

	int side = (int)sides.BOTTOM;
	int groundY;
	float halfWidth;
	float height;

	float hspeed = 0;
	float vspeed = 0;

	SpriteRenderer sprite;

	void Start() {
		groundY = -32 + frameThickness;

		sprite = GetComponentInChildren<SpriteRenderer>();

		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		halfWidth = collider.size.x / 2;
		height = collider.size.y;
	}

	void FixedUpdate() {
		hspeed = Input.GetAxis("Horizontal") * walkSpeed;

		if (transform.position.y == groundY && Input.GetAxis("Jump") > 0) {
			vspeed += jumpSpeed;
		}
		vspeed -= gravity;

		// Prevent player from moving past the walls or floor.
		hspeed = Mathf.Max(hspeed, groundY + halfWidth - transform.position.x); // left
		hspeed = Mathf.Min(hspeed, -groundY - halfWidth - transform.position.x); // right
		vspeed = Mathf.Max(vspeed, groundY - transform.position.y); // bottom
		vspeed = Mathf.Min(vspeed, -groundY - height - transform.position.y); // top

		transform.position += new Vector3(hspeed, vspeed, 0);

		sprite.transform.position = new Vector3(
			Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
	}
}
