using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class HandleCheckpoints : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;


	void Start () {
		
	}

	void Update () {

		float maxDistanceOfLocalMin = 0.0f;
		if (MainController.control.checkpointID < objStatic.transform.childCount) {
			for (int i = 0; i < objMoving.transform.childCount; i++) {
				float localMin = float.MaxValue;
				for (int j = 0; j < objStatic.transform.GetChild (MainController.control.checkpointID).transform.childCount; j++) {
					float distance = Vector3.Distance (objMoving.transform.GetChild (i).transform.position, objStatic.transform.GetChild (MainController.control.checkpointID).GetChild (j).transform.position);

					if (distance < localMin) {
						localMin = distance;
					}
				}
				if (localMin > maxDistanceOfLocalMin)
					maxDistanceOfLocalMin = localMin;
			}
		}
		MainController.control.stackDistance = maxDistanceOfLocalMin;
	}

	void OnTriggerEnter(Collider objCollided){
		if(objCollided.tag == "EndTask")
			MainController.control.endTask = true;
	}

	void OnTriggerExit(Collider objCollided){
		if (MainController.control.checkpointID < objStatic.transform.childCount) {
			if (objCollided.name == objStatic.transform.GetChild (MainController.control.checkpointID).name) {
				MainController.control.checkpointID++;
				print (MainController.control.stackDistance);
				MainController.control.stackDistance = float.MaxValue;
			}
		}
	}
		
}
