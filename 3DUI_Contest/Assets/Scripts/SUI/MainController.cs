using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;




public class MainController : MonoBehaviour {

	public static MainController control;

	public volatile List<Client> clients = new List<Client>();
	public bool RUNNING = false;
	public TcpListener tcpListener;
	private Thread tcpServerRunThread;
	public volatile Transforms t = new Transforms();
	public float gameRuntime;


	void OnGUI(){
		//GUI.Label (new Rect (50, 50, 400, 50), "PlayTime:" + gameRuntime);
		//GUI.Label (new Rect (50, 60, 400, 50), "PlayTime:" + Time.realtimeSinceStartup);
	}

	void Awake () {
		
		if (control == null) {
			DontDestroyOnLoad (gameObject);
			control = this;
			tcpListener = new TcpListener (IPAddress.Any, 8002);
			tcpListener.Start ();
			RUNNING = true;
			tcpServerRunThread = new Thread (new ThreadStart (TcpServerRun));
			tcpServerRunThread.Start ();
			if (RecordGamePlay.SP != null)
				RecordGamePlay.SP.StartRecording ();
			else
				Debug.LogError("Record script not attached");

		} else if (control != this) {
			Destroy (gameObject);
		}

	}

	public void TcpServerRun(){
		while(RUNNING) {
			try{
				TcpClient c = tcpListener.AcceptTcpClient();
				Client client = new Client();
				clients.Add (client);
				//print(c.Client.RemoteEndPoint.ToString());

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


		while (clientDevice.Connected && RUNNING)
		{
			int pos = 0;
			byte[] bytes = new byte[261];
			bool abort = false;
			while (pos != 261 && !abort)
			{
				if (!clientDevice.Connected || !RUNNING) abort = true;
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

			//print (t.translateMatrix);

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

			if (Vector3.Magnitude (translation) > 0.001f) {
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

	public void OnApplicationQuit() { // stop listening thread
		RUNNING = false;

		tcpServerRunThread.Abort ();
		tcpListener.Stop();


		if(RecordGamePlay.SP != null)
			RecordGamePlay.SP.StopRecording ();

	}
		
}
