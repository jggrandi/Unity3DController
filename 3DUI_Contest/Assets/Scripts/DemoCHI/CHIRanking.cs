using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CHITeamInfo{
	public String teamName = "";
	public float teamScore = 0.0f;

}

public class CHIRanking{
	public List<CHITeamInfo> teamsInRanking;

	public CHIRanking(){
		teamsInRanking = new List<CHITeamInfo> ();
	}

	public void addToRank(CHITeamInfo team){
		teamsInRanking.Add (team);

	}

	public void sort(){
		teamsInRanking = teamsInRanking.OrderByDescending (o => o.teamScore).ToList ();
	}
		
}
