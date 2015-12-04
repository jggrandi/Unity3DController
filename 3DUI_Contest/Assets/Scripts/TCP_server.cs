using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;


public class transforms{
	public Matrix4x4[] deviceMatrix    = new Matrix4x4[10];
	public Matrix4x4   rotateMatrix    = new Matrix4x4();
	public Matrix4x4   translateMatrix = new Matrix4x4();
	public Matrix4x4   scaleMatrix     = new Matrix4x4();
	public Matrix4x4   viewMatrix      = new Matrix4x4();
}

public class TCP_server : MonoBehaviour {
	
	private volatile transforms t = new transforms();
	
	public Thread mThread;
	private GameObject[] devicesObject = new GameObject[10];
	private volatile int devicesConnected = 0;
	private bool STOP = false;
	
	public TcpListener tcpListener;
	private Thread tcpServerRunThread;
	
	
	//List<Thread> threads = new List<Thread>();
	
	void Awake() {
		
		for (int i = 0; i < 10; i++)
			devicesObject[i] = GameObject.Instantiate(GameObject.FindGameObjectWithTag("device"));
		
		/*		for (int i = 0; i < 10; i++)
		{
			Thread newThread = new Thread(new ThreadStart(Listeners));
			newThread.Start();
			threads.Add(newThread);
		}*/
		
		for(int i = 0; i < 10; i++) t.deviceMatrix[i] = Matrix4x4.identity;
		t.rotateMatrix = Matrix4x4.identity;
		t.scaleMatrix = Matrix4x4.identity;
		t.translateMatrix = Matrix4x4.identity;
		t.viewMatrix = Matrix4x4.identity;
		Vector3 pos = GameObject.FindGameObjectWithTag ("box").transform.position;
		t.translateMatrix.SetColumn (3, new Vector4 (pos.x, pos.y, pos.z, 1.0f));
		
		tcpListener = new TcpListener(IPAddress.Parse("192.168.111.21"), 8002);
		tcpListener.Start();
		
		tcpServerRunThread = new Thread (new ThreadStart (TcpServerRun));
		tcpServerRunThread.Start ();
		
	}
	
	public void TcpServerRun(){
		while(!STOP) {
			
			try{
				print ("A");
				TcpClient c = tcpListener.AcceptTcpClient();
				print ("B");
				new Thread(new ThreadStart(()=> DeviceListener(c))).Start();
			}
			catch(Exception ex){
				print("Error Listener thread");
				print (ex.Message);
			}
		}
	}
	
	public void stopListening() {
	}
	
	void DeviceListener (TcpClient clientDevice){
		print ("AQUI");
		int clientId = devicesConnected++;
		
		float[] clientDeviceMatrix = new float[16];
		float[] clientRotMatrix = new float[16];
		float[] clientTransMatrix = new float[16];
		float[] clientScaleMatrix = new float[16];
		print ("3");
		
		NetworkStream stream = clientDevice.GetStream ();
		while (clientDevice.Connected && !STOP) {
			
			byte[] bytes = new byte[256];
			stream.Read(bytes, 0, bytes.Length);
			
			clientRotMatrix    = ConvertByteToFloat(bytes, 0,  64);
			clientTransMatrix  = ConvertByteToFloat(bytes, 64, 64);
			clientScaleMatrix  = ConvertByteToFloat(bytes, 128,64);
			clientDeviceMatrix = ConvertByteToFloat(bytes, 192,64);
			
			t.deviceMatrix[clientId] = Utils.convertToMatrix(clientDeviceMatrix);
			
			Matrix4x4 tr = Utils.convertToMatrix(clientRotMatrix);
			t.rotateMatrix = tr * t.rotateMatrix;
			
			Matrix4x4 tt = Utils.convertToMatrix(clientTransMatrix);
			t.translateMatrix = ( tt) * t.translateMatrix;
			
			Matrix4x4 ts = Utils.convertToMatrix(clientScaleMatrix);
			t.scaleMatrix = ( ts) * t.scaleMatrix;
		}
		stream.Close ();
		clientDevice.Close ();
	}
	
	
	
