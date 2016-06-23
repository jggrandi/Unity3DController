using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SVRShowRanking : MonoBehaviour {

	public GameObject Snowman;

	public Ranking rank;
	int teamPosition = 0;
	// Use this for initialization
	void Start () {
//		GameObject go = MainController.control.finalConstruction;
//		go.transform.position = new Vector3 (-73.7f, -59.82f, -225.5f);
//		go.transform.rotation = Quaternion.Euler (0, 358.0f, 0);
		rank = new Ranking();
		TeamInfo team = new TeamInfo();
		int rankingQnt = PlayerPrefs.GetInt ("rankingQnt");

		for (int i = 0; i < rankingQnt; i++) {
			team.teamName = PlayerPrefs.GetString ("teamName" + i);
			team.teamScore = PlayerPrefs.GetFloat ("teamScore" + i);
			rank.addToRank (team);
			team = new TeamInfo();
		}
		team = new TeamInfo();
		team.teamName = MainController.control.teamName;
		team.teamScore = MainController.control.finalScore;
		rank.addToRank (team);
		rank.sort ();
		team = new TeamInfo();

		teamPosition = rank.teamsInRanking.FindIndex (a => a.teamName == MainController.control.teamName);
		//print (teamPosition);

		PlayerPrefs.SetInt("rankingQnt", rank.teamsInRanking.Count);
		for (int i = 0; i < rank.teamsInRanking.Count; i++) {
			PlayerPrefs.SetString ("teamName" + i, rank.teamsInRanking [i].teamName);
			PlayerPrefs.SetFloat ("teamScore" + i, rank.teamsInRanking [i].teamScore);
		}
		//print (rank.teamsInRanking.Count);


	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey ("space")) {
			SceneManager.LoadScene ("StartSetup");
		}	
	}

	void OnGUI(){
		
		GUIStyle titleStyle = new GUIStyle();
		titleStyle.fontSize = 50;
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.alignment = TextAnchor.MiddleCenter;
		int correctedPos = teamPosition + 1;
		GUI.Label (new Rect (Screen.width/2,40, 50, 50),"Pos: " + correctedPos + "\t " + MainController.control.teamName + "\t" + MainController.control.finalScore  , titleStyle);

		string ranking = null;
		for(int i = 0; i < rank.teamsInRanking.Count; i++){
			ranking += i.ToString ();
			ranking += "-\t";
			ranking += rank.teamsInRanking[i].teamName;
			ranking += "\t";
			ranking += rank.teamsInRanking[i].teamScore.ToString();
			ranking += "\n";
		}

		GUI.Box (new Rect (Screen.width / 2.0f, Screen.height / 4.0f, 200, 200), "Ranking:\n" + ranking );

		GUI.Label (new Rect (Screen.width/2.0f,Screen.height-100, 50, 50), "Press Space to Start"  , titleStyle);
	}

}
