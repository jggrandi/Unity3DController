using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CHIShowRanking : MonoBehaviour {

	public CHIRanking rank;
	int teamPosition = 0;
	// Use this for initialization
	void Start () {

        MainController.control.acceptingConnections = true;
        //		GameObject go = MainController.control.finalConstruction;
        //		go.transform.position = new Vector3 (-73.7f, -59.82f, -225.5f);
        //		go.transform.rotation = Quaternion.Euler (0, 358.0f, 0);
        rank = new CHIRanking();
		CHITeamInfo team = new CHITeamInfo();
		int rankingQnt = PlayerPrefs.GetInt ("rankingQnt");

		for (int i = 0; i < rankingQnt; i++) {
			team.teamName = PlayerPrefs.GetString ("teamName" + i);
			team.teamScore = PlayerPrefs.GetFloat ("teamScore" + i);
			rank.addToRank (team);
			team = new CHITeamInfo();
		}
		team = new CHITeamInfo();
		team.teamName = MainController.control.teamName;
		team.teamScore = MainController.control.finalScore;
		rank.addToRank (team);
		rank.sort ();
		team = new CHITeamInfo();

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

        Rect recRank = new Rect(Screen.width / 2.0f + 240.0f, Screen.height / 6.0f -10.0f, 300, 430);
        
        GUIStyle currentStyle = new GUIStyle(GUI.skin.box);
        currentStyle.normal.background = Utils.MakeTexture(200, 200, new Color32(200, 200, 200, 100));

        GUI.Box(recRank, "", currentStyle);

        string ranking = null;
        for (int i = 0; i < 17; i++)
        {
            int index = i + 1;
            ranking += index.ToString();
            ranking += "\t";
            ranking += PlayerPrefs.GetString("teamName" + i);
            ranking += "\t";
            ranking += PlayerPrefs.GetFloat("teamScore" + i).ToString();
            ranking += "\n";
        }

        GUIStyle rankStyle = new GUIStyle();
        rankStyle.fontSize = 20;
        rankStyle.fontStyle = FontStyle.Normal;
        //rankStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Label (new Rect (Screen.width / 2.0f +250.0f, Screen.height /6.0f , 200,100), "Ranking:\n" + ranking, rankStyle);

		GUI.Label (new Rect (Screen.width/2.0f,Screen.height-100, 50, 50), "Press Space to Start"  , titleStyle);
	}

}
