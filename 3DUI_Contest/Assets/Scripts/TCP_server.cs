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
	public Matrix4x4[] deviceMatrix = new Matrix4x4[10];
	public Matrix4x4 rotatingMatrix = new Matrix4x4();
	public Matrix4x4 translatingMatrix = new Matrix4x4();
	public Matrix4x4 scalingMatrix = new Matrix4x4();
	public Matrix4x4 viewMatrix = new Matrix4x4();
}

public class TCP_server : MonoBehaviour {
	private bool mRunning;
	public static string msg = "";

	private volatile transforms t = new transforms();

	private volatile int devicesConnected = 0;
	public bool STOP = false;

	public Thread mThread;
	public TcpListener tcp_Listener = null;

	static TcpListener tcpListener = new TcpListener(IPAddress.Parse("143.54.13.40"), 8002);


	private GameObject[] devicesObject = new GameObject[10];

	
	List<Thread> threads = new List<Thread>();


	void Awake() {

		for (int i = 0; i < 10; i++)
			 devicesObject[i] = GameObject.Instantiate(GameObject.FindGameObjectWithTag("device"));

		tcpListener.Start();

		for (int i = 0; i < 10; i++)
		{
			Thread newThread = new Thread(new ThreadStart(Listeners));
			newThread.Start();
			threads.Add(newThread);
		}
		
		for(int i = 0; i < 10; i++) t.deviceMatrix[i] = Matrix4x4.identity;
		t.rotatingMatrix = Matrix4x4.identity;
		t.scalingMatrix = Matrix4x4.identity;
		t.translatingMatrix = Matrix4x4.identity;
		t.viewMatrix = Matrix4x4.identity;
		Vector3 pos = GameObject.FindGameObjectWithTag ("box").transform.position;
		t.translatingMatrix.SetColumn (3, new Vector4 (pos.x, pos.y, pos.z, 1.0f));

	}
	
	public void stopListening() {
		mRunning = false;
	}
	
	List<TcpClient > clients = new List<TcpClient >();


	void Listeners()
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

				//print (Utils.matrixString(tempDeviceMatrix));

				t.deviceMatrix[clientId] = Utils.convertToMatrix(tempDeviceMatrix);

				Matrix4x4 tr = Utils.convertToMatrix(tempRotMatrix);
				/*tr.SetColumn(0, new Vector4(1,0,0,0));
				tr.SetColumn(1, new Vector4(0,1,0,0));
				tr.SetColumn(2, new Vector4(0,0,1,0));*/

				t.rotatingMatrix = tr * t.rotatingMatrix;

				Matrix4x4 tt = Utils.convertToMatrix(tempTransMatrix);
				t.translatingMatrix = ( tt) * t.translatingMatrix;

				Matrix4x4 ts = Utils.convertToMatrix(tempScaleMatrix);
				t.scalingMatrix = ( ts) * t.scalingMatrix;


			}
			reader.Close();
			networkStream.Close();
		}
		socketForClient.Close();
	}
	
	public static float[] ConvertByteToFloat(byte[] array, int offSet, int size) {

		float[] floats = new float[size / 4];
		
		for (int i = 0; i < size / 4; i++)
			floats[i] = BitConverter.ToSingle(array, i * 4 + offSet);
		
		return floats;
	}

	Vector3 positionSmooth;
	
	void Update() {


		//print(Utils.matrixString (scalingMatrix));
		
		GameObject.FindGameObjectWithTag("box").transform.rotation = t.rotatingMatrix.GetRotation();
		GameObject.FindGameObjectWithTag("box").transform.position = t.translatingMatrix.GetPosition();
		GameObject.FindGameObjectWithTag("box").transform.localScale = t.scalingMatrix.GetScale();

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

		foreach(Thread t in threads){
			//t.Interrupt();
			if(!t.Join(500)){
				t.Abort();
			}
		}
		tcpListener.Stop();
	
	}
}
