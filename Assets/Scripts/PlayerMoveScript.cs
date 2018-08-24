using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour {
	public float walkSpeed = 1;
	public float jumpSpeed = 7;
	public float gravity = 1;
	public int frameThickness = 1;

	enum dirs { BOTTOM, LEFT, TOP, RIGHT };
	Vector2[] dirVectors = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };

	int floorDir = (int)dirs.BOTTOM;
	float frameDist;
	float halfWidth;
	float height;

	float hspeed = 0;
	float vspeed = 0;

	// Margin allowances on various conditions for changing direction.
	float angleThreshold = 7;
	float cornerThreshold = 0.5f;

	SpriteRenderer sprite;

	void Start() {
		frameDist = 32 - frameThickness;

		sprite = GetComponentInChildren<SpriteRenderer>();

		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		halfWidth = collider.size.x / 2;
		height = collider.size.y;
	}

	void FixedUpdate() {
		MovePlayer();
		RotatePlayer();

		// Snap the sprite to the nearest pixel.
		sprite.transform.position = new Vector3(
			Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
	}

	void MovePlayer() {
		Vector2 relPos = WorldToRelative(transform.position);
		Vector2 relInput = WorldToRelative(GetInput());

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
	}

	void RotatePlayer() {
		Vector2 relPos = WorldToRelative(transform.position);
		Vector2 input = GetInput();

		if (input.x != 0 || input.y != 0) {
			int inputDir = VectorDir(input);
			Vector2 inputDirVector = dirVectors[inputDir];

			int relInputDir = WorldToRelative(inputDir);
			int dirChange = 0;

			// Check if the input direction is sideways relative to the floor,
			// and the player is closest to the wall they are moving toward.
			if (relInputDir % 2 == 1 &&
				Vector2.Angle(inputDirVector, transform.position) <= 45 + angleThreshold) {
				dirChange = DirSign(relInputDir);
			}
			// Alternatively check if the player is against a wall and moving up it.
			else if (relInputDir == 2 && Mathf.Abs(relPos.x) > frameDist - halfWidth - cornerThreshold) {
				dirChange = (int)Mathf.Sign(relPos.x);
			}

			if (dirChange != 0) {
				// Move and rotate the player.
				transform.position += RelativeToWorld(new Vector3(halfWidth * dirChange, halfWidth));
				transform.Rotate(new Vector3(0, 0, 90 * dirChange));

				floorDir = (floorDir - dirChange + 4) % 4;

				float newHspeed = vspeed * dirChange;
				float newVspeed = hspeed * -dirChange;
				hspeed = newHspeed;
				vspeed = newVspeed;
			}
		}
	}

	Vector2 GetInput() {
		return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	float VectorAngle(Vector2 vector) {
		return Vector2.SignedAngle(vector, Vector2.up) + 180;
	}

	int AngleDir(float angle) {
		return (int)Mathf.Floor((angle + 45) / 90) % 4;
	}

	int VectorDir(Vector2 vector) {
		return AngleDir(VectorAngle(vector));
	}

	int DirSign(int dir) {
		return dir > 1 ? 1 : -1;
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
}
