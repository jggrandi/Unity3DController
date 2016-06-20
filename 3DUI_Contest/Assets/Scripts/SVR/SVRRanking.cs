using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TeamInfo{
	public String teamName = "";
	public float teamScore = 0.0f;

}

public class Ranking{
	public List<TeamInfo> teamsInRanking;

	public Ranking(){
		teamsInRanking = new List<TeamInfo> ();
	}

	public void addToRank(TeamInfo team){
		teamsInRanking.Add (team);

	}

	public void sort(){
		teamsInRanking = teamsInRanking.OrderByDescending (o => o.teamScore).ToList ();
	}
		
}
