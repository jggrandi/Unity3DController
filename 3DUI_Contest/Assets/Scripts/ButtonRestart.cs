﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonRestart : MonoBehaviour {

	public void OnClick()
	{
		//GameController.control.gameRuntime = 0;
		SceneManager.LoadScene("Start");
	}
}