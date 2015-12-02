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
	private bool mRunning;
	public static string msg = "";

	private volatile float[] rotatingMatrix = new float[16];
	private volatile float[] translatingMatrix = new float[16];
	private volatile float[] scalingMatrix = new float[16];

	public bool STOP = false;

	public Thread mThread;
	public TcpListener tcp_Listener = null;

	static TcpListener tcpListener = new TcpListener(IPAddress.Parse("143.54.13.40"), 8002);

	List<Thread> threads = new List<Thread>();

	void Awake() {
		tcpListener.Start();

		//Console.WriteLine("************This is Server program************");
		//Console.WriteLine("Hoe many clients are going to connect to this server?:");
		//int numberOfClientsYouNeedToConnect =int.Parse( Console.ReadLine());
		for (int i = 0; i < 10; i++)
		{
			Thread newThread = new Thread(new ThreadStart(Listeners));
			newThread.Start();
			threads.Add(newThread);
		}

		rotatingMatrix [0] = rotatingMatrix [5] = rotatingMatrix [10] = rotatingMatrix [15] = 1;
		translatingMatrix [0] = translatingMatrix [5] = translatingMatrix [10] = translatingMatrix [15] = 1;
		scalingMatrix [0] = scalingMatrix [5] = scalingMatrix [10] = scalingMatrix [15] = 1;

//		mRunning = true;
//		ThreadStart ts = new ThreadStart(Receive);
//		mThread = new Thread(ts);
//		mThread.Start();
//		print("Thread done...");
	}
	
	public void stopListening() {
		mRunning = false;
	}
	
	List<TcpClient > clients = new List<TcpClient >();


	void Listeners()
	{
		float[] tempRotMatrix = new float[16];
		float[] tempTransMatrix = new float[16];
		float[] tempScaleMatrix = new float[16];

		TcpClient socketForClient = tcpListener.AcceptTcpClient();
		if (socketForClient.Connected)
		{
			//print("Client:"+socketForClient.RemoteEndPoint+" now connected to server.");
			NetworkStream networkStream = socketForClient.GetStream();

			StreamReader reader = new StreamReader(networkStream);
			//msg = reader.ReadLine();
			
			//			System.IO.StreamWriter streamWriter =
//				new System.IO.StreamWriter(networkStream);
//			System.IO.StreamReader streamReader =
//				new System.IO.StreamReader(networkStream);
			
			////here we send message to client
			//Console.WriteLine("type your message to be recieved by client:");
			//string theString = Console.ReadLine();
			//streamWriter.WriteLine(theString);
			////Console.WriteLine(theString);
			//streamWriter.Flush();
			
			//while (true)
			//{
			//here we recieve client's text if any.
			while (socketForClient.Connected && !STOP)
			{
				byte[] bytes = new byte[192];

				//print(networkStream.Length);

				networkStream.Read(bytes, 0, bytes.Length);
				tempRotMatrix = ConvertByteToFloat(bytes, 0, 64);
				tempTransMatrix = ConvertByteToFloat(bytes, 64, 64);
				tempScaleMatrix = ConvertByteToFloat(bytes, 128, 64);

				Matrix4x4 tr = Utils.convertToMatrix(tempRotMatrix);
				Matrix4x4 r = Utils.convertToMatrix(rotatingMatrix);

				tr = tr * r;
				rotatingMatrix = Utils.convertToFloat(tr);

				Matrix4x4 tt = Utils.convertToMatrix(tempTransMatrix);
				Matrix4x4 t = Utils.convertToMatrix(translatingMatrix);

				tt = tt * t;
				translatingMatrix = Utils.convertToFloat(tt);

				Matrix4x4 ts = Utils.convertToMatrix(tempScaleMatrix);
				Matrix4x4 s = Utils.convertToMatrix(scalingMatrix);
				
				ts = ts * s;
				scalingMatrix = Utils.convertToFloat(ts);



				//print ("CCCC");

				//transform
				//print ("*0*" + transform[0][0] + ","+ transform[0][1]);
				//print ("*1*" + stream[1] + ","+ stream[5] + ","+ stream[9] + "," + stream[13] );
				//print ("*2*" + stream[2] + ","+ stream[6] + ","+ stream[10] + "," + stream[14] );
				//print ("*3*" + stream[3] + ","+ stream[7] + ","+ stream[11] + "," + stream[15] );

				// Read can return anything from 0 to numBytesToRead. 
				// This method blocks until at least one byte is read.
				//netStream.Read (bytes, 0, (int)tcpClient.ReceiveBufferSize);
				
				// Returns the data received from the host to the console.
				//


				//string theString = reader.ReadLine();
				//if(theString.Length == 0)
				//	continue;
				//print("Message recieved by client:" + theString);
				//if (theString == "exit")
				//	break;

			}
			reader.Close();
			networkStream.Close();
			//streamWriter.Close();
			//}
			
		}
		socketForClient.Close();
		///print("Press any key to exit from server program");
		//Console.ReadKey();
	}
	
	public static float[] ConvertByteToFloat(byte[] array, int offSet, int size) {

		float[] floats = new float[size / 4];
		
		for (int i = 0; i < size / 4; i++)
			floats[i] = BitConverter.ToSingle(array, i * 4 + offSet);
		
		return floats;

		//var floatArray2 = new float[array.Length / 4];
		//Buffer.BlockCopy(array, 0, floatArray2, 0, array.Length);

		//float[] floatArr = new float[16];
		//for (int i = 0; i < floatArr.Length; i++) {
			//if (BitConverter.IsLittleEndian) {
			//	Array.Reverse(array,0, 16);
			//}
		//	floatArr[i] = BitConverter.ToSingle(array,0);
		//}
		//return floatArray2;
	}
	
	
	void Update() {
		Matrix4x4 mr = Utils.convertToMatrix(rotatingMatrix);
		Matrix4x4 mt = Utils.convertToMatrix (translatingMatrix);
		Matrix4x4 ms = Utils.convertToMatrix (scalingMatrix);

//		print(Utils.matrixString (rotatingMatrix));
//		print(Utils.matrixString (translatingMatrix));
		print(Utils.matrixString (scalingMatrix));
		
		GameObject.FindGameObjectWithTag("box").transform.rotation = Utils.GetRotation(mr);
		GameObject.FindGameObjectWithTag("box").transform.position = Utils.GetPosition(mt);
		GameObject.FindGameObjectWithTag("box").transform.localScale = Utils.GetScale(ms);

		
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

		Debug.DrawLine (Vector3.zero, mr.GetColumn(0)*5, Color.red);
		Debug.DrawLine (Vector3.zero, mr.GetColumn(1)*5, Color.green);
		Debug.DrawLine (Vector3.zero, mr.GetColumn(2)*5, Color.blue);
	
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
