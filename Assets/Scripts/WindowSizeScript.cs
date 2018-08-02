using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowSizeScript : MonoBehaviour {
	public int windowSize = 512;

	void Start() {
		Screen.SetResolution(windowSize, windowSize, false);
	}

	void Update() {

	}
}
