﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonStartTask : MonoBehaviour {

	public void OnClick(){
		GameObject teamName = GameObject.Find ("TeamName");
		InputField inpTeamName = teamName.GetComponent<InputField> ();
		GameObject taskNumber = GameObject.Find ("TaskNumber");
		InputField inpTaskNumber = taskNumber.GetComponent<InputField> ();
		MainController.control.teamName = inpTeamName.text;
		int act = int.Parse( inpTaskNumber.text);
		MainController.control.activeScene = act;
		 
		MainController.control.activeScene += 1;
		int indexScene = MainController.control.activeScene;
		SceneManager.LoadScene(indexScene);

	}
}