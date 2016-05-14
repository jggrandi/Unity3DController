using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Finish : MonoBehaviour {

	Text finalTaskTime;
	// Use this for initialization
	void Start () {
		finalTaskTime = GetComponent<Text> ();
		finalTaskTime.text = "Time: " + GameController.control.gameRuntime + " s";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
