using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CHIStackController : MonoBehaviour {
	
	public GameObject objMoving;
	public GameObject objStatic;

	private static float stackTolerance = 0.12f;
	public int objIndex;

	private static float timeToStack = 30.0f; //tempo para stacking
	private float stackingTime = timeToStack; //actual time to finish this stacking piece

	public float initialDistance = 0.0f;
	private float totalScore = 0.0f;
	private float actualScore = 0;

	public bool showSceneOverText = false;
	public bool showEndTaskOverText = false;
	public bool firstMeassurement = true;

	public IEnumerator timeIsUp()    
	{
		totalScore += actualScore;
		objIndex++;
		showSceneOverText = true;    
		yield return new WaitForSeconds(3.5f);  

		showSceneOverText = false;
		firstMeassurement = true;
		stackingTime = timeToStack;
	}

	public IEnumerator endTask()    
	{
		showEndTaskOverText = true; 
		showSceneOverText = false;
		yield return new WaitForSeconds(1.5f);  

		showEndTaskOverText = false;
		SceneManager.LoadScene ("CHIFinalScore");
	}

	void Awake () {
		
		foreach (Transform child in objMoving.transform) {
            //child.gameObject.transform.rotation = Random.rotation;
		    child.gameObject.SetActive (false);
		}

		foreach (Transform child in objStatic.transform) {
			child.gameObject.SetActive (false);
		}

		MainController.control.stackingObjQnt = objMoving.transform.childCount;
        // MainController.control.finalConstruction = GameObject.Instantiate (objMoving);
		objIndex = 0;
	}

	void Update () {
		if (!showSceneOverText && !showEndTaskOverText) {
			Transform childMoving = objMoving.transform.GetChild (objIndex);
			Transform childStatic = objStatic.transform.GetChild (objIndex);
			childMoving.gameObject.SetActive (true);
			childStatic.gameObject.SetActive (true);

			Matrix4x4 movingObjMatrix = Matrix4x4.TRS (childMoving.transform.position, childMoving.transform.rotation, childMoving.transform.localScale);
			Matrix4x4 staticObjMatrix = Matrix4x4.TRS (childStatic.transform.position, childStatic.transform.rotation, childStatic.transform.localScale);
			MainController.control.stackDistance [objIndex] = Utils.distMatrices (movingObjMatrix, staticObjMatrix);
			if (firstMeassurement) {
				initialDistance = MainController.control.stackDistance [objIndex];
				firstMeassurement = false;
			} else
                //actualScore = (MainController.control.stackDistance [objIndex] - 0) / ((initialDistance) - 0) * (0 - 1000) + 1000;
                actualScore = (MainController.control.stackDistance[objIndex] - initialDistance) * (0 - 100) + 100;
            // print (actualScore);
			if (objIndex == 0) { // Se é o primeiro objeto, avalia pela distância até o objetivo. Assim os players podem treinar sem q o tempo passe.
				if (Utils.distMatrices (movingObjMatrix, staticObjMatrix) < stackTolerance) {
					//childMoving.gameObject.transform.position = childStatic.gameObject.transform.position;
					//childMoving.gameObject.transform.rotation = childStatic.gameObject.transform.rotation;
					//childMoving.gameObject.transform.localScale = childStatic.gameObject.transform.localScale;
					childStatic.gameObject.SetActive (false);
                    childMoving.gameObject.SetActive(false);
					StartCoroutine (timeIsUp ());			
				}
			} else { // Stacka pelo tempo, começa o teste 
				if (!showSceneOverText) {
					stackingTime -= Time.deltaTime;
					if (stackingTime < 0.0f) {
						childStatic.gameObject.SetActive (false);
						StartCoroutine (timeIsUp ());
					}
				}
			}

			if (objIndex >= objMoving.transform.childCount) {
                //MainController.control.finalConstruction = GameObject.Instantiate (objMoving);
				MainController.control.finalScore = totalScore;
				StartCoroutine (endTask ());
			}
		}
	}



	void OnGUI(){

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 30;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.alignment = TextAnchor.MiddleCenter;

		int pieceNumber = objIndex + 1;
		int pieceTotal = objMoving.transform.childCount;

		if (showSceneOverText) {
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			GUI.Label (new Rect (Screen.width / 2, Screen.height / 2, 50, 50), "Get Ready!\n Piece: " + pieceNumber + "/" +pieceTotal, titleStyle);
		}else if(showEndTaskOverText){
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			GUI.Label (new Rect (Screen.width / 2, Screen.height / 2, 50, 50), "You have finished the task!", titleStyle);			
		}else {
			int printStakingTime = (int)stackingTime;

            // GUI.Label (new Rect (Screen.width / 2, 15, 50, 50),MainController.control.teamName, titleStyle);	
			GUI.Label (new Rect (Screen.width / 2, 35, 50, 50), MainController.control.teamName + "\t Time: " + printStakingTime.ToString () + " s" + "\tPiece: " + pieceNumber + "/" +pieceTotal + "\t Score: " + (int)totalScore , titleStyle);	
		}
	}

}
