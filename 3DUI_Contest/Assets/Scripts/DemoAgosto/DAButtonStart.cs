using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DAButtonStart : MonoBehaviour {

	public void OnClick()
	{
		SceneManager.LoadScene("BlocksBuilderTask");
	}
}