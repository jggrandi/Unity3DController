using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonRestart : MonoBehaviour {

	public void OnClick()
	{
		SceneManager.LoadScene("Start");
	}
}