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
	public Vector3 boxPostion = new Vector3();
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
	public GameObject deviceQuad;
	
	public bool connected;

	public Client(){
		this.deviceMatrix = Matrix4x4.identity;
		this.connected = true;
		this.deviceObject = null;
		this.deviceQuad = null;
		this.deviceRotation = new Quaternion ();
		this.deviceCameraMatrix = Matrix4x4.identity;
		this.deviceCamera = null;
		this.deviceCameraRotation = new Quaternion ();
		this.deviceCameraCamera = new Camera ();
	}

}

public class TCP_server : MonoBehaviour {
	
	private volatile Transforms t = new Transforms();
	
	private Vector4[] clientColors = new [] { new Vector4(0.9f,0.7f,0.3f,0.5f), new Vector4(0.7f,0.9f,0.3f,0.5f), new Vector4(0.3f,0.7f,0.9f,0.5f), new Vector4(0.3f,0.9f,0.7f,0.5f), new Vector4(0.9f,0.3f,0.7f,0.5f) };

	private volatile List<Client> clients = new List<Client>();
	private bool STOP = false;
	
	public TcpListener tcpListener;
	private Thread tcpServerRunThread;

	Vector3 positionSmooth;
	
	//List<Thread> threads = new List<Thread>();
	
	void Awake() {

		
		t.cameraPosition = GameObject.FindGameObjectWithTag ("MainCamera").transform.position;
		t.boxPostion = GameObject.FindGameObjectWithTag ("box").transform.position;

		//for (int i = 0; i < 10; i++)
		//	devicesObject[i] = GameObject.Instantiate(GameObject.FindGameObjectWithTag("device"));
				
		//for(int i = 0; i < 10; i++) t.deviceMatrix[i] = Matrix4x4.identity;

		t.rotateMatrix = Matrix4x4.identity;
		t.scaleMatrix = Matrix4x4.identity;
		t.translateMatrix = Matrix4x4.identity;
		t.viewMatrix = Matrix4x4.identity;
		t.rotateCameraMatrix = Matrix4x4.identity;

		Vector3 pos = GameObject.FindGameObjectWithTag ("box").transform.position;
		t.translateMatrix.SetColumn (3, new Vector4 (pos.x, pos.y, pos.z, 1.0f));
		
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

		float[] clientDeviceMatrix = new float[16];
		float[] clientRotMatrix = new float[16];
		float[] clientTransMatrix = new float[16];
		float[] clientScaleMatrix = new float[16];


		NetworkStream stream = clientDevice.GetStream ();
		while (clientDevice.Connected && !STOP) {
			
			byte[] bytes = new byte[257];
			if(stream.Read(bytes, 0, bytes.Length) < 257) break;

			clientRotMatrix    = ConvertByteToFloat(bytes, 0,  64);
			clientTransMatrix  = ConvertByteToFloat(bytes, 64, 64);
			clientScaleMatrix  = ConvertByteToFloat(bytes, 128,64);
			clientDeviceMatrix = ConvertByteToFloat(bytes, 192,64);

			if(!t.isCameraRotation)
				t.isCameraRotation = Convert.ToBoolean(bytes[256]);
		
			client.deviceMatrix =  t.viewMatrix.inverse * Utils.convertToMatrix(clientDeviceMatrix);

			Matrix4x4 tr =  t.viewMatrix.inverse * Utils.convertToMatrix(clientRotMatrix) * t.viewMatrix ;
			if(!t.isCameraRotation)
				t.rotateMatrix = tr * t.rotateMatrix;
			else
				t.rotateCameraMatrix = tr * t.rotateCameraMatrix;

			Vector4 translation = Utils.convertToMatrix(clientTransMatrix).GetColumn(3);
			translation = t.viewMatrix.inverse * translation;
			Matrix4x4 tt = Matrix4x4.identity;
			tt.SetColumn(3, translation);
			t.translateMatrix = tt * t.translateMatrix;
			
			Matrix4x4 ts = Utils.convertToMatrix(clientScaleMatrix);
			t.scaleMatrix = ts * t.scaleMatrix;
		}
		stream.Close ();
		clientDevice.Close ();
		client.connected = false;
	}
	

	public static float[] ConvertByteToFloat(byte[] array, int offSet, int size) {
		
		float[] floats = new float[size / 4];
		
		for (int i = 0; i < size / 4; i++)
			floats[i] = BitConverter.ToSingle(array, i * 4 + offSet);
		
		return floats;
	}

