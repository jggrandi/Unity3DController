using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;




public class TCP_server : MonoBehaviour {
	
	private volatile Transforms t = new Transforms();
	
	private volatile List<Client> clients = new List<Client>();
	private bool STOP = false;
	
	public TcpListener tcpListener;
	private Thread tcpServerRunThread;

    public GameObject objControlled;
    public GameObject objControlledSmooth;
    public GameObject objCamera;
	public GameObject objDevice;

	void Awake() {

		t.cameraPosition = objCamera.transform.position;
        t.boxPosition = objControlled.transform.position;
        t.boxPositionSmooth = t.boxPosition;
		t.rotateMatrix = Matrix4x4.identity;
		t.scaleMatrix = Matrix4x4.identity;
		t.translateMatrix = Matrix4x4.identity;
		t.viewMatrix = Matrix4x4.identity;
		t.rotateCameraMatrix = Matrix4x4.identity;

		tcpListener = new TcpListener(IPAddress.Any, 8002);
		tcpListener.Start();
		
		tcpServerRunThread = new Thread (new ThreadStart (TcpServerRun));
		tcpServerRunThread.Start ();
		
	}
		
	public void TcpServerRun(){
		while(!STOP) {
			try{
				TcpClient c = tcpListener.AcceptTcpClient();
				Client client = new Client();
				clients.Add (client);
				new Thread(new ThreadStart(()=> DeviceListener(c, client))).Start();
			}
			catch(Exception ex){
				print("Error Listener thread");
				print (ex.Message);
			}
		}
	}

	void DeviceListener (TcpClient clientDevice, Client client){

		Matrix4x4 clientDeviceMatrix;
        Matrix4x4 clientRotMatrix;
        Matrix4x4 clientTransMatrix;
        Matrix4x4 clientScaleMatrix;
		NetworkStream stream = clientDevice.GetStream();
       
        while (clientDevice.Connected && !STOP)
        {
            int pos = 0;
            byte[] bytes = new byte[261];
            bool abort = false;
            while (pos != 261 && !abort)
            {
                if (!clientDevice.Connected || STOP) abort = true;
                int l = stream.Read(bytes, pos, bytes.Length - pos);
                if (l == 0) abort = true;
                pos += l;
            }

            if (abort) break;
            
            clientRotMatrix = Utils.ConvertToMatrix(bytes, 0);
            clientTransMatrix = Utils.ConvertToMatrix(bytes, 64);
            clientScaleMatrix = Utils.ConvertToMatrix(bytes, 128);
            clientDeviceMatrix = Utils.ConvertToMatrix(bytes, 192);

			//clientRotMatrix = Matrix4x4.TRS (new Vector3 (0.0f,0.0f,0.0f), Quaternion.AngleAxis (Utils.rand()*0.0f+1.5f, Utils.RandomUnitVector ()), new Vector3 (1.0f, 1.0f, 1.0f));
			//clientTransMatrix = Matrix4x4.TRS (Utils.RandomUnitVector ()*0.1f, Quaternion.identity, new Vector3 (1.0f, 1.0f, 1.0f));
			//float scale = Utils.rand () * 0.1f+1.0f;
			//clientScaleMatrix = Matrix4x4.TRS (new Vector3 (0.0f,0.0f,0.0f), Quaternion.identity, new Vector3 (scale, scale, scale));

            client.color = BitConverter.ToInt32(bytes, 257);
            client.deviceMatrix = t.viewMatrix.inverse * clientDeviceMatrix;

			t.mutex.WaitOne ();

            //Camera Rotation
            if (!t.isCameraRotation) t.isCameraRotation = Convert.ToBoolean(bytes[256]);
            Matrix4x4 rot = clientRotMatrix;
            Matrix4x4 tr = t.viewMatrix.inverse * rot * t.viewMatrix;
            if (!t.isCameraRotation) t.rotateMatrix = tr * t.rotateMatrix;
            else t.rotateCameraMatrix = tr * t.rotateCameraMatrix;
            
            //Translation
            Vector4 translation = clientTransMatrix.GetColumn(3);
			translation.w = 0.0f;
            translation = t.viewMatrix.inverse * translation;
			if (translation.magnitude > 0.3f) translation = translation.normalized * 0.3f;
			translation.w = 1.0f;
            Matrix4x4 tt = Matrix4x4.identity;
            tt.SetColumn(3, translation);
            t.translateMatrix = tt * t.translateMatrix;

            //Scalling
            Matrix4x4 ts = clientScaleMatrix;
            t.scaleMatrix = ts * t.scaleMatrix;
			float sss = t.scaleMatrix.GetScale ().x;
			if (sss > 2.0f) {
				t.scaleMatrix [0,0] = 2.0f;
				t.scaleMatrix [1,1] = 2.0f;
				t.scaleMatrix [2,2] = 2.0f;
			} else if (sss < 0.3f) {
				t.scaleMatrix [0,0] = 0.3f;
				t.scaleMatrix [1,1] = 0.3f;
				t.scaleMatrix [2,2] = 0.3f;
			}
			t.mutex.ReleaseMutex();

            if (Vector3.Magnitude(translation) > 0.001f)
            {
                client.isRotation = 0;
                client.isTranslation = 0;
                client.isScale = 0;
                client.isTranslation = 90;
            }
            if (rot[0, 0] < 0.9999999 || rot[1, 1] < 0.9999999 || rot[2, 2] < 0.9999999)
            {
                client.isRotation = 0;
                client.isTranslation = 0;
                client.isScale = 0;
                client.isRotation = 90;
            }
            if ((ts[0, 0] > 1.001 || ts[0, 0] < 0.999) || (ts[1, 1] > 1.001 || ts[1, 1] < 0.999) || (ts[2, 2] > 1.001 || ts[2, 2] < 0.999))
            {
                client.isRotation = 0;
                client.isTranslation = 0;
                client.isScale = 0;
                client.isScale = 90;
            }
            //Byte[] b = System.Text.Encoding.UTF8.GetBytes("Teste");
            //clientDevice.Client.Send(b);
            //stream.Write(b, 0, b.Length);
            //stream.Flush();
        }
		stream.Close ();
		clientDevice.Close ();
		client.connected = false;
	}
	

	

