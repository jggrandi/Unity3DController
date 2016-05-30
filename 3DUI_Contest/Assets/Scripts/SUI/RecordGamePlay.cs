using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum RecordStatus { stopped = 1, paused, recording }
public enum RecordActions { playerAction, objectTransformation, collisionEvent, checkPoint}
public enum TransformationAction { translation, rotation, scale , cameraRotation, collision}

public class RecordGamePlay : MonoBehaviour {
	public static RecordGamePlay SP;
	private RecordStatus recordStatus;
	private float startedRecording = 0;
	private List<RecordedEvent> replayData = new List<RecordedEvent>();
	private float pausedAt = 0;

	private StreamWriter SW;

	void Awake(){
		SP = this;
		//StartRecording();

	}
	public float RecordTime()
	{
		if (recordStatus != RecordStatus.recording)
			Debug.LogError("Cant get time!");
		return MainController.control.gameRuntime - startedRecording;
	}

	public void PauseRecording()
	{
		if (recordStatus != RecordStatus.recording)
			Debug.LogError("Cant pause! " + recordStatus);
		pausedAt = RecordTime();
		recordStatus = RecordStatus.paused;
	}

//	public void StartRecording(float startTime){
//		//Delete all actions after STARTTIME. Continue Recording from this point
//		if (replayData.Count >= 0)
//		{
//			for (int i = replayData.Count - 1; i >= 0; i--)
//			{
//				RecordedEvent action = replayData[i];
//				if (action.mainTime >= startTime)
//					replayData.Remove(action);
//			}
//		}
//		pausedAt = startTime;
//		StartRecording();
//	}

//	public void StartRecording()
//	{
//		if(recordStatus == RecordStatus.paused){
//			startedRecording = MainController.control.gameRuntime - pausedAt;
//		}else{
//			startedRecording = MainController.control.gameRuntime;
//			replayData = new List<RecordedEvent>();
//		}
//		recordStatus = RecordStatus.recording;
//	}

	public void StartRecording(float gameRunTime)
	{
		if(recordStatus == RecordStatus.paused){
			startedRecording = gameRunTime - pausedAt;
		}else{
			startedRecording = gameRunTime;
			replayData = new List<RecordedEvent>();
		}
		recordStatus = RecordStatus.recording;
	}

	public bool IsRecording(){
		return recordStatus == RecordStatus.recording;
	}

	public bool IsPaused(){
		return recordStatus == RecordStatus.paused;
	}

	//Hereby multiple defenitions so that you can add as much data as you want.
	public void AddAction(RecordActions action)
	{
		AddAction(action, Vector3.zero);
	}

	public void AddAction(RecordActions action, Vector3 position)
	{
		AddAction(action, position, Quaternion.identity);
	}

	public void AddAction(RecordActions action, Vector3 position, Quaternion rotation)
	{
		AddAction (action,99,0,0,0, Vector3.zero, position, Quaternion.identity, rotation, Vector3.zero, Vector3.zero, 0,0);
	}

	public void AddAction(RecordActions action, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		AddAction (action,99,0,0,0, Vector3.zero, position, Quaternion.identity, rotation, Vector3.zero, scale, 0,0);
	}

	public void AddAction (RecordActions action, int idd, TransformationAction taction, float initActTime, float finActTime, Quaternion initRot, Quaternion finRot, float initPose, float finPose){
		AddAction (action,idd,taction, initActTime,finActTime, Vector3.zero, Vector3.zero, initRot, finRot, Vector3.zero, Vector3.zero, initPose,finPose);
	}

	public void AddAction (RecordActions action, TransformationAction taction, float initActTime, float finActTime){
		AddAction (action, 99, taction, initActTime, finActTime, Vector3.zero, Vector3.zero, Quaternion.identity, Quaternion.identity, Vector3.zero, Vector3.zero, 0,0);
	}

