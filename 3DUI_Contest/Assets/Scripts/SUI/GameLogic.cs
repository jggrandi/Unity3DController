using UnityEngine;
using System;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour {

	private GameObject objControlledSharp; // This object will handle all collisions and sharp moviments
	public GameObject objControlledSmooth; // This object is rendered with smoothed transformations
    public GameObject objCamera;
	public GameObject objDevice;
	public GameObject objCheckpoints;

    public Log log;
    private int countFrames = 0;

	private Vector3 prevPosition;
	private Vector3 physicForce;

	public static String countdownToBeginTask = "";    
	public bool showCountdown = false;
	public bool showSceneOverText = false;

	void Start() {

		StartCoroutine(getReady());
		MainController.control.endTask = false;

		MainController.control.t.boxPosition = objControlledSmooth.transform.position;
		MainController.control.t.boxPositionSmooth = objControlledSmooth.transform.position;

		objControlledSharp = new GameObject ("objControlled"); 
		objControlledSharp.transform.position = objControlledSmooth.transform.position;
		objControlledSharp.transform.rotation = Quaternion.identity;
		objControlledSharp.AddComponent<Rigidbody> (); // Rigidbody to handle collisions
		objControlledSharp.AddComponent<BoxCollider> (); // Also to handle collisions
		Rigidbody rb = objControlledSharp.GetComponent<Rigidbody> ();
		rb.useGravity = false; // We need to set this parameters to get consistent collisions
		rb.mass = 10000.0f;
		rb.drag = 10000.0f;
		rb.angularDrag = 10000.0f;
		rb.constraints = RigidbodyConstraints.FreezeRotation; // This do not allow the object to adapat to the surface

		objControlledSharp.AddComponent<HandleCollision> (); // Add the HandleCollision script to the objControlledSharp
		HandleCollision handleCollision = objControlledSharp.GetComponent<HandleCollision>(); //Go to the script
		handleCollision.objCollider = objControlledSmooth; // Add the objControlledSmooth to objCollider. It makes the cube blinks when collision is detected

		objControlledSharp.AddComponent<HandleCheckpoints> ();
		HandleCheckpoints handleCheckpoints = objControlledSharp.GetComponent<HandleCheckpoints> ();
		handleCheckpoints.objMoving = objControlledSmooth;
		handleCheckpoints.objStatic = objCheckpoints;

		MainController.control.t.rotateMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), objControlledSharp.transform.rotation, new Vector3(1, 1, 1));
		MainController.control.t.scaleMatrix = Matrix4x4.identity;
		MainController.control.t.translateMatrix = Matrix4x4.identity;
		MainController.control.t.viewMatrix = Matrix4x4.identity;
		MainController.control.t.rotateCameraMatrix = Matrix4x4.identity;
		MainController.control.t.cameraPosition = objCamera.transform.position;

		prevPosition = objControlledSharp.transform.position;
	
		MainController.control.logFilename = MainController.control.teamName + "-" + SceneManager.GetActiveScene ().name;
		log = new Log(MainController.control.logFilename, MainController.control.clients.Count, objCheckpoints.transform.childCount);
		//print (Application.persistentDataPath);

		foreach (Client c in MainController.control.clients) { // generate the label texture for each client's minicamera 
			c.deviceColorTextureForGUI = Utils.MakeTexture (2, 2, Utils.HexColor (c.color, 0.6f));
		}
	}

	// call this function to display countdown
	public IEnumerator getReady()    
	{

		showCountdown = true;    

		countdownToBeginTask = "3";    
		yield return new WaitForSeconds(0.5f);  

		countdownToBeginTask = "2";    
		yield return new WaitForSeconds (0.5f);

		countdownToBeginTask = "1";    
		yield return new  WaitForSeconds (0.5f);

		countdownToBeginTask = "GO!";    
		yield return new  WaitForSeconds (0.2f);

		showCountdown = false;
		countdownToBeginTask = "";  
	}
		
	public IEnumerator sceneIsOver()    
	{
		showSceneOverText = true;    
		yield return new WaitForSeconds(1.5f);  

		showSceneOverText = false;
		if (MainController.control.activeScene == SceneManager.sceneCountInBuildSettings - 2)
			SceneManager.LoadScene ("EndTest");
		else
			SceneManager.LoadScene ("SetupTask");
	}

	void OnGUI(){
		
		// Apply a color label to each client's PIP 

		foreach (Client c in MainController.control.clients) {
			if (c.deviceCameraCamera == null) continue;
			float posRecX = (c.deviceCameraCamera.rect.width * Screen.width - 10) + c.deviceCameraCamera.rect.x * Screen.width ;
			float posRecY = (c.deviceCameraCamera.rect.height - 10) + (1 - c.deviceCameraCamera.rect.y - c.deviceCameraCamera.rect.height) * Screen.height ;
			Rect rec = new Rect(posRecX, posRecY, 20, 20);

			GUIStyle currentStyle = new GUIStyle( GUI.skin.box );
			currentStyle.normal.background = c.deviceColorTextureForGUI;

			GUI.Box (rec, "", currentStyle);
		}

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 50;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = Color.red;    
		 
		titleStyle.normal.textColor = Color.white;    

		if (showCountdown)
		{    
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			GUI.Label (new Rect (Screen.width/2,Screen.height/2, 50, 50), countdownToBeginTask  , titleStyle);

		} 
		if (showSceneOverText) {
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			GUI.Label (new Rect (Screen.width/2,Screen.height/2, 50, 50), "Fim!"  , titleStyle);
		}
	}


    void Update()
    {
		sceneIsOver ();
		//print (MainController.control.stackingDistance);

		if (countdownToBeginTask == "" && !MainController.control.endTask) {
			foreach (Client c in MainController.control.clients) {
				if (c.deviceObject == null)
					continue;
				c.deviceObject.transform.GetChild (0).gameObject.transform.GetChild (0).gameObject.SetActive (c.isRotation > 0);
				c.deviceObject.transform.GetChild (0).gameObject.transform.GetChild (1).gameObject.SetActive (c.isTranslation > 0);
				c.deviceObject.transform.GetChild (0).gameObject.transform.GetChild (2).gameObject.SetActive (c.isScale > 0);

				if (c.isRotation > 0)
					c.isRotation--;
				if (c.isTranslation > 0)
					c.isTranslation--;
				if (c.isScale > 0)
					c.isScale--;

			}
		}

	}


   
    void FixedUpdate()
	{
		if (countdownToBeginTask == "" && !MainController.control.endTask) {
			for (int i = MainController.control.clients.Count - 1; i >= 0; i--) {
				if (!MainController.control.clients [i].connected) {
					GameObject.Destroy (MainController.control.clients [i].deviceObject);
					MainController.control.clients.RemoveAt (i);
				}
			}
        
			Vector3 translation = MainController.control.t.translateMatrix.GetPosition ();// * 0.3f; // translation factor slow or faster
			MainController.control.t.translateMatrix [0, 3] *= 0.7f;
			MainController.control.t.translateMatrix [1, 3] *= 0.7f;
			MainController.control.t.translateMatrix [2, 3] *= 0.7f;

			MainController.control.objActualTranform.boxPosition = objControlledSharp.transform.position + translation;
			MainController.control.t.boxPosition = MainController.control.objActualTranform.boxPosition;

			MainController.control.objActualTranform.rotateMatrix = Matrix4x4.TRS (new Vector3 (0, 0, 0), objControlledSharp.transform.rotation, new Vector3 (1, 1, 1));
			MainController.control.objActualTranform.rotateMatrix = MainController.control.t.rotateMatrix * MainController.control.objActualTranform.rotateMatrix;

			Quaternion rot = MainController.control.objActualTranform.rotateMatrix.GetRotation ();
			if (!Utils.isNaN (rot))
				objControlledSharp.transform.rotation = rot;
			MainController.control.t.rotateMatrix = Matrix4x4.identity;
			MainController.control.objActualTranform.scaleMatrix = MainController.control.t.scaleMatrix;

			physicForce = objControlledSharp.transform.position - prevPosition;

			objControlledSharp.transform.position = MainController.control.t.boxPosition;
			objControlledSharp.transform.localScale = MainController.control.objActualTranform.scaleMatrix.GetScale ();

			prevPosition = objControlledSharp.transform.position; // Calculate collision intensity

			objControlledSmooth.transform.position = Vector3.Lerp (objControlledSmooth.transform.position, objControlledSharp.transform.position, 0.3f);
			objControlledSmooth.transform.rotation = Quaternion.Lerp (objControlledSmooth.transform.rotation, objControlledSharp.transform.rotation, 0.4f);
			objControlledSmooth.transform.localScale = Vector3.Lerp (objControlledSmooth.transform.localScale, objControlledSharp.transform.localScale, 0.7f);

			MainController.control.t.boxPositionSmooth = 0.95f * MainController.control.t.boxPositionSmooth + 0.05f * MainController.control.t.boxPosition;
			Vector3 pos = MainController.control.t.boxPositionSmooth;
			Vector3 cam = MainController.control.t.cameraPosition;
			Vector3 dir = Vector3.Normalize (pos - cam);
			dir = Vector3.Normalize (dir);
		
			if (MainController.control.t.isCameraRotation) { // Camera rotation
				dir = MainController.control.t.rotateCameraMatrix * dir;
				MainController.control.t.cameraPosition = pos + (-5 * dir); // Move the camera away a little bit
				MainController.control.t.rotateCameraMatrix = Matrix4x4.identity;
				MainController.control.t.isCameraRotation = false;
			} else {
				//dir.z = dir.y = 0;
				MainController.control.t.cameraPosition = 0.1f * (pos + (-5 * dir)) + 0.9f * cam;
			}
			objCamera.transform.position = Vector3.Slerp (objCamera.transform.position, MainController.control.t.cameraPosition, 0.1f);

			objCamera.transform.LookAt (MainController.control.t.boxPositionSmooth);
			MainController.control.t.viewMatrix = objCamera.transform.worldToLocalMatrix;
			MainController.control.t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
			float y = 0.75f;

			foreach (Client c in MainController.control.clients) {


				if (c.deviceObject == null) {
					c.deviceObject = GameObject.Instantiate (objDevice);
					c.deviceRotation = c.deviceObject.transform.rotation;
				}
					
				c.deviceObject.transform.GetChild (0).gameObject.GetComponent<Renderer> ().materials [2].color = Utils.HexColor (c.color, 0.6f); //borda
				c.deviceObject.transform.GetChild (0).gameObject.GetComponent<Renderer> ().materials [1].color = Utils.HexColor (c.color, 0.9f); //botao
				c.deviceObject.transform.GetChild (0).gameObject.GetComponent<Renderer> ().materials [3].color = Utils.HexColor (c.color, 0.9f); //tela

				//Vector3 yAxis = -Matrix4x4.TRS (new Vector3 (0, 0, 0), c.deviceRotation, new Vector3 (1, 1, 1)).GetColumn (1);
				//c.deviceCameraCamera.transform.LookAt (MainController.control.t.boxPosition, yAxis);

				c.deviceObject.transform.GetChild(1).GetComponent<Camera>().enabled = true; // enable the minicamera.
				c.deviceCameraCamera = c.deviceObject.transform.GetChild(1).gameObject.GetComponent<Camera>();
				c.deviceCameraCamera.rect = new Rect (0.75f, y, 0.2f, 0.2f); // set the minicamera position and size

//					c.deviceCameraCamera.orthographic = true;
//					c.deviceCameraCamera.orthographicSize = 2.0f;
//					c.deviceCameraCamera.nearClipPlane = 0.1f;

				if (c.prevColor != c.color) {
					c.deviceColorTextureForGUI = Utils.MakeTexture (2, 2, Utils.HexColor (c.color, 0.6f));
					c.prevColor = c.color;
				}

				y -= 0.25f;

				Quaternion q = Quaternion.Slerp (c.deviceMatrix.GetRotation (), c.deviceRotation, 0.5f);
				if (Utils.isNaN (q))
					continue;
				c.deviceRotation = q;
				c.deviceRotation = Utils.NormalizeQuaternion (c.deviceRotation);

				c.deviceObject.transform.rotation = c.deviceRotation;

				Matrix4x4 r = Matrix4x4.TRS (new Vector3 (0, 0, 0), c.deviceRotation, new Vector3 (1, 1, 1));

				Vector3 v = objControlledSmooth.transform.position;
				v = v - (Vector3)r.GetColumn (2) * MainController.control.t.scaleMatrix.GetScale ().x;

				c.deviceObject.transform.position = v;

			}

			if (countFrames % 5 == 0) {
				log.save (MainController.control.clients, objControlledSharp, Camera.main.transform.rotation , MainController.control.inCollision, physicForce, MainController.control.stackDistance);
				physicForce = Vector3.zero;
				for (int i = 0; i < objCheckpoints.transform.childCount; i++) {
					//print (MainController.control.stackDistance [i]);
					MainController.control.stackDistance [i] = 0.0f;
				}
			}

			if (Input.GetKey ("space")) {
				
				if (MainController.control.activeScene == SceneManager.sceneCountInBuildSettings - 2)
					SceneManager.LoadScene ("EndTest");
				else
					SceneManager.LoadScene ("SetupTask");
			}

			countFrames++;
		}
		if (MainController.control.endTask) {		
			StartCoroutine(sceneIsOver());
		}

	}

    public void OnApplicationQuit()
    {
        log.close();
    }

}
