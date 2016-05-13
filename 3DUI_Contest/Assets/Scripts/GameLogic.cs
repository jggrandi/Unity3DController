using UnityEngine;
using System;





public class GameLogic : MonoBehaviour {

	// Objects to get informations from 3DController script
	GameObject extGameObject;
	Controller extController;



    public GameObject objControlled;
	private GameObject objTransformSharp;
    public GameObject objCamera;
	public GameObject objDevice;

	private int extControllerPrevIndex;

	void Awake() {

		extGameObject = GameObject.Find("3DController");
		extController = extGameObject.GetComponent<Controller>();

		objControlled = extController.objMoving.transform.GetChild (extController.objIndex).gameObject;
		extControllerPrevIndex = extController.objIndex;

		GameController.control.t.boxPosition = objControlled.transform.position;
      
		objTransformSharp = new GameObject ("objSharpMoviments");
		objTransformSharp.transform.position = GameController.control.t.boxPosition;

		GameController.control.t.rotateMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), objControlled.transform.rotation, new Vector3(1, 1, 1));
		GameController.control.t.scaleMatrix = Matrix4x4.identity;
		GameController.control.t.translateMatrix = Matrix4x4.identity;
		GameController.control.t.viewMatrix = Matrix4x4.identity;
		GameController.control.t.rotateCameraMatrix = Matrix4x4.identity;
		GameController.control.t.cameraPosition = objCamera.transform.position;

	}
		

	void OnGUI(){
		
		// Apply a color label to each client's PIP 
		foreach (Client c in GameController.control.clients) {
			if (c.deviceCameraCamera == null || c.deviceCameraCamera.rect == null) continue;
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
		//if (Input.GetKey (KeyCode.A)) {
			//Instantiate (objControlledSmooth, new Vector3 (objControlled.transform.position.x, objControlled.transform.position.y, objControlled.transform.position.z), Quaternion.identity);
			//objControlled = GameObject.FindGameObjectWithTag ("box");
			//objControlledSmooth = GameObject.FindGameObjectWithTag ("boxSmooth");
		//}

		foreach (Client c in GameController.control.clients)
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

		for(int i = GameController.control.clients.Count - 1; i >= 0; i--){
			if(!GameController.control.clients[i].connected){
				GameObject.Destroy(GameController.control.clients[i].deviceObject);
				GameController.control.clients.RemoveAt(i);
			}
		}

		objControlled = extController.objMoving.transform.GetChild (extController.objIndex).gameObject;
		if (extControllerPrevIndex != extController.objIndex) {
			objTransformSharp.transform.position = objControlled.transform.position;
			extControllerPrevIndex = extController.objIndex;
		}
		


		Vector3 translation = GameController.control.t.translateMatrix.GetPosition () * 0.3f; // translation factor slow or faster
		GameController.control.t.translateMatrix[0,3] *= 0.7f;
		GameController.control.t.translateMatrix[1,3] *= 0.7f;
		GameController.control.t.translateMatrix[2,3] *= 0.7f;
       
		GameController.control.t.boxPosition = objTransformSharp.transform.position + translation;
        
		Matrix4x4 rotate = Matrix4x4.TRS(new Vector3(0, 0, 0), objTransformSharp.transform.rotation, new Vector3(1, 1, 1));
		rotate = GameController.control.t.rotateMatrix * rotate;

		Quaternion rot = rotate.GetRotation();
		if (!Utils.isNaN (rot)) objTransformSharp.transform.rotation = rot;
		GameController.control.t.rotateMatrix = Matrix4x4.identity;

		objTransformSharp.transform.position = GameController.control.t.boxPosition;
		objControlled.transform.localScale = GameController.control.t.scaleMatrix.GetScale ();

		objControlled.transform.position = Vector3.Lerp(objControlled.transform.position, objTransformSharp.transform.position, 0.3f);
		objControlled.transform.rotation = Quaternion.Lerp(objControlled.transform.rotation, objTransformSharp.transform.rotation, 0.4f);
		objControlled.transform.localScale = Vector3.Lerp(objControlled.transform.localScale, objTransformSharp.transform.localScale, 0.7f);

		GameController.control.t.boxPositionSmooth = 0.95f * GameController.control.t.boxPositionSmooth + 0.05f * GameController.control.t.boxPosition;
		Vector3 pos = GameController.control.t.boxPositionSmooth;
		Vector3 cam = GameController.control.t.cameraPosition;
		Vector3 dir = Vector3.Normalize (pos - cam);
		dir = Vector3.Normalize (dir);
		
		if (GameController.control.t.isCameraRotation) {
			dir = GameController.control.t.rotateCameraMatrix * dir;
			GameController.control.t.cameraPosition = pos + (-10 * dir);
			GameController.control.t.rotateCameraMatrix = Matrix4x4.identity;
			GameController.control.t.isCameraRotation = false;
		} 
		else {
			//dir.z = dir.y = 0;
			GameController.control.t.cameraPosition = 0.1f * (pos + (-5 * dir)) + 0.9f * cam;
		}
		objCamera.transform.position = Vector3.Slerp(objCamera.transform.position, GameController.control.t.cameraPosition, 0.1f);


		objCamera.transform.LookAt(GameController.control.t.boxPositionSmooth);
		GameController.control.t.viewMatrix = objCamera.transform.worldToLocalMatrix;
		GameController.control.t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
		float y = 0.75f;
		foreach (Client c in GameController.control.clients) {
			
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
			c.deviceCameraCamera.transform.LookAt(GameController.control.t.boxPosition, yAxis);
            c.deviceCameraCamera.orthographic = true;
            c.deviceCameraCamera.orthographicSize = 2.0f;
            c.deviceCameraCamera.nearClipPlane = 0.1f;


            y -= 0.25f;

            Quaternion q = Quaternion.Slerp(c.deviceMatrix.GetRotation(), c.deviceRotation, 0.5f);
            if (Utils.isNaN(q)) continue;
            c.deviceRotation = q;
            //c.deviceMatrix = Utils.fixMatrix(c.deviceMatrix);
            //c.deviceRotation = c.deviceMatrix.GetRotation();
            c.deviceRotation = Utils.NormalizeQuaternion(c.deviceRotation);

			c.deviceObject.transform.rotation = c.deviceRotation;

			Matrix4x4 r = Matrix4x4.TRS (new Vector3(0,0,0), c.deviceRotation, new Vector3 (1,1,1));

			Vector3 v = objControlled.transform.position;
			v = v - (Vector3) r.GetColumn(2)*GameController.control.t.scaleMatrix.GetScale().x*2.0f;

			c.deviceObject.transform.position = v;
            c.deviceCameraCamera.transform.position = v - (Vector3)r.GetColumn(2);

		}
	}

	

}
