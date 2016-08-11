using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DAFinish : MonoBehaviour {

	Text finalTaskTime;
	// Use this for initialization
	void Start () {
		finalTaskTime = GetComponent<Text> ();
		finalTaskTime.text = "Time: " + MainController.control.gameRuntime + " s";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
