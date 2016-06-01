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

	public HandleConnections handleConnections = new HandleConnections();
	public volatile List<Client> clients = new List<Client>();
	public bool RUNNING = false;
	public TcpListener tcpListener;
	private Thread tcpServerRunThread;
	public volatile Transforms t = new Transforms();
	public Transforms objActualTranform = new Transforms();
	//public float gameRuntime = 0.0f;
	public float stackingDistance = 1000.0f;
	public String logFilename = "";


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
//			gameRuntime = Time.realtimeSinceStartup;
			stackingDistance = 1000.0f;
//			if(RecordGamePlay.SP != null)
//				RecordGamePlay.SP.StartRecording (gameRuntime);

		} else if (control != this) {
			Destroy (gameObject);
		}

	}

	void Update(){
//		gameRuntime = Time.realtimeSinceStartup; // Need to Update gameRuntime here because threads cant access Time.realtimeSinceStartup directly

//		if (RecordGamePlay.SP.replayData.Count >= 10) { // Save to a file when fill the buffer.
//			print ("Logs Saved!");
//			RecordGamePlay.SP.RecordDataToFile ();
//		}

	}

	public void TcpServerRun(){
		while(RUNNING) {
			try{
				TcpClient c = tcpListener.AcceptTcpClient();

				string ipport = c.Client.RemoteEndPoint.ToString(); // get the ip and port of the new client ip:port
				string[] ip = ipport.Split(':'); //separate ip and port. The ip is in ip[0] position.
				int clientId = handleConnections.getId(ip[0]); // get an id for the client. If the client has already connected with the same ip, the id will be the same as last connection.
				Client client = new Client(clientId);
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
//		float initTimeRot = 0.0f;
//		float initTimeTrans = 0.0f;
//		float initTimeScale = 0.0f;
//		float initTimeRotCam = 0.0f;
//		float initPoseErrorRot = 0.0f;
//		float initPoseErrorTrans = 0.0f;
//		float initPoseErrorScale = 0.0f;
//		Quaternion initRot = new Quaternion();
//		Quaternion initRotCam = new Quaternion();
//		Vector3 initTrans = new Vector3();
//		Vector3 initScale = new Vector3();
//
//		int inRotation = 0;
//		int inScale = 0;
//		int inTranlation = 0;
//		int inRotationCam = 0;


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

            if (!t.isCameraRotation) client.totalRotation *= tr.GetRotation();
            else client.totalRotationCamera *= tr.GetRotation();


			//Translation
			Vector4 translation = clientTransMatrix.GetColumn(3);
			translation.w = 0.0f;
			translation = t.viewMatrix.inverse * translation;
			if (translation.magnitude > 0.3f) translation = translation.normalized * 0.3f;
			translation.w = 1.0f;
			Matrix4x4 tt = Matrix4x4.identity;
			tt.SetColumn(3, translation);
			t.translateMatrix = tt * t.translateMatrix;

            client.totalTranslation += new Vector3(translation.x, translation.y, translation.z);
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
            client.totalScaling *= ts.GetScale().x;
			t.mutex.ReleaseMutex();

			if (Vector3.Magnitude (translation) > 0.001f) {
				client.isRotation = 0;
				client.isTranslation = 0;
				client.isScale = 0;
				client.isTranslation = 90;
//				inTranlation = 90;
			}

				

			if (rot [0, 0] < 0.9999999 || rot [1, 1] < 0.9999999 || rot [2, 2] < 0.9999999) {
				client.isRotation = 0;
				client.isTranslation = 0;
				client.isScale = 0;
				client.isRotation = 90;
//				if (t.isCameraRotation)
//					inRotationCam = 90;
//				else
//					inRotation = 90;
			}
				
				
			if ((ts [0, 0] > 1.001 || ts [0, 0] < 0.999) || (ts [1, 1] > 1.001 || ts [1, 1] < 0.999) || (ts [2, 2] > 1.001 || ts [2, 2] < 0.999)) {
				client.isRotation = 0;
				client.isTranslation = 0;
				client.isScale = 0;
				client.isScale = 90;
//				inScale = 90;
			}


//			if (RecordGamePlay.SP != null) { // It only register activities if the RecordGamePlay is being used
//				t.mutex.WaitOne ();
//				if (inRotation > 0) {
//					if (initTimeRot == 0.0f) {
//						initTimeRot = gameRuntime;
//						initRot = objActualTranform.rotateMatrix.GetRotation ();
//						initPoseErrorRot = stackingDistance;
//					}
//				} else if (inRotation < 10) {
//					if (initTimeRot > 0.0f) {
//						RecordGamePlay.SP.AddAction (RecordActions.playerAction, client.id, TransformationAction.rotation, initTimeRot, gameRuntime, initRot, objActualTranform.rotateMatrix.GetRotation (), initPoseErrorRot, stackingDistance);
//						initTimeRot = 0;
//					}
//				}
//
//				if (inRotationCam > 0) {
//					if (initTimeRotCam == 0.0f) {
//						initTimeRotCam = gameRuntime;
//						initRotCam = objActualTranform.rotateMatrix.GetRotation ();
//					}
//				} else if (inRotationCam < 10) {
//					if (initTimeRotCam > 0.0f) {
//						RecordGamePlay.SP.AddAction (RecordActions.playerAction, client.id, TransformationAction.cameraRotation, initTimeRotCam, gameRuntime, initRotCam, objActualTranform.rotateMatrix.GetRotation (), 0, 0);
//						initTimeRotCam = 0;
//					}
//				}
//
//				if (inTranlation > 0) {
//					if (initTimeTrans == 0.0f) {
//						initTimeTrans = gameRuntime;
//						initTrans = objActualTranform.boxPosition;
//						initPoseErrorTrans = stackingDistance;
//					}
//				} else if (inTranlation < 10) {
//					if (initTimeTrans > 0.0f) {
//						RecordGamePlay.SP.AddAction (RecordActions.playerAction, client.id, TransformationAction.translation, initTimeTrans, gameRuntime, initTrans, objActualTranform.boxPosition, initPoseErrorTrans, stackingDistance);
//						initTimeTrans = 0;
//	
//					}
//				}
//
//				if (inScale > 0) {
//					if (initTimeScale == 0.0f) {
//						initTimeScale = gameRuntime;
//						initScale = objActualTranform.scaleMatrix.GetScale ();
//						initPoseErrorScale = stackingDistance;
//					}
//				} else if (inScale < 10) {
//					if (initTimeScale > 0.0f) {
//						RecordGamePlay.SP.AddAction (RecordActions.playerAction, client.id, TransformationAction.scale, initTimeScale, gameRuntime, initScale, objActualTranform.scaleMatrix.GetScale (), initPoseErrorScale, stackingDistance);
//						initTimeScale = 0;
//					}
//				}
//				t.mutex.ReleaseMutex();
//			}
//				inRotation--;
//				inRotationCam--;
//				inScale--;
//				inTranlation--;
//				if (inRotation <= 0)
//					inRotation = 0;
//				if (inRotationCam <= 0)
//					inRotationCam = 0;			
//				if (inTranlation <= 0)
//					inTranlation = 0;
//				if (inScale <= 0)
//					inScale = 0;
//			
			//if (inScale > 0) inScale--;
			//if (inTranlation > 0) inTranlation--;

			//Byte[] b = System.Text.Encoding.UTF8.GetBytes("Teste");
			//clientDevice.Client.Send(b);
			//stream.Write(b, 0, b.Length);
			//stream.Flush();
		}

		stream.Close ();
		clientDevice.Close ();
		client.connected = false;

	}

	public void OnApplicationQuit() { // when application quit
		RUNNING = false;

		tcpServerRunThread.Abort ();  // Shutdown server
		tcpListener.Stop();


//		if (RecordGamePlay.SP != null) { 
//			RecordGamePlay.SP.StopRecording (); // stop gameplay recording
//			RecordGamePlay.SP.RecordDataToFile (); // save the remain buffed actions into file
//			RecordGamePlay.SP.CloseFile (); // close the file
//		}

	}
		
}
