using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour {
	public float walkSpeed = 120;
	public float horizInertiaGround = 0.1f;
	public float horizInertiaAir = 0.3f;
	public float jumpSpeed = 300;
	public float gravity = 30;
	public float maxRotateDistance = 12;

	public float chargeTime = 1;
	public Color glowColor;
	public float pulseFrequency = 8;
	public float pulseAmount = .1f;
	public float dashSpeed = 300;

	enum dirs { BOTTOM, LEFT, TOP, RIGHT };
	Vector2[] dirVectors = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };
	float frameDist = 31;
	int floorDir = (int)dirs.BOTTOM;
	float halfWidth;
	float height;

	float hspeed = 0;
	float vspeed = 0;

	// Margin allowances on various conditions for changing direction.
	float angleThreshold = 7;
	float cornerThreshold = 0.5f;

	float chargeTimeElapsed = 0;
	Plane[] walls;
	Vector2 dashTarget;
	public bool dashing { get; private set; }

	Material mat;
	SpriteRenderer sprite;
	PlayerInputScript input;

	void Start() {
		mat = GetComponentInChildren<SpriteRenderer>().material;
		sprite = GetComponentInChildren<SpriteRenderer>();
		input = GetComponent<PlayerInputScript>();

		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		halfWidth = collider.size.x / 2;
		height = collider.size.y;

		walls = new Plane[] {
			new Plane(Vector2.up, Vector2.down * frameDist),
			new Plane(Vector2.right, Vector2.left * frameDist),
			new Plane(Vector2.down, Vector2.up * frameDist),
			new Plane(Vector2.left, Vector2.right * frameDist),
		};
	}

	void Update() {
		CheckDash();

		if (dashing) {
			DashPlayer();
		}
		else {
			MovePlayer();
			RotatePlayer();
		}

		// Snap the sprite to the nearest pixel.
		sprite.transform.position = new Vector3(
			Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
	}

	void MovePlayer() {
		Vector2 relPos = WorldToRelative(transform.position);
		Vector2 relMoveInput = WorldToRelative(input.GetMoveInput());

		bool onGround = relPos.y == -frameDist;

		float maxHspeed = walkSpeed * Time.deltaTime;
		hspeed = Mathf.Min(maxHspeed,
			relMoveInput.x * maxHspeed + hspeed * (onGround ? horizInertiaGround : horizInertiaAir));

		if (onGround && Input.GetButtonDown(input.jump)) {
			vspeed += jumpSpeed * Time.deltaTime;
		}
		vspeed -= gravity * Time.deltaTime;

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
		Vector2 moveInput = input.GetMoveInput();

		if (moveInput.magnitude > 0) {
			int inputDir = VectorDir(moveInput);
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
				DoRotation(dirChange);
			}
		}
	}

	void CheckDash() {
		if (Input.GetButton(input.charge)) {
			chargeTimeElapsed += Time.deltaTime;

			float chargeAmount = Mathf.Clamp01(chargeTimeElapsed / chargeTime);
			float alpha = chargeAmount * (1 - pulseAmount);

			if (chargeTimeElapsed >= chargeTime) {
				alpha += Mathf.Sin(Mathf.PI * (chargeTimeElapsed - chargeTime) * pulseFrequency) * pulseAmount;
			}

			mat.SetColor("_EmissionColor", Color.Lerp(Color.clear, glowColor, alpha));
		}
		else if (Input.GetButtonUp(input.charge)) {
			if (chargeTimeElapsed >= chargeTime) {
				Vector3 aimInput = input.GetAimInput();

				if (aimInput.magnitude > 0) {
					Ray dashDirection = new Ray(transform.position, aimInput);
					float shortestDist = 100;
					float dist;

					float maxHitCoord = frameDist + cornerThreshold;

					for (int i = 0; i < 4; i++) {
						bool forward = walls[i].Raycast(dashDirection, out dist);

						if (forward && dist < shortestDist) {
							shortestDist = dist;
							Vector2 target = dashDirection.GetPoint(dist);

							if (Mathf.Abs(target.x) <= maxHitCoord && Mathf.Abs(target.y) <= maxHitCoord) {
								float maxRelX = frameDist - halfWidth;
								dashTarget = (i % 2 == 1) ?
									new Vector2(target.x, Mathf.Clamp(target.y, -maxRelX, maxRelX)) :
									new Vector2(Mathf.Clamp(target.x, -maxRelX, maxRelX), target.y);
								dashing = true;

								DoRotation((floorDir - i + 4) % 4);
							}
						}
					}
				}
			}

			chargeTimeElapsed = 0;
			mat.SetColor("_EmissionColor", Color.clear);
		}
	}

	void DoRotation(int dirChange) {
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

			float newHspeed = vspeed * dirChange;
			float newVspeed = hspeed * -dirChange;
			hspeed = newHspeed;
			vspeed = newVspeed;
		}

		floorDir = (floorDir - dirChange + 4) % 4;
	}

	void DashPlayer() {
		transform.position = Vector2.MoveTowards(transform.position, dashTarget,
			dashSpeed * Time.deltaTime);

		if ((Vector2)transform.position == dashTarget) {
			dashing = false;
		}
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
