using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HandleCollision : MonoBehaviour
{
	Color collisionColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
    Color noCollisionColor = new Color(0.0f, 1.0f, 0.0f, 0.6f);
    private float f = 0.0f;
	public GameObject objCollider;

	float collisionInit = 0.0f;
	int inCollision = 0;
	// Use this for initialization
	void Start ()
	{
	}
	
	void OnCollisionEnter(Collision other) {
        f = 1.0f;
//		collisionInit = MainController.control.gameRuntime;
	}

	void OnCollisionStay(Collision other) {
		f = 1.0f;
	}

	void OnCollisionExit(Collision other){
//		if (RecordGamePlay.SP != null)  // It only register activities if the RecordGamePlay is being used
//			RecordGamePlay.SP.AddAction (RecordActions.collisionEvent, TransformationAction.collision, collisionInit, MainController.control.gameRuntime);
	}

	void OnTriggerEnter(Collider other){
		print ("AQUI");
	}

	void OnTriggerExit(Collider other){
		print ("Saiu");
	}

	void Update(){
		objCollider.GetComponent<Renderer>().material.color = collisionColor * f + noCollisionColor * (1.0f - f);
        f *= 0.96f;




	}

}

