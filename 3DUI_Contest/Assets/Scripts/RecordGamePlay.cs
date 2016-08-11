using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class Log{

	StreamWriter f;
	int numberOfClients;
	String filename = "";
	int numberOfCheckpoints;

	public Log(string team, int n, int c)
	{

		numberOfClients = n;
		numberOfCheckpoints = c;

		f = File.CreateText(Application.persistentDataPath + "/" + team + "-" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");
		f.WriteLine(n + "," + c + "," + team);

		string header = "Time,Translation X,Translation Y,Translation Z,Rotation X,Rotation Y,Rotation Z,Rotation W,Scalling,Camera X,Camera Y,Camera Z,Camera W,IsInCollision,CollisionForce X,CollisionForce Y,CollisionForce Z";
		for (int i = 0; i < c; i++) {
			header += ",Checkpoint" + i.ToString();
		}
		for (int i = 0; i < n; i++){
			header += ",User,Connected,Translation X,Translation Y,Translation Z,Rotation X,Rotation Y,Rotation Z,Rotation W,Scalling,Camera X,Camera Y,Camera Z,Camera W";
		}
		f.WriteLine(header);
	}

	public void close()
	{
		f.Close();
	}

	public void setFilename(String name){
		filename = name;
	}

	public void save( List<Client> clients, GameObject t, Quaternion cameraRotation, bool isInCollision, Vector3 collisionForce, float[] stackDist)
	{

		//if (clients.Count < numberOfClients) return;

		String line = "";

		line += Time.realtimeSinceStartup + "";

		line += "," + t.transform.position.x + "," + t.transform.position.y + "," + t.transform.position.z;
		line += "," + t.transform.rotation.x + "," + t.transform.rotation.y + "," + t.transform.rotation.z + "," + t.transform.rotation.w;
		line += "," + t.transform.localScale.x;
		line += "," + cameraRotation.x + "," + cameraRotation.y + "," + cameraRotation.z + "," + cameraRotation.w;
		line += "," + Convert.ToInt32(isInCollision) + "," + collisionForce.x + "," + collisionForce.y + "," + collisionForce.z;

		for (int j = 0; j < numberOfCheckpoints; j++) {
			line += "," + stackDist [j];
		}

		for (int j = 0; j < numberOfClients; j++) {

			bool connected = false;

			foreach (Client i in clients) {
				if (i.id != j) continue;
				line += "," + i.id + ",1";
				line += "," + i.totalTranslation.x + "," + i.totalTranslation.y + "," + i.totalTranslation.z;
				i.totalTranslation = Vector3.zero;
				line += "," + i.totalRotation.x + "," + i.totalRotation.y + "," + i.totalRotation.z + "," + i.totalRotation.w;
				i.totalRotation = Quaternion.identity;
				line += "," + i.totalScaling;
				i.totalScaling = 1;
				line += "," + i.totalRotationCamera.x + "," + i.totalRotationCamera.y + "," + i.totalRotationCamera.z + "," + i.totalRotationCamera.w;
				i.totalRotationCamera = Quaternion.identity;
				connected = true;
				break;

			}

			if (!connected) {

				line += ","+j+",0,0,0,0,0,0,0,1,1,0,0,0,1";

			}

		}
		f.WriteLine(line);
	}
}

