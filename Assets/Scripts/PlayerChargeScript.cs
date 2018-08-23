using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChargeScript : MonoBehaviour {
	public float chargeTime = 1;
	public Color glowColor;

	public float pulseFrequency = 8;
	public float pulseAmount = .1f;

	float chargeTimeElapsed = 0;

	Material mat;

	void Start() {
		mat = GetComponentInChildren<SpriteRenderer>().material;
	}

	void Update() {
		if (Input.GetButton("Charge")) {
			chargeTimeElapsed += Time.deltaTime;

			float chargeAmount = Mathf.Clamp01(chargeTimeElapsed / chargeTime);
			float alpha = chargeAmount * (1 - pulseAmount);

			if (chargeTimeElapsed >= chargeTime) {
				alpha += Mathf.Sin(Mathf.PI * (chargeTimeElapsed - chargeTime) * pulseFrequency) * pulseAmount;
			}

			mat.SetColor("_EmissionColor", Color.Lerp(Color.clear, glowColor, alpha));
		}
		else if (Input.GetButtonUp("Charge")) {
			chargeTimeElapsed = 0;
			mat.SetColor("_EmissionColor", Color.clear);
		}
	}
}
