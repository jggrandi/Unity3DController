using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SetupWaitClients : MonoBehaviour {
	public GameObject objDevice;

	// Use this for initialization
	void Start () {
	
		MainController.control.t.rotateMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), objDevice.transform.rotation, new Vector3(1, 1, 1));
		MainController.control.t.scaleMatrix = Matrix4x4.identity;
		MainController.control.t.translateMatrix = Matrix4x4.identity;
		MainController.control.t.viewMatrix = Matrix4x4.identity;

		//print(SceneManager.GetActiveScene ().name + " - " + SceneManager.GetActiveScene ().buildIndex);
		//print (SceneManager.GetActiveScene ().buildIndex);
	}

	void OnGUI(){
		foreach (Client c in MainController.control.clients) {
			//if (c.deviceCameraCamera == null) continue;
			Vector3 devicePosition =  Camera.main.WorldToScreenPoint(c.deviceObject.transform.position);
			float scale = devicePosition.z / 8;
			GUIStyle playersStyle = new GUIStyle();
			playersStyle.fontSize = Mathf.CeilToInt (20 / scale);
			playersStyle.alignment = TextAnchor.MiddleCenter;
			int id = c.id + 1;
			GUI.Label (new Rect (devicePosition.x , devicePosition.y /1.5f , 10 , 10 ), "Player " + id, playersStyle);
		}

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 50;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.alignment = TextAnchor.MiddleCenter;

		GUI.Label (new Rect (Screen.width/2,40, 50, 50), "Setting up "  , titleStyle);

	}

	// Update is called once per frame
	void Update () {

		for(int i = MainController.control.clients.Count - 1; i >= 0; i--){
			if(!MainController.control.clients[i].connected){
				GameObject.Destroy(MainController.control.clients[i].deviceObject);
				MainController.control.clients.RemoveAt(i);
			}
		}
		float xPos = -1.5f;
//		int qntClients = MainController.control.clients.Count;
		foreach (Client c in MainController.control.clients) {

			if(c.deviceObject == null){
				c.deviceObject = GameObject.Instantiate (objDevice);
				c.deviceRotation = c.deviceObject.transform.rotation;
				c.deviceObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(false);
				c.deviceObject.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
				c.deviceObject.transform.GetChild(0).gameObject.transform.GetChild(2).gameObject.SetActive(false);


			}

			c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[2].color = Utils.HexColor(c.color, 0.2f); //borda
			c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[1].color = Utils.HexColor(c.color, 0.8f); //botao
			c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[3].color = Utils.HexColor(c.color, 0.8f); //tela


			Quaternion q = Quaternion.Slerp(c.deviceMatrix.GetRotation(), c.deviceRotation, 0.5f);
			if (Utils.isNaN(q)) continue;
			c.deviceRotation = q;
			c.deviceRotation = Utils.NormalizeQuaternion(c.deviceRotation);

			c.deviceObject.transform.rotation = c.deviceRotation;

			c.deviceObject.transform.position = new Vector3(xPos,1.0f,2.5f);
			xPos = 1.5f;
		}

	}
}
