using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;


public class Transforms{
	public List<Matrix4x4> deviceMatrix = new List<Matrix4x4>();
	public Matrix4x4   rotateMatrix    = new Matrix4x4();
	public Matrix4x4   translateMatrix = new Matrix4x4();
	public Matrix4x4   scaleMatrix     = new Matrix4x4();
	public Matrix4x4   viewMatrix      = new Matrix4x4();
	public Matrix4x4   rotateCameraMatrix = new Matrix4x4();
	public Vector3   cameraPosition = new Vector3();
    public Vector3 boxPosition = new Vector3();
    public Vector3 boxPositionSmooth = new Vector3();
	public bool isCameraRotation = false;
}

public class Client{
	public GameObject deviceObject;
	public Matrix4x4 deviceMatrix;
	public Quaternion deviceRotation;

	public GameObject deviceCamera;
	public Matrix4x4 deviceCameraMatrix;
	public Quaternion deviceCameraRotation;
	public Camera deviceCameraCamera;
	//public GameObject deviceQuad;

    public int isTranslation = 0;
    public int isRotation = 0;
    public int isScale = 0;

	public bool connected;
    public int color;

	public Client(){
		this.deviceMatrix = Matrix4x4.identity;
		this.connected = true;
		this.deviceObject = null;
		//this.deviceQuad = null;
		this.deviceRotation = new Quaternion ();
		this.deviceCameraMatrix = Matrix4x4.identity;
		this.deviceCamera = null;
		this.deviceCameraRotation = new Quaternion ();
		this.deviceCameraCamera = new Camera ();
	}

}

public class TCP_server : MonoBehaviour {
	
	private volatile Transforms t = new Transforms();
	
	private volatile List<Client> clients = new List<Client>();
	private bool STOP = false;
	
	public TcpListener tcpListener;
	private Thread tcpServerRunThread;

    GameObject objBox;
    GameObject objBoxSmooth;
    GameObject objCamera;


	void Awake() {

        objBox = GameObject.FindGameObjectWithTag("box");
        objBoxSmooth = GameObject.FindGameObjectWithTag("boxSmooth");
        objCamera = GameObject.FindGameObjectWithTag("MainCamera");

        t.cameraPosition = objCamera.transform.position;
        t.boxPosition = objBox.transform.position;
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

            client.color = BitConverter.ToInt32(bytes, 257);
            client.deviceMatrix = t.viewMatrix.inverse * clientDeviceMatrix;

            //Camera Rotation
            if (!t.isCameraRotation) t.isCameraRotation = Convert.ToBoolean(bytes[256]);
            Matrix4x4 rot = clientRotMatrix;
            Matrix4x4 tr = t.viewMatrix.inverse * rot * t.viewMatrix;
            if (!t.isCameraRotation) t.rotateMatrix = tr * t.rotateMatrix;
            else t.rotateCameraMatrix = tr * t.rotateCameraMatrix;
            
            //Translation
            Vector4 translation = clientTransMatrix.GetColumn(3);
            translation = t.viewMatrix.inverse * translation;
            Matrix4x4 tt = Matrix4x4.identity;
            tt.SetColumn(3, translation);
            t.translateMatrix = tt * t.translateMatrix;

            //Scalling
            Matrix4x4 ts = clientScaleMatrix;
            t.scaleMatrix = ts * t.scaleMatrix;

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
            if (c.deviceCameraCamera.rect == null) continue;
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

        objBoxSmooth.transform.position = Vector3.Lerp(objBoxSmooth.transform.position, objBox.transform.position, 0.3f);
        objBoxSmooth.transform.rotation = Quaternion.Lerp(objBoxSmooth.transform.rotation, objBox.transform.rotation, 0.4f);
        objBoxSmooth.transform.localScale = Vector3.Lerp(objBoxSmooth.transform.localScale, objBox.transform.localScale, 0.7f);


        foreach (Client c in clients)
        {

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
        t.rotateCameraMatrix = Utils.fixMatrix(t.rotateCameraMatrix);
        t.rotateMatrix = Utils.fixMatrix(t.rotateMatrix);
        t.viewMatrix = Utils.fixMatrix(t.viewMatrix);

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
        t.boxPosition = objBox.transform.position + translation;
        //t.translateMatrix = Matrix4x4.identity;

        Matrix4x4 rotate = Matrix4x4.TRS(new Vector3(0, 0, 0), GameObject.FindGameObjectWithTag("box").transform.rotation, new Vector3(1, 1, 1));
        rotate = t.rotateMatrix * rotate;

        GameObject.FindGameObjectWithTag("box").transform.rotation = rotate.GetRotation();
        t.rotateMatrix = Matrix4x4.identity;

        GameObject.FindGameObjectWithTag("box").transform.position = t.boxPosition;
		GameObject.FindGameObjectWithTag ("box").transform.localScale = t.scaleMatrix.GetScale ();
		//print (t.translateMatrix.GetPosition ());

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
		GameObject.FindGameObjectWithTag ("MainCamera").transform.position = Vector3.Slerp(GameObject.FindGameObjectWithTag ("MainCamera").transform.position, t.cameraPosition, 0.1f);


        GameObject.FindGameObjectWithTag("MainCamera").transform.LookAt(t.boxPositionSmooth);
		t.viewMatrix = GameObject.FindGameObjectWithTag ("MainCamera").transform.worldToLocalMatrix;
		t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
		float y = 0.75f;
		foreach (Client c in clients) {
			
			if(c.deviceObject == null){
				c.deviceObject = GameObject.Instantiate (GameObject.FindGameObjectWithTag ("device"));
                
				c.deviceCamera = GameObject.Instantiate (GameObject.FindGameObjectWithTag ("MainCamera"));
				//c.deviceQuad = GameObject.Instantiate(GameObject.FindGameObjectWithTag("deviceQuad"));

				c.deviceCamera.transform.parent = c.deviceObject.transform;
				c.deviceRotation = c.deviceObject.transform.rotation;

                /*Color[] colors = {
                    newDeviceColor(0xA30501),
                    newDeviceColor(0xE1D145),
                    newDeviceColor(0x299745),
                    newDeviceColor(0x45BFE1),
                    newDeviceColor(0x0096B2)/*,
                    new Color(109/255.0f, 181/255.0f, 81/255.0f, 0.7f),
                    new Color(241/255.0f, 67/255.0f, 63/255.0f, 0.7f),
                    new Color(247/255.0f, 233/255.0f, 103/255.0f, 0.7f),
                    new Color(112/255.0f, 183/255.0f, 186/255.0f, 0.7f),
                    new Color(61/255.0f, 76/255.0f, 83/255.0f, 0.7f),
                    new Color(169/255.0f, 207/255.0f, 84/255.0f, 0.7f)*/
                //};

                //c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[2].color = colors[colorCount];
                //colorCount = (colorCount + 1) % colors.Length;

			}

            c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[2].color = Utils.HexColor(c.color, 0.8f); //borda
            c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[1].color = Utils.HexColor(c.color, 1.0f); //botao
            c.deviceObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().materials[3].color = Utils.HexColor(c.color, 0.2f); //tela

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

            Vector3 v = GameObject.FindGameObjectWithTag("boxSmooth").transform.position;
			v = v - (Vector3) r.GetColumn(2);

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

