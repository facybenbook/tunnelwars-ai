using UnityEngine;
using System.Collections;

public class TimedDestroy : MonoBehaviour {

	int Timer = AIAgent.Level3StepSize - 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Timer <= 0) {
			Object.Destroy(this.gameObject);
		}
		Timer--;
	}
}
