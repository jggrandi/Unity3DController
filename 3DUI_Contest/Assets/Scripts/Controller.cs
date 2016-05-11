using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;
		
	private static float stackTolerance = 0.15f;

	private int objIndex;
	// Use this for initialization
	void Start () {
		
		foreach (Transform child in objMoving.transform) {
		child.gameObject.SetActive (false);
		}

		foreach (Transform child in objStatic.transform) {
			child.gameObject.SetActive (false);
		}


		objIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {


		Transform childMoving = objMoving.transform.GetChild (objIndex);
		Transform childStatic = objStatic.transform.GetChild (objIndex);
		childMoving.gameObject.SetActive (true);
		childStatic.gameObject.SetActive (true);

		Matrix4x4 movingObjMatrix = Matrix4x4.TRS(objMoving.transform.position, objMoving.transform.rotation, objMoving.transform.localScale);
		Matrix4x4 staticObjMatrix = Matrix4x4.TRS(objStatic.transform.position, objStatic.transform.rotation, objStatic.transform.localScale);
//		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < stackTolerance) {
//			
//			objStatic.transform.position = new Vector3 (objStatic.transform.position.x + Random.Range(0,10), objStatic.transform.position.y + Random.Range(0,10), objStatic.transform.position.z + Random.Range(0,10));
//			objStatic.transform.rotation = Random.rotation;
//			int uniformScale = Random.Range (0, 3);
//			objStatic.transform.localScale = new Vector3 (objStatic.transform.localScale.x + uniformScale, objStatic.transform.localScale.y + uniformScale, objStatic.transform.localScale.z + uniformScale);
//		}
			
	}

}
