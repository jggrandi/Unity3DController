using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class RealTimeTube : MonoBehaviour {
	//Net stuff
	private const int gariPort = 5050;
	private UdpClient listener;
	private IPEndPoint groupEP;
	private int discardCount = 0;

	private Vector3 previous = Vector3.zero;
	Boolean first = true;
	private Quaternion baseRotation;

	void Start () {
		listener = new UdpClient (gariPort);
		groupEP = new IPEndPoint (IPAddress.Any, gariPort);
		listener.Client.SetSocketOption (SocketOptionLevel.Socket, 
		                                 SocketOptionName.ReceiveBuffer, 0);
		//followPipe = true;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		byte[] recvBytes = listener.Receive (ref groupEP);
		discardCount++;
		//if (discardCount == 30) 
		//{
			String dataLine = Encoding.ASCII.GetString (recvBytes, 0, recvBytes.Length);
			string[] quatStep = dataLine.Split('#');
			
		float qW = float.Parse(quatStep[0]);
		float qX = float.Parse(quatStep[1]);
		float qY = float.Parse(quatStep[2]);
		float qZ = float.Parse(quatStep[3]);

		Quaternion quat = new Quaternion (qX, qY, qZ, qW);

		print (quat.w);
		print (quat.x);
		print (quat.y);
		print (quat.z);

		gameObject.transform.rotation = Quaternion.Slerp (gameObject.transform.rotation, quat, Time.time * 0.1f);

		//gameObject.transform.rotation.eulerAngles.x = quat.eulerAngles.x;
		//this.transform.Rotate (quat.eulerAngles ());
			//if(quat.x < 0)
			//{
			//	;//quat.w = -1*quat.w;
			//}

			//if(first)
			//{
			//	GameObject go = (GameObject)Instantiate(imuPoint,previous,quat);
			//	baseRotation = new Quaternion(quat.x,quat.y,quat.z,quat.w);
			//	first = false;
			//}
			//else
			//{

			//	GameObject go = (GameObject)Instantiate(imuPoint,previous,Quaternion.Inverse(baseRotation)*quat);
			//	go.transform.Translate(-1*Vector3.right/1000*float.Parse(quatStep[4]),Space.Self);
			//	//go.transform.Translate(new Vector3(0,0,1.5f),Space.Self);
			//	go.transform.localScale += new Vector3(0,float.Parse(quatStep[4])/2000-0.005F);
			//	go.transform.Rotate(new Vector3(0,0,90));
			//	if(followPipe)
			//	{
			//		gameCamera.transform.position = go.transform.position + new Vector3(10,10,0);
			//		gameCamera.transform.LookAt(go.transform);
			//	}
		//
		//		previous = go.transform.position;
		//		pipeLenght += float.Parse(quatStep[4])/1000;
		//
		//u	}

			discardCount = 0;
		//}
	
	}
}
