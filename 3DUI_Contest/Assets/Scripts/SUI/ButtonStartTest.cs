﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonStartTest : MonoBehaviour {

	public void OnClick(){
		GameObject inputField = GameObject.Find ("InputField");
		InputField inpF = inputField.GetComponent<InputField> (); 
		//RecordGamePlay.SP.setFileNameToRecord (inpF.text);
		MainController.control.logFilename = inpF.text;
		SceneManager.LoadScene("Task1");

	}
}