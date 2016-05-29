using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RecordStatus { stopped = 1, paused, recording }
public enum RecordActions { playerAction, objectTransformation, collisionEvent, checkPoint}
public enum TransformationAction { translation, rotation, scale , cameraRotation}

public class RecordGamePlay : MonoBehaviour {
	public static RecordGamePlay SP;
	private RecordStatus recordStatus;
	private float startedRecording =0;
	private List<RecordedEvent> replayData = new List<RecordedEvent>();
	private float pausedAt = 0;

	void Awake(){
		SP =this;
		StartRecording();
	}
	public float RecordTime()
	{
		if (recordStatus != RecordStatus.recording)
			Debug.LogError("Cant get time!");
		return Time.realtimeSinceStartup - startedRecording;
	}

	public void PauseRecording()
	{
		if (recordStatus != RecordStatus.recording)
			Debug.LogError("Cant pause! " + recordStatus);
		pausedAt = RecordTime();
		recordStatus = RecordStatus.paused;
	}

	public void StartRecording(float startTime){
		//Delete all actions after STARTTIME. Continue Recording from this point
		if (replayData.Count >= 0)
		{
			for (int i = replayData.Count - 1; i >= 0; i--)
			{
				RecordedEvent action = replayData[i];
				if (action.mainTime >= startTime)
					replayData.Remove(action);
			}
		}
		pausedAt = startTime;
		StartRecording();
	}

	public void StartRecording()
	{
		if(recordStatus == RecordStatus.paused){
			startedRecording = Time.realtimeSinceStartup - pausedAt;
		}else{
			startedRecording = Time.realtimeSinceStartup;
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
		AddAction (action, Vector3.zero, position, Quaternion.identity, rotation, Vector3.zero, Vector3.zero, 0);
	}

	public void AddAction(RecordActions action, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		AddAction (action, Vector3.zero, position, Quaternion.identity, rotation, Vector3.zero, scale, 0);
	}


	public void AddAction(RecordActions action, Vector3 initPos, Vector3 finPos, Quaternion initRot, Quaternion finRot, Vector3 initScale, Vector3 finScale, float pose){
		if (!IsRecording ())
			return;
		RecordedEvent newAction = new RecordedEvent ();
		newAction.recordedAction = action;
		newAction.positionInitial = initPos;
		newAction.positionInitial = finPos;
		newAction.rotationInitial = initRot;
		newAction.rotationFinal = finRot;
		newAction.scaleInitial = initScale;
		newAction.scaleFinal = finScale;
		newAction.poseError = pose;
		newAction.mainTime = RecordTime ();
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

	public string RecordedDataToReadableString(){
		string output ="Replay data:\n";
		foreach(RecordedEvent action in replayData){
			output+= action.mainTime +": "+action.recordedAction+"\n";
		}
		return output;
	}

	public string RecordedDataToString()
	{
		string output = "";
		foreach (RecordedEvent action in replayData)
		{
			output += action.mainTime 
			+ "#" + action.startActionTime.ToString("G4") + "#" + action.finishActionTime.ToString("G4")
			+ "#" + (int)action.recordedAction
			+ "#" + action.positionInitial.ToString ("G4") + "#" + action.positionFinal.ToString ("G4")
			+ "#" + action.rotationInitial.ToString ("G4") + "#" + action.rotationFinal.ToString ("G4")
			+ "#" + action.scaleInitial.ToString ("G4") + "#" + action.scaleFinal.ToString ("G4")
			+ "#" + action.poseError.ToString ("G4");
		}
		return output;
	}

	public List<RecordedEvent> GetEventsList(){
		return replayData;
	}

	/* //Add the right URL to your upload script
	public IEnumerator UploadData()
	{
		WWWForm wwwForm = new WWWForm();
		wwwForm.AddField("replayData", RecordedDataToString());
		WWW www = new WWW("http://www.YOURSITE.com/uploadData.php", wwwForm);
		yield return www;
		Debug.Log("Uploaded replay data!");
	}
	*/
}

public class RecordedEvent {
	public RecordActions recordedAction;
	//public TransformationAction recordedTransformationAction;
	public float mainTime;
	public float startActionTime;
	public float finishActionTime;
	public Vector3 positionInitial;
	public Vector3 positionFinal;
	public Quaternion rotationInitial;
	public Quaternion rotationFinal;
	public Vector3 scaleInitial;
	public Vector3 scaleFinal;
	public float poseError;
}