using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum dirs { BOTTOM, LEFT, TOP, RIGHT };

public class PlayerMoveScript : MonoBehaviour {
	public float walkSpeed = 1;
	public float jumpSpeed = 7;
	public float gravity = 1;
	public int frameThickness = 1;

	int floorDir = (int)dirs.BOTTOM;
	float frameDist;
	float halfWidth;
	float height;

	float hspeed = 0;
	float vspeed = 0;

	SpriteRenderer sprite;

	void Start() {
		frameDist = 32 - frameThickness;

		sprite = GetComponentInChildren<SpriteRenderer>();

		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		halfWidth = collider.size.x / 2;
		height = collider.size.y;
	}

	void FixedUpdate() {
		float hAxis = Input.GetAxis("Horizontal");
		float vAxis = Input.GetAxis("Vertical");

		hspeed = hAxis * walkSpeed;

		if (transform.position.y == -frameDist && Input.GetAxis("Jump") > 0) {
			vspeed += jumpSpeed;
		}
		vspeed -= gravity;

		// Prevent player from moving past the walls or floor.
		hspeed = Mathf.Clamp(hspeed,
			-frameDist + halfWidth - transform.position.x,
			frameDist - halfWidth - transform.position.x);
		vspeed = Mathf.Clamp(vspeed,
			-frameDist - transform.position.y,
			frameDist - height - transform.position.y);

		transform.position += new Vector3(hspeed, vspeed, 0);

		// Should do the same thing as above, but for some reason it doesn't work right.
		// transform.position = new Vector3(
		// 	Mathf.Clamp(transform.position.x + hspeed, -frameDist + halfWidth, frameDist - halfWidth),
		// 	Mathf.Clamp(transform.position.y + vspeed, -frameDist, frameDist - height),
		// 	transform.position.z);

		sprite.transform.position = new Vector3(
			Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);

		if (hAxis != 0 || vAxis != 0) {
			// Find the closest cardinal direction to player input.
			float inputAngle = Vector2.SignedAngle(new Vector2(hAxis, vAxis), Vector2.up) + 180;
			int inputDir = (int)Mathf.Floor((inputAngle + 45) / 90) % 4;
			int relDir = RelativeDir(inputDir);

			// Check if the input direction is sideways relative to the floor.
			if (relDir % 2 == 1) {
				Vector2 relPos = RelativePosition(transform.position);

				// Check if the player is against the wall they are moving toward.
				if (relPos.x == (frameDist - halfWidth) * DirSign(relDir)) {
					// Rotate the player.
				}
			}
		}
	}

	Vector2 RelativePosition(Vector2 worldPos) {
		switch (floorDir) {
			default:
			case (int)dirs.BOTTOM:
				return new Vector2(worldPos.x, worldPos.y);

			case (int)dirs.LEFT:
				return new Vector2(-worldPos.y, worldPos.x);

			case (int)dirs.TOP:
				return new Vector2(-worldPos.x, -worldPos.y);

			case (int)dirs.RIGHT:
				return new Vector2(worldPos.y, -worldPos.x);
		}
	}

	int RelativeDir(int worldDir) {
		return (worldDir - floorDir + 4) % 4;
	}

	int DirSign(int dir) {
		return dir > 2 ? -1 : 1;
	}
}