	void OnGUI(){
		// Apply a color label to each client's PIP 
		foreach (Client c in clients) {
			float posRecX = (c.deviceCameraCamera.rect.width * Screen.width - 10) + c.deviceCameraCamera.rect.x * Screen.width ;
			float posRecY = (c.deviceCameraCamera.rect.height - 10) + (1 - c.deviceCameraCamera.rect.y - c.deviceCameraCamera.rect.height) * Screen.height ;
			Rect rec = new Rect(posRecX, posRecY, 20, 20);

			GUIStyle currentStyle = new GUIStyle( GUI.skin.box );
			currentStyle.normal.background = MakeTex( 2, 2,c.deviceObject.GetComponent<Renderer>().material.color );

			GUI.Box (rec, "", currentStyle);
		}
	}

	private Texture2D MakeTex( int width, int height, Color col )
	{
		Color[] pix = new Color[width * height];
		for( int i = 0; i < pix.Length; ++i )
		{
			pix[ i ] = col;
		}
		Texture2D result = new Texture2D( width, height );
		result.SetPixels( pix );
		result.Apply();
		return result;
	}
	
	void Update() {

		for(int i = clients.Count - 1; i >= 0; i--){
			if(!clients[i].connected){
				GameObject.Destroy(clients[i].deviceObject);
				clients.RemoveAt(i);
			}
		}

		//print (t.isCameraRotation);

		t.boxPostion = 0.8f * t.boxPostion + 0.2f * t.translateMatrix.GetPosition ();

		GameObject.FindGameObjectWithTag ("box").transform.rotation = t.rotateMatrix.GetRotation ();
		GameObject.FindGameObjectWithTag ("box").transform.position = t.boxPostion;
		GameObject.FindGameObjectWithTag ("box").transform.localScale = t.scaleMatrix.GetScale ();
		//print (t.translateMatrix.GetPosition ());

		positionSmooth = 0.95f * positionSmooth + 0.05f * t.boxPostion;
		Vector3 pos = positionSmooth;
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


		GameObject.FindGameObjectWithTag ("MainCamera").transform.LookAt (positionSmooth);
		t.viewMatrix = GameObject.FindGameObjectWithTag ("MainCamera").transform.worldToLocalMatrix;
		t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
		float y = 0.75f;
		foreach (Client c in clients) {
			
			if(c.deviceObject == null){
				c.deviceObject = GameObject.Instantiate (GameObject.FindGameObjectWithTag ("device"));
				c.deviceCamera = GameObject.Instantiate (GameObject.FindGameObjectWithTag ("MainCamera"));
				c.deviceQuad = GameObject.Instantiate(GameObject.FindGameObjectWithTag("deviceQuad"));

				c.deviceCamera.transform.parent = c.deviceObject.transform;
				c.deviceRotation = c.deviceObject.transform.rotation;

				Color cc = new Color(UnityEngine.Random.Range(0.0f,1.0f), UnityEngine.Random.Range(0.0f,1.0f), UnityEngine.Random.Range(0.0f,1.0f), 0.5f); 
				c.deviceObject.GetComponent<Renderer>().material.color = cc;

			}

			c.deviceCameraCamera = c.deviceCamera.GetComponent<Camera>();
			c.deviceCameraCamera.rect = new Rect(0.75f,y,0.2f,0.2f);
			c.deviceCameraCamera.transform.LookAt(t.boxPostion);
			c.deviceCameraCamera.orthographic = true;
			c.deviceCameraCamera.orthographicSize = 2.0f;

			y-=0.25f;
			c.deviceRotation = Quaternion.Slerp(c.deviceMatrix.GetRotation(), c.deviceRotation, 0.5f);
			c.deviceObject.transform.rotation = c.deviceRotation;

			Matrix4x4 r = Matrix4x4.TRS (new Vector3(0,0,0), c.deviceRotation, new Vector3 (1,1,1));

			Vector3 v = t.boxPostion;
			v = v - (Vector3) r.GetColumn(2);

			c.deviceObject.transform.position = v;
			c.deviceCameraCamera.transform.position = v ;


		}

	}
	
	void OnApplicationQuit() { // stop listening thread
		STOP = true;

		tcpServerRunThread.Abort ();
		tcpListener.Stop();

	}
}

