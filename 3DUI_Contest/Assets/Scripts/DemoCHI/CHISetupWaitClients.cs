using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CHISetupWaitClients : MonoBehaviour {
	public GameObject objDevice;

	// Use this for initialization
	void Start () {
	
		MainController.control.t.rotateMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), objDevice.transform.rotation, new Vector3(1, 1, 1));
		MainController.control.t.scaleMatrix = Matrix4x4.identity;
		MainController.control.t.translateMatrix = Matrix4x4.identity;
		MainController.control.t.viewMatrix = Matrix4x4.identity;

		GameObject teamName = GameObject.Find ("TeamName");
		InputField inpTeamName = teamName.GetComponent<InputField> ();
		inpTeamName.text = MainController.control.teamName;
		//PlayerPrefs.DeleteAll ();

	}

	void OnGUI(){
		foreach (Client c in MainController.control.clients) {
			Vector3 devicePosition =  Camera.main.WorldToScreenPoint(c.deviceObject.transform.position);
			float scale = devicePosition.z / 8;
			GUIStyle playersStyle = new GUIStyle();
			playersStyle.fontSize = Mathf.CeilToInt (10 / scale);
			playersStyle.alignment = TextAnchor.MiddleCenter;
			int id = c.id + 1;
			GUI.Label (new Rect (devicePosition.x , devicePosition.y - 50.0f , 10 , 10 ), "Player " + id, playersStyle);
		}

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 50;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.alignment = TextAnchor.MiddleCenter;
		GUI.Label (new Rect (Screen.width/2,40, 50, 50), "Collaborative Challenge  "  , titleStyle);

		int rankingQnt = PlayerPrefs.GetInt ("rankingQnt");
		string ranking = null;
		for (int i = 0; i < 10; i++) {
			int index = i+1;
			ranking += index.ToString ();
			ranking += "\t";
			ranking += PlayerPrefs.GetString ("teamName" + i); 
			ranking += "\t";
			ranking += PlayerPrefs.GetFloat ("teamScore" + i).ToString();
			ranking += "\n";
		}
		GUI.Box (new Rect (Screen.width / 1.5f, Screen.height / 1.7f, 200, 200), "Top 10:\n" + ranking );
	}

	// Update is called once per frame
	void Update () {

		for(int i = MainController.control.clients.Count - 1; i >= 0; i--){
			if(!MainController.control.clients[i].connected){
				GameObject.Destroy(MainController.control.clients[i].deviceObject);
				MainController.control.clients.RemoveAt(i);
			}
		}

//		int qntClients = MainController.control.clients.Count;
		int countClients = 0;
		foreach (Client c in MainController.control.clients) {

			if(c.deviceObject == null){
				c.deviceObject = GameObject.Instantiate (objDevice);
				c.deviceObject.SetActive (true);
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

			float n = MainController.control.clients.Count;
			float i = countClients++;

			c.deviceObject.transform.position = new Vector3((-(n-1)/2.0f+i)*2.0f,1.0f,2.5f);
			c.deviceObject.transform.position += new Vector3(-0.0f,0.0f,0.0f);
		}

	}
}
