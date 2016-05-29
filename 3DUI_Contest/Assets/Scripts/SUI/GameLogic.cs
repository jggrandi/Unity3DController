using UnityEngine;
using System;


public class GameLogic : MonoBehaviour {

	private GameObject objControlledSharp; // This object will handle all collisions and sharp moviments
	public GameObject objControlledSmooth; // This object is rendered with smoothed transformations
    public GameObject objCamera;
	public GameObject objDevice;


	void Start() {

		MainController.control.t.boxPosition = objControlledSmooth.transform.position;
		MainController.control.t.boxPositionSmooth = objControlledSmooth.transform.position;

		objControlledSharp = new GameObject ("objControlled"); 
		objControlledSharp.transform.position = objControlledSmooth.transform.position;
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

		MainController.control.t.rotateMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), objControlledSharp.transform.rotation, new Vector3(1, 1, 1));
		MainController.control.t.scaleMatrix = Matrix4x4.identity;
		MainController.control.t.translateMatrix = Matrix4x4.identity;
		MainController.control.t.viewMatrix = Matrix4x4.identity;
		MainController.control.t.rotateCameraMatrix = Matrix4x4.identity;
		MainController.control.t.cameraPosition = objCamera.transform.position;

		//MainController.control.recordGamePlay.AddAction
		if (RecordGamePlay.SP != null) {
			RecordGamePlay.SP.AddAction (RecordActions.playerAction, MainController.control.t.boxPosition);
			string asd = RecordGamePlay.SP.RecordedDataToString ();
			print (asd);
		}

	}
		
	void OnGUI(){
		
		// Apply a color label to each client's PIP 
		foreach (Client c in MainController.control.clients) {
			if (c.deviceCameraCamera == null) continue;
			float posRecX = (c.deviceCameraCamera.rect.width * Screen.width - 10) + c.deviceCameraCamera.rect.x * Screen.width ;
			float posRecY = (c.deviceCameraCamera.rect.height - 10) + (1 - c.deviceCameraCamera.rect.y - c.deviceCameraCamera.rect.height) * Screen.height ;
			Rect rec = new Rect(posRecX, posRecY, 20, 20);

			GUIStyle currentStyle = new GUIStyle( GUI.skin.box );
            currentStyle.normal.background = Utils.MakeTexture(2, 2, c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[2].color);

			GUI.Box (rec, "", currentStyle);
		}
	}


    void Update()
    {

		foreach (Client c in MainController.control.clients)
        {
			if (c.deviceObject == null) continue;
            c.deviceObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(c.isRotation > 0);
            c.deviceObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(c.isTranslation > 0);
            c.deviceObject.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject.SetActive(c.isScale > 0);

            if (c.isRotation > 0) c.isRotation--;
            if (c.isTranslation > 0) c.isTranslation--;
            if (c.isScale > 0) c.isScale--;

        }

    }
   
    void FixedUpdate()
    {

		for(int i = MainController.control.clients.Count - 1; i >= 0; i--){
			if(!MainController.control.clients[i].connected){
				GameObject.Destroy(MainController.control.clients[i].deviceObject);
				MainController.control.clients.RemoveAt(i);
			}
		}

		Vector3 translation = MainController.control.t.translateMatrix.GetPosition () * 0.3f; // translation factor slow or faster
		MainController.control.t.translateMatrix[0,3] *= 0.7f;
		MainController.control.t.translateMatrix[1,3] *= 0.7f;
		MainController.control.t.translateMatrix[2,3] *= 0.7f;
       
		MainController.control.t.boxPosition = objControlledSharp.transform.position + translation;

		Matrix4x4 rotate = Matrix4x4.TRS(new Vector3(0, 0, 0), objControlledSharp.transform.rotation, new Vector3(1, 1, 1));
		rotate = MainController.control.t.rotateMatrix * rotate;

		Quaternion rot = rotate.GetRotation();
		if (!Utils.isNaN (rot)) objControlledSharp.transform.rotation = rot;
		MainController.control.t.rotateMatrix = Matrix4x4.identity;

		objControlledSharp.transform.position = MainController.control.t.boxPosition;
		objControlledSharp.transform.localScale = MainController.control.t.scaleMatrix.GetScale ();

		objControlledSmooth.transform.position = Vector3.Lerp(objControlledSmooth.transform.position, objControlledSharp.transform.position, 0.3f);
		objControlledSmooth.transform.rotation = Quaternion.Lerp(objControlledSmooth.transform.rotation, objControlledSharp.transform.rotation, 0.4f);
		objControlledSmooth.transform.localScale = Vector3.Lerp(objControlledSmooth.transform.localScale, objControlledSharp.transform.localScale, 0.7f);

		MainController.control.t.boxPositionSmooth = 0.95f * MainController.control.t.boxPositionSmooth + 0.05f * MainController.control.t.boxPosition;
		Vector3 pos = MainController.control.t.boxPositionSmooth;
		Vector3 cam = MainController.control.t.cameraPosition;
		Vector3 dir = Vector3.Normalize (pos - cam);
		dir = Vector3.Normalize (dir);
		
		if (MainController.control.t.isCameraRotation) { // Camera rotation
			dir = MainController.control.t.rotateCameraMatrix * dir;
			MainController.control.t.cameraPosition = pos + (-10* dir); // Move the camera away a little bit
			MainController.control.t.rotateCameraMatrix = Matrix4x4.identity;
			MainController.control.t.isCameraRotation = false;
		} 
		else {
			//dir.z = dir.y = 0;
			MainController.control.t.cameraPosition = 0.1f * (pos + (-5 * dir)) + 0.9f * cam;
		}
		objCamera.transform.position = Vector3.Slerp(objCamera.transform.position, MainController.control.t.cameraPosition, 0.1f);

		objCamera.transform.LookAt(MainController.control.t.boxPositionSmooth);
		MainController.control.t.viewMatrix = objCamera.transform.worldToLocalMatrix;
		MainController.control.t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
		float y = 0.75f;

		foreach (Client c in MainController.control.clients) {
			
			if(c.deviceObject == null){
				c.deviceObject = GameObject.Instantiate (objDevice);
				c.deviceCamera = GameObject.Instantiate (objCamera);

				c.deviceCamera.transform.parent = c.deviceObject.transform;
				c.deviceRotation = c.deviceObject.transform.rotation;

			}

            c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[2].color = Utils.HexColor(c.color, 0.2f); //borda
            c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[1].color = Utils.HexColor(c.color, 0.8f); //botao
            c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[3].color = Utils.HexColor(c.color, 0.8f); //tela

            Vector3 yAxis = -Matrix4x4.TRS(new Vector3(0, 0, 0), c.deviceRotation, new Vector3(1, 1, 1)).GetColumn(1);

			c.deviceCameraCamera = c.deviceCamera.GetComponent<Camera>();
			c.deviceCameraCamera.rect = new Rect(0.75f,y,0.2f,0.2f);
			c.deviceCameraCamera.transform.LookAt(MainController.control.t.boxPosition, yAxis);
            c.deviceCameraCamera.orthographic = true;
            c.deviceCameraCamera.orthographicSize = 2.0f;
            c.deviceCameraCamera.nearClipPlane = 0.1f;


            y -= 0.25f;

            Quaternion q = Quaternion.Slerp(c.deviceMatrix.GetRotation(), c.deviceRotation, 0.5f);
            if (Utils.isNaN(q)) continue;
            c.deviceRotation = q;
            c.deviceRotation = Utils.NormalizeQuaternion(c.deviceRotation);

			c.deviceObject.transform.rotation = c.deviceRotation;

			Matrix4x4 r = Matrix4x4.TRS (new Vector3(0,0,0), c.deviceRotation, new Vector3 (1,1,1));

			Vector3 v = objControlledSmooth.transform.position;
			v = v - (Vector3) r.GetColumn(2) * MainController.control.t.scaleMatrix.GetScale().x;

			c.deviceObject.transform.position = v - (Vector3)r.GetColumn(2);
			c.deviceCameraCamera.transform.position = v - (Vector3)r.GetColumn(2) ;

		}
	}

	

}
