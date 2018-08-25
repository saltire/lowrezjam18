using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour {
	public float walkSpeed = 2;
	public float horizInertiaGround = 0.1f;
	public float horizInertiaAir = 0.3f;
	public float jumpSpeed = 5;
	public float gravity = 0.5f;
	public float maxRotateDistance = 12;
	public int frameThickness = 1;
	public bool player2 = false;

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

		bool onGround = relPos.y == -frameDist;

		hspeed = Mathf.Min(walkSpeed,
			relInput.x * walkSpeed + hspeed * (onGround ? horizInertiaGround : horizInertiaAir));

		if (onGround && Input.GetButtonDown("Jump" + (player2 ? " p2" : ""))) {
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
			float distanceFromDirFloor = Mathf.Abs(inputDirVector.x != 0 ?
				inputDirVector.x * frameDist - transform.position.x :
				inputDirVector.y * frameDist - transform.position.y);

			int relInputDir = WorldToRelative(inputDir);
			int dirChange = 0;

			// Check if the input direction is away from the floor,
			// and the player is closest to the side they are moving toward.
			if (relInputDir > 0 && distanceFromDirFloor < maxRotateDistance &&
				Vector2.Angle(inputDirVector, transform.position) <= 45 + angleThreshold) {
				dirChange = relInputDir == 2 ? 2 : DirSign(relInputDir);
			}
			// Alternatively check if the player is against a wall and moving up it.
			else if (relInputDir == 2 && Mathf.Abs(relPos.x) > frameDist - halfWidth - cornerThreshold) {
				dirChange = (int)Mathf.Sign(relPos.x);
			}

			if (dirChange != 0) {
				if (dirChange == 2) {
					// Move and rotate the player 180 degrees.
					transform.position += RelativeToWorld(new Vector2(0, height));
					transform.Rotate(new Vector3(0, 0, 180));

					hspeed = -hspeed;
					vspeed = -vspeed;
				}
				else {
					// Move and rotate the player 90 degrees.
					transform.position += RelativeToWorld(new Vector2(halfWidth * dirChange, halfWidth));
					transform.Rotate(new Vector3(0, 0, 90 * dirChange));

					// Swap horizontal speed and vertical speed.
					float newHspeed = vspeed * dirChange;
					float newVspeed = hspeed * -dirChange;
					hspeed = newHspeed;
					vspeed = newVspeed;
				}

				floorDir = (floorDir - dirChange + 4) % 4;
			}
		}
	}

	Vector2 GetInput() {
		return new Vector2(
			Input.GetAxis("Horizontal" + (player2 ? " p2" : "")),
			Input.GetAxis("Vertical" + (player2 ? " p2" : "")));
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
