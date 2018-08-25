using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChargeScript : MonoBehaviour {
	public float chargeTime = 1;
	public Color glowColor;
	public float pulseFrequency = 8;
	public float pulseAmount = .1f;

	int frameDist = 31;
	float chargeTimeElapsed = 0;

	Plane[] walls;
	Vector2 dashTarget;
	public bool dashing { get; private set; }

	Material mat;
	PlayerInputScript input;

	void Start() {
		mat = GetComponentInChildren<SpriteRenderer>().material;
		input = GetComponent<PlayerInputScript>();

		walls = new Plane[] {
			new Plane(Vector2.up, Vector2.down * frameDist),
			new Plane(Vector2.right, Vector2.left * frameDist),
			new Plane(Vector2.down, Vector2.up * frameDist),
			new Plane(Vector2.left, Vector2.right * frameDist),
		};
	}

	void Update() {
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
			Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue };
			if (chargeTimeElapsed >= chargeTime) {
				Vector3 aimInput = input.GetAimInput();
				if (aimInput.magnitude > 0) {
					Ray dashDirection = new Ray(transform.position, aimInput);
					float shortestDist = 100;
					for (int i = 0; i < 4; i++) {
						float dist;
						bool forward = walls[i].Raycast(dashDirection, out dist);
						if (forward && dist < shortestDist) {
							shortestDist = dist;
							Vector2 target = dashDirection.GetPoint(dist);
							Debug.DrawLine(transform.position, target, colors[i], 5);
							Debug.Log(i + " " + target + " " + dist);
							if (Mathf.Abs(target.x) <= frameDist && Mathf.Abs(target.y) <= frameDist) {
								dashing = true;
								dashTarget = target;
							}
						}
					}

					if (dashing) {
						Debug.Log(dashTarget);
						Debug.DrawLine(transform.position, dashTarget, Color.white, 5);
					}
				}
			}

			chargeTimeElapsed = 0;
			mat.SetColor("_EmissionColor", Color.clear);
		}

		dashing = false;
	}
}
