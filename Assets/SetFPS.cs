/*
 * SetFPS.cs
 * 
 * Used for setting the FPS at the beginning of the game.
 * 
 */

using UnityEngine;
using System.Collections;

public class SetFPS : MonoBehaviour {

	public int Fps = 60;

	void Awake() {
	
		// Setup framerate and vsynccount
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = Fps;
	}
}