	/*	void Listeners()
	{
		float[] tempDeviceMatrix = new float[16];
		float[] tempRotMatrix = new float[16];
		float[] tempTransMatrix = new float[16];
		float[] tempScaleMatrix = new float[16];

		TcpClient socketForClient = tcpListener.AcceptTcpClient();
		if (socketForClient.Connected)
		{
			int clientId = devicesConnected++;

			NetworkStream networkStream = socketForClient.GetStream();

			StreamReader reader = new StreamReader(networkStream);

			while (socketForClient.Connected && !STOP)
			{
				byte[] bytes = new byte[256];

				networkStream.Read(bytes, 0, bytes.Length);

				tempRotMatrix = ConvertByteToFloat(bytes, 0, 64);
				tempTransMatrix = ConvertByteToFloat(bytes, 64, 64);
				tempScaleMatrix = ConvertByteToFloat(bytes, 128, 64);
				tempDeviceMatrix = ConvertByteToFloat(bytes, 192, 64);

				t.deviceMatrix[clientId] = Utils.convertToMatrix(tempDeviceMatrix);

				Matrix4x4 tr = Utils.convertToMatrix(tempRotMatrix);
				t.rotateMatrix = tr * t.rotateMatrix;

				Matrix4x4 tt = Utils.convertToMatrix(tempTransMatrix);
				t.translateMatrix = ( tt) * t.translateMatrix;

				Matrix4x4 ts = Utils.convertToMatrix(tempScaleMatrix);
				t.scaleMatrix = ( ts) * t.scaleMatrix;
			}
			reader.Close();
			networkStream.Close();
		}
		socketForClient.Close();
	}*/
	
	public static float[] ConvertByteToFloat(byte[] array, int offSet, int size) {
		
		float[] floats = new float[size / 4];
		
		for (int i = 0; i < size / 4; i++)
			floats[i] = BitConverter.ToSingle(array, i * 4 + offSet);
		
		return floats;
	}
	
	Vector3 positionSmooth;
	
	void Update() {
		
		GameObject.FindGameObjectWithTag("box").transform.rotation = t.rotateMatrix.GetRotation();
		GameObject.FindGameObjectWithTag("box").transform.position = t.translateMatrix.GetPosition();
		GameObject.FindGameObjectWithTag("box").transform.localScale = t.scaleMatrix.GetScale();
		
		positionSmooth = 0.95f * positionSmooth + 0.05f * GameObject.FindGameObjectWithTag ("box").transform.position;
		
		Vector3 pos = positionSmooth;
		Vector3 cam = GameObject.FindGameObjectWithTag ("MainCamera").transform.position;
		Vector3 dir = Vector3.Normalize(pos - cam);
		dir.z = dir.y = 0;
		dir = Vector3.Normalize(dir);
		
		GameObject.FindGameObjectWithTag("MainCamera").transform.position = 0.1f * (pos + (-5 * dir)) + 0.9f * cam;
		GameObject.FindGameObjectWithTag ("MainCamera").transform.LookAt (positionSmooth);
		t.viewMatrix = GameObject.FindGameObjectWithTag("MainCamera").transform.localToWorldMatrix;
		t.viewMatrix.SetColumn (3, new Vector4 (0, 0, 0, 1));
		
		for (int i = 0; i < devicesConnected; i++) {
			devicesObject[i].transform.rotation = t.deviceMatrix[i].GetRotation();
			
			Vector3 v = GameObject.FindGameObjectWithTag("box").transform.position;
			v = v - (Vector3)t.deviceMatrix[i].GetColumn(2);
			devicesObject[i].transform.position = v;
			
		}
		
		
		/*Debug.DrawLine (Vector3.zero, deviceDebugMatrix.GetColumn(0)*5, Color.red);
		Debug.DrawLine (Vector3.zero, deviceDebugMatrix.GetColumn(1)*5, Color.green);
		Debug.DrawLine (Vector3.zero, deviceDebugMatrix.GetColumn(2)*5, Color.blue);*/
		
		
		
		//		GameObject.FindGameObjectWithTag("MainCamera").transform.position = Utils.GetPosition(mt);
		
		//		float x = GameObject.FindGameObjectWithTag ("MainCamera").transform.position.x+1000;
		//		float y = GameObject.FindGameObjectWithTag ("MainCamera").transform.position.y+1000;
		//		float z = GameObject.FindGameObjectWithTag ("MainCamera").transform.position.z+1000;
		
		
		
		//		GameObject.FindGameObjectWithTag ("MainCamera").transform.position.Set(x,y,z);
		
		//Vector3 x = m.GetColumn (0);
		//Vector3 z = m.GetColumn (2);
		
		//Vector3.Dot(x,
		//print (" " + mr.GetRow(0));
		//print (" " + mr.GetRow(1));
		//print (" " + mr.GetRow(2));
		//print (" " + mr.GetRow(3));
		
		/*Debug.DrawLine (Vector3.zero, mr.GetColumn(0)*5, Color.red);
		Debug.DrawLine (Vector3.zero, mr.GetColumn(1)*5, Color.green);
		Debug.DrawLine (Vector3.zero, mr.GetColumn(2)*5, Color.blue);*/
		
	}
	
	void OnApplicationQuit() { // stop listening thread
		stopListening();// wait for listening thread to terminate (max. 500ms)
		STOP = true;
		
		/*		foreach(Thread t in threads){
			//t.Interrupt();
			if(!t.Join(500)){
				t.Abort();
			}
		}*/
		tcpServerRunThread.Abort ();
		tcpListener.Stop();
		print ("SAIU");
	}
}

