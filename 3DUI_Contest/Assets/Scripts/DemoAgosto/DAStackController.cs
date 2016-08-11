using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DAStackController : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;

	private static float stackTolerance = 0.15f;
	public int objIndex;

	private float prevTime;

	void Awake () {
		
		foreach (Transform child in objMoving.transform) {
		child.gameObject.SetActive (false);
		}

		foreach (Transform child in objStatic.transform) {
			child.gameObject.SetActive (false);
		}
		MainController.control.stackingObjQnt = (objMoving.transform.childCount - 4)*3;	
		objIndex = 0;
	}

	void Update () {

		Transform childMoving = objMoving.transform.GetChild (objIndex);
		Transform childStatic = objStatic.transform.GetChild (objIndex);
		childMoving.gameObject.SetActive (true);
		childStatic.gameObject.SetActive (true);

		//MainController.control.t.scaleMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1.0f,1.0f,1.0f));
		Matrix4x4 movingObjMatrix = Matrix4x4.TRS(childMoving.transform.position, childMoving.transform.rotation, childMoving.transform.localScale);
		Matrix4x4 staticObjMatrix = Matrix4x4.TRS(childStatic.transform.position, childStatic.transform.rotation, childStatic.transform.localScale);

		Matrix4x4 movingObjMatrixTrans = Matrix4x4.TRS(childMoving.transform.position, Quaternion.identity, new Vector3(1.0f,1.0f,1.0f));
		Matrix4x4 movingObjMatrixRot = Matrix4x4.TRS(new Vector3(0, 0, 0), childMoving.transform.rotation, new Vector3(1.0f,1.0f,1.0f));
		Matrix4x4 movingObjMatrixScale = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, childMoving.transform.localScale);

		Matrix4x4 staticObjMatrixTrans = Matrix4x4.TRS(childStatic.transform.position, Quaternion.identity, new Vector3(1.0f,1.0f,1.0f));
		Matrix4x4 staticObjMatrixRot = Matrix4x4.TRS(new Vector3(0, 0, 0), childStatic.transform.rotation, new Vector3(1.0f,1.0f,1.0f));
		Matrix4x4 staticObjMatrixScale = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, childStatic.transform.localScale);

		MainController.control.stackDistance [objIndex] = Utils.distMatrices (movingObjMatrixTrans, staticObjMatrixTrans);
		MainController.control.stackDistance [objIndex+1] = Utils.distMatrices (movingObjMatrixRot, staticObjMatrixRot);
		MainController.control.stackDistance [objIndex+2] = Utils.distMatrices (movingObjMatrixScale, staticObjMatrixScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < stackTolerance) {
			print ("Stacked");
			//foreach(Client c in MainController.control.clients)
			//	c.sw.WriteLine ("S," + objIndex);
			
			childMoving.gameObject.transform.position = childStatic.gameObject.transform.position;
			childMoving.gameObject.transform.rotation = childStatic.gameObject.transform.rotation;
			childMoving.gameObject.transform.localScale = childStatic.gameObject.transform.localScale;
			childStatic.gameObject.SetActive (false);
			objIndex++;
//			
//			objStatic.transform.position = new Vector3 (objStatic.transform.position.x + Random.Range(0,10), objStatic.transform.position.y + Random.Range(0,10), objStatic.transform.position.z + Random.Range(0,10));
//			objStatic.transform.rotation = Random.rotation;
//			int uniformScale = Random.Range (0, 3);
//			objStatic.transform.localScale = new Vector3 (objStatic.transform.localScale.x + uniformScale, objStatic.transform.localScale.y + uniformScale, objStatic.transform.localScale.z + uniformScale);
		}
		if (objIndex >= MainController.control.stackingObjQnt/3) {
			
			SceneManager.LoadScene("Finish");
		}

		if (objIndex > 0 && objIndex < objMoving.transform.childCount) { // start count time
			MainController.control.gameRuntime = Time.time-prevTime;
		}
		else
			prevTime = Time.time;

	}

}
