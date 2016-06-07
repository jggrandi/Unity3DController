using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class HandleCheckpoints : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;

	void Start () {
		
	}

	float distCubes(GameObject a, Transform b){
		float maxDistanceOfLocalMin = 0.0f;
		for (int i = 0; i < a.transform.childCount; i++) {
			float localMin = float.MaxValue;
			for (int j = 0; j < b.transform.childCount; j++) {
				float distance = Vector3.Distance (a.transform.GetChild (i).transform.position, b.transform.GetChild (j).transform.position);

				if (distance < localMin) {
					localMin = distance;
				}
			}
			if (localMin > maxDistanceOfLocalMin)
				maxDistanceOfLocalMin = localMin;
		}
		//print (maxDistanceOfLocalMin);
		return maxDistanceOfLocalMin;
	}

	void Update () {
		if (GameLogic.countdownToBeginTask == "") {
			for (int i = 0; i < objStatic.transform.childCount; i++) {
				MainController.control.stackDistance [i] += distCubes (objMoving, objStatic.transform.GetChild (i));
			}
		}

	}

	void OnTriggerEnter(Collider objCollided){
		if(objCollided.tag == "EndTask")
			MainController.control.endTask = true;
	}

	void OnTriggerExit(Collider objCollided){

	}
		
}
