using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonStart : MonoBehaviour {

	public void OnClick()
	{
		SceneManager.LoadScene("BlocksBuilderTask");
	}
}