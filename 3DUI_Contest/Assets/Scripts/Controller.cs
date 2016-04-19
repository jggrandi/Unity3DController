using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;
		
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		//if (stackObject (objControlled, objToPlace, 0.5f))
		//	print ("STACKED");
		Matrix4x4 movingObjMatrix = Matrix4x4.TRS(objMoving.transform.position, objMoving.transform.rotation, objMoving.transform.localScale);
		Matrix4x4 staticObjMatrix = Matrix4x4.TRS(objStatic.transform.position, objStatic.transform.rotation, objStatic.transform.localScale);
		if(Utils.distMatrices(movingObjMatrix, staticObjMatrix) < 0.2f) print ("STACKED");

			
	}

}
