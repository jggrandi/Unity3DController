using UnityEngine;
using System.Collections;

public class StartWaitForClients : MonoBehaviour {
	public GameObject objDevice;

	// Use this for initialization
	void Start () {
	
		GameController.control.t.rotateMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), objDevice.transform.rotation, new Vector3(1, 1, 1));
		GameController.control.t.scaleMatrix = Matrix4x4.identity;
		GameController.control.t.translateMatrix = Matrix4x4.identity;
		GameController.control.t.viewMatrix = Matrix4x4.identity;


	}
	
	// Update is called once per frame
	void Update () {

		for(int i = GameController.control.clients.Count - 1; i >= 0; i--){
			if(!GameController.control.clients[i].connected){
				GameObject.Destroy(GameController.control.clients[i].deviceObject);
				GameController.control.clients.RemoveAt(i);
			}
		}
		float xPos = -1.5f;
		foreach (Client c in GameController.control.clients) {

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

			Vector3 yAxis = -Matrix4x4.TRS(new Vector3(0, 0, 0), c.deviceRotation, new Vector3(1, 1, 1)).GetColumn(1);

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
