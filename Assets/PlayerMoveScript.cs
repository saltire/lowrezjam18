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
		Vector2 relPos = WorldToRelative(transform.position);
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector2 relInput = WorldToRelative(input);

		// Movement

		hspeed = relInput.x * walkSpeed;

		if (relPos.y == -frameDist && Input.GetButtonDown("Jump")) {
			vspeed += jumpSpeed;
		}
		vspeed -= gravity;

		hspeed = Mathf.Clamp(hspeed,
			-frameDist + halfWidth - relPos.x,
			frameDist - halfWidth - relPos.x);
		vspeed = Mathf.Clamp(vspeed,
			-frameDist - relPos.y,
			frameDist - height - relPos.y);

		transform.position += RelativeToWorld(new Vector3(hspeed, vspeed, 0));
		relPos = WorldToRelative(transform.position);

		// Rotation

		if (relInput.x != 0 || relInput.y != 0) {
			// Find the closest cardinal direction to player input.
			float relInputAngle = Vector2.SignedAngle(relInput, Vector2.up) + 180;
			int relInputDir = (int)Mathf.Floor((relInputAngle + 45) / 90) % 4;
			int dirSign = DirSign(relInputDir);

			// Check if the input direction is sideways relative to the floor.
			if (relInputDir % 2 == 1) {
				// Check if the player is against the wall they are moving toward.
				if (relPos.x == (frameDist - halfWidth) * dirSign) {
					// Move and rotate the player so their feet are on the wall.
					transform.position += RelativeToWorld(new Vector3(halfWidth * dirSign, halfWidth));
					transform.Rotate(new Vector3(0, 0, 90 * dirSign));

					floorDir = RelativeToWorld(relInputDir);
				}
			}
		}

		// Snap the sprite to the nearest pixel.
		sprite.transform.position = new Vector3(
			Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
	}

	Vector2 WorldToRelative(Vector2 worldPos) {
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

	Vector3 RelativeToWorld(Vector2 relPos) {
		switch (floorDir) {
			default:
			case (int)dirs.BOTTOM:
				return new Vector3(relPos.x, relPos.y);

			case (int)dirs.LEFT:
				return new Vector3(relPos.y, -relPos.x);

			case (int)dirs.TOP:
				return new Vector3(-relPos.x, -relPos.y);

			case (int)dirs.RIGHT:
				return new Vector3(-relPos.y, relPos.x);
		}
	}

	int WorldToRelative(int worldDir) {
		return (worldDir - floorDir + 4) % 4;
	}

	int RelativeToWorld(int relDir) {
		return (relDir + floorDir) % 4;
	}

	int DirSign(int dir) {
		return dir > 1 ? 1 : -1;
	}
}