	public void AddAction (RecordActions action, int idd, TransformationAction taction, float initActTime, float finActTime, Vector3 initTrans, Vector3 finTrans, float initPose, float finPose){
		if (taction == TransformationAction.translation)
			AddAction (action, idd, taction, initActTime, finActTime, initTrans, finTrans, Quaternion.identity, Quaternion.identity, Vector3.zero, Vector3.zero, initPose,finPose);
		else if(taction == TransformationAction.scale)
			AddAction (action,idd,taction,initActTime,finActTime, Vector3.zero, Vector3.zero, Quaternion.identity, Quaternion.identity, initTrans, finTrans, initPose,finPose);
	}


	public void AddAction(RecordActions action,int idd, TransformationAction taction, float initActTime, float finActTime, Vector3 initPos, Vector3 finPos, Quaternion initRot, Quaternion finRot, Vector3 initScale, Vector3 finScale, float initPose, float finPose){
		if (!IsRecording ())
			return;
		RecordedEvent newAction = new RecordedEvent ();

		newAction.mainTime = RecordTime ();
		newAction.startActionTime = initActTime;
		newAction.finishActionTime = finActTime;
		newAction.recordedAction = action;
		newAction.recordedTransformationAction = taction;
		newAction.id = idd;
		newAction.positionInitial = initPos;
		newAction.positionFinal = finPos;
		newAction.rotationInitial = initRot;
		newAction.rotationFinal = finRot;
		newAction.scaleInitial = initScale;
		newAction.scaleFinal = finScale;
		newAction.poseErrorInitial = initPose;
		newAction.poseErrorFinal = finPose;
		replayData.Add (newAction);
	}

	public void StopRecording()
	{
		if(recordStatus != RecordStatus.recording)
			Debug.LogError("Cant STOP!");
		stoppedAtLength = RecordTime();
		recordStatus = RecordStatus.stopped;
	}

	private float stoppedAtLength = 0;

	public float LastRecordLength()
	{
		if (recordStatus == RecordStatus.paused) return pausedAt;
		if (recordStatus != RecordStatus.stopped) return RecordTime();
		return stoppedAtLength;
	}

	public String RecordedDataToReadableString(){
		String output ="Replay data:\n";
		foreach(RecordedEvent action in replayData){
			output+= action.mainTime +": "+action.recordedAction+"\n";
		}
		return output;
	}

	public String RecordedDataToString()
	{
		String output = "";
		foreach (RecordedEvent action in replayData)
		{
			output += action.mainTime 
				+ "#" + action.startActionTime.ToString("G4") + "#" + action.finishActionTime.ToString("G4")
				+ "#" + (int)action.recordedAction + "#" + action.id + "#" + action.recordedTransformationAction
				+ "#" + action.positionInitial.ToString ("G4") + "#" + action.positionFinal.ToString ("G4")
				+ "#" + action.rotationInitial.ToString ("G4") + "#" + action.rotationFinal.ToString ("G4")
				+ "#" + action.scaleInitial.ToString ("G4") + "#" + action.scaleFinal.ToString ("G4")
				+ "#" + action.poseErrorInitial.ToString ("G4") + "#" + action.poseErrorFinal.ToString ("G4")
				+ "\n";
		}
		return output;
	}

	public List<RecordedEvent> GetEventsList(){
		return replayData;
	}

	public void RecordDataToFile(){
		if (SW == null) {
			SW = new StreamWriter ("/home/jeronimo/Documents/Logs/test.dat",false);

		}
		//else
		//SW.			
		SW.Write (RecordedDataToString ());
		SW.Flush ();
		//SW.Close ();
	}


}

public class RecordedEvent {
	public RecordActions recordedAction;
	public TransformationAction recordedTransformationAction;
	public int id;
	public float mainTime;
	public float startActionTime;
	public float finishActionTime;
	public Vector3 positionInitial;
	public Vector3 positionFinal;
	public Quaternion rotationInitial;
	public Quaternion rotationFinal;
	public Vector3 scaleInitial;
	public Vector3 scaleFinal;
	public float poseErrorInitial;
	public float poseErrorFinal;
}