	void OnGUI(){
		// Apply a color label to each client's PIP 
		foreach (Client c in clients) {
			if (c.deviceCameraCamera == null ) continue;
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
		if (Input.GetKey (KeyCode.A)) {
			Instantiate (objControlledSmooth, new Vector3 (objControlled.transform.position.x, objControlled.transform.position.y, objControlled.transform.position.z), Quaternion.identity);
			//objControlled = GameObject.FindGameObjectWithTag ("box");
			//objControlledSmooth = GameObject.FindGameObjectWithTag ("boxSmooth");
		}
			

        foreach (Client c in clients)
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

		for(int i = clients.Count - 1; i >= 0; i--){
			if(!clients[i].connected){
				GameObject.Destroy(clients[i].deviceObject);
				clients.RemoveAt(i);
			}
		}

		//print (t.isCameraRotation);

        Vector3 translation = t.translateMatrix.GetPosition () * 0.3f;
        t.translateMatrix[0,3] *= 0.7f;
        t.translateMatrix[1,3] *= 0.7f;
        t.translateMatrix[2,3] *= 0.7f;
        t.boxPosition = objControlled.transform.position + translation;
        //t.translateMatrix = Matrix4x4.identity;

		Matrix4x4 rotate = Matrix4x4.TRS(new Vector3(0, 0, 0), objControlled.transform.rotation, new Vector3(1, 1, 1));
        rotate = t.rotateMatrix * rotate;

		Quaternion rot = rotate.GetRotation();
		if (!Utils.isNaN (rot)) objControlled.transform.rotation = rot;
        t.rotateMatrix = Matrix4x4.identity;

		objControlled.transform.position = t.boxPosition;
		objControlled.transform.localScale = t.scaleMatrix.GetScale ();
		//print (t.translateMatrix.GetPosition ());

		objControlledSmooth.transform.position = Vector3.Lerp(objControlledSmooth.transform.position, objControlled.transform.position, 0.3f);
		objControlledSmooth.transform.rotation = Quaternion.Lerp(objControlledSmooth.transform.rotation, objControlled.transform.rotation, 0.4f);
		objControlledSmooth.transform.localScale = Vector3.Lerp(objControlledSmooth.transform.localScale, objControlled.transform.localScale, 0.7f);

        t.boxPositionSmooth = 0.95f * t.boxPositionSmooth + 0.05f * t.boxPosition;
        Vector3 pos = t.boxPositionSmooth;
		Vector3 cam = t.cameraPosition;
		Vector3 dir = Vector3.Normalize (pos - cam);
		dir = Vector3.Normalize (dir);
		
		if (t.isCameraRotation) {
			dir = t.rotateCameraMatrix * dir;
			t.cameraPosition = pos + (-5 * dir);
			t.rotateCameraMatrix = Matrix4x4.identity;
			t.isCameraRotation = false;
		} 
		else {
			//dir.z = dir.y = 0;
			t.cameraPosition = 0.1f * (pos + (-5 * dir)) + 0.9f * cam;
		}
		objCamera.transform.position = Vector3.Slerp(objCamera.transform.position, t.cameraPosition, 0.1f);


		objCamera.transform.LookAt(t.boxPositionSmooth);
		t.viewMatrix = objCamera.transform.worldToLocalMatrix;
		t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
		float y = 0.75f;
		foreach (Client c in clients) {
			
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
            c.deviceCameraCamera.transform.LookAt(t.boxPosition, yAxis);
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

			Vector3 v = objControlledSmooth.transform.position;
			v = v - (Vector3) r.GetColumn(2)*t.scaleMatrix.GetScale().x;

			c.deviceObject.transform.position = v;
            c.deviceCameraCamera.transform.position = v - (Vector3)r.GetColumn(2);

		}
	}

	
	void OnApplicationQuit() { // stop listening thread
		STOP = true;

		tcpServerRunThread.Abort ();
		tcpListener.Stop();

	}
}

