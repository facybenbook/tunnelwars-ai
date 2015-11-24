using UnityEngine;
using System.Collections;

public class BodyControl : MonoBehaviour {

	public bool  visible = false;

	void  Start (){
		transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = false;
		transform.GetChild(1).gameObject.GetComponent<Renderer>().enabled = false;
	}
	
	void  Update (){
		
	}
	
	void  SetMaster ( bool mode  ){
		visible = true;
		if (mode == true)
		{
			transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = true;
			transform.GetChild(1).gameObject.GetComponent<Renderer>().enabled = false;
		}
		else
		{
			transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = false;
			transform.GetChild(1).gameObject.GetComponent<Renderer>().enabled = true;
		}
	}
}