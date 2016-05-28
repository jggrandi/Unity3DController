using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StackCube : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;
	private GameObject objStaticOtherPoses;

	//private static float stackTolerance = 0.15f;
	private float closeStackedValue;


	void Start () {
		closeStackedValue = 1000.0f;
		objStaticOtherPoses = new GameObject ("objStaticOtherPoses");
		objStaticOtherPoses.transform.position = objStatic.transform.position;
		objStaticOtherPoses.transform.rotation = objStatic.transform.rotation;

	}

	void Update () {

		Matrix4x4 movingObjMatrix = Matrix4x4.TRS(objMoving.transform.position, objMoving.transform.rotation, objMoving.transform.localScale);
		Matrix4x4 staticObjMatrix = Matrix4x4.TRS(objStatic.transform.position, objStatic.transform.rotation, objStatic.transform.localScale);

		closeStackedValue = checkCubeStackingOrientations (staticObjMatrix, movingObjMatrix);
			
		print (closeStackedValue);

	}



	// Retorna o menor valor de stacking alcançado por todas as possíveis orientações do cubo
	float checkCubeStackingOrientations(Matrix4x4 staticObjMatrix, Matrix4x4 movingObjMatrix){
		float lowestValue = 1000.0f;



		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (90, 0, 0);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (180, 0, 0);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (270, 0, 0);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (0, 90, 0);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (0, 180, 0);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (0, 270, 0);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (0, 0, 90);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (0, 0, 180);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		objStaticOtherPoses.transform.rotation = Quaternion.Euler (0, 0, 270);
		staticObjMatrix = Matrix4x4.TRS(objStaticOtherPoses.transform.position, objStaticOtherPoses.transform.rotation, objStaticOtherPoses.transform.localScale);
		if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < lowestValue) {
			lowestValue = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
		}

		return lowestValue;
	}

}
