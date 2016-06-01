using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StackCube : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;

	private float offset = 0.0f;


	void Start () {
		//closeStackedValue = 1000.0f;

		for (int i = 0; i < objStatic.transform.childCount; i++) {
			for (int j = 0; j < objStatic.transform.childCount; j++) {
				float distance = Vector3.Distance (objStatic.transform.GetChild (i).transform.position, objStatic.transform.GetChild (j).transform.position);
				if (distance > offset) {
					offset = distance;
				}
			}
		}

	}

	void Update () {
//		closeStackedValue = 1000.0f;
//		Matrix4x4 movingObjMatrix = Matrix4x4.TRS(objMoving.transform.position, objMoving.transform.rotation, objMoving.transform.localScale);
//		for (int i = 0; i < objStatic.transform.childCount; i++) {
//			Transform childStatic = objStatic.transform.GetChild (i);
//			Matrix4x4 staticObjMatrix = Matrix4x4.TRS (childStatic.transform.position, childStatic.transform.rotation, childStatic.transform.localScale);
//			if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < closeStackedValue) {
//				closeStackedValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
//			}
//		}
//			
//		print (closeStackedValue);
		//		

//		Matrix4x4 movingObjMatrix = Matrix4x4.TRS(objMoving.transform.position, objMoving.transform.rotation, objMoving.transform.localScale);
//		Matrix4x4 staticObjMatrix = Matrix4x4.TRS(objStatic.transform.position, objStatic.transform.rotation, objStatic.transform.localScale);
//		MainController.control.stackingDistance = Utils.distMatrices (movingObjMatrix, staticObjMatrix);

		float maxDistance = 0.0f;




		for (int i = 0; i < objMoving.transform.childCount; i++) {
			for (int j = 0; j < objStatic.transform.childCount; j++) {
				
				float distance = Vector3.Distance (objMoving.transform.GetChild (i).transform.position, objStatic.transform.GetChild (j).transform.position);

				if (distance - offset > maxDistance) {
					maxDistance = distance - offset;
				}
			}
		}
		print (maxDistance);
			


	}

//	void OnTriggerEnter(Collider other)
//	{
//		print ("COL");
//		// If the object passes through the checkpoint, we activate it
//		if (other.tag == "box")
//		{
//			print ("AQUI");
//
//		}
//	}

	// Retorna o menor valor de stacking alcançado por todas as possíveis orientações do cubo
//	float checkCubeStackingOrientations(Matrix4x4 staticObjMatrix, Matrix4x4 movingObjMatrix){
//		float lowestValue = 1000.0f;
//
//
//
//		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
//			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
//		}
//
//		objStaticOtherPoses.transform.rotation = objStatic.transform.rotation;
//		Vector3 axis = new Vector3 (objStaticOtherPoses.transform.rotation.x, objStaticOtherPoses.transform.rotation.y, objStaticOtherPoses.transform.rotation.z);// + Vector3.up;
//		Vector3 correctedAxis = Quaternion.Inverse (objStaticOtherPoses.transform.rotation) * axis;
//		objStaticOtherPoses.transform.rotation = objStaticOtherPoses.transform.rotation * Quaternion.AngleAxis (270, correctedAxis);
//		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
//		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
//			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
//		}
//
//		print ("x:" + objStaticOtherPoses.transform.rotation.eulerAngles.x + " y:" + objStaticOtherPoses.transform.rotation.eulerAngles.y + " z:" + objStaticOtherPoses.transform.rotation.eulerAngles.z);
//		//print ("x:" + correctedAxis.x + " y:" + correctedAxis.y + " z:" + correctedAxis.z);
//
//		return lowestValue;
//	}

}
