using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SVRButtonStartTask : MonoBehaviour {

	public void OnClick(){
		MainController.control.ALLOWINGCONNECTIONS = false;
		GameObject teamName = GameObject.Find ("TeamName");
		InputField inpTeamName = teamName.GetComponent<InputField> ();
		MainController.control.teamName = inpTeamName.text;
		SceneManager.LoadScene("SnowmanBuild");

	}
}