using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour {

    public Text pOne;
    public Text pTwo;
    private int pOneKills = 0;
    private int pTwoKills = 0;
    private string padding = "     ";

	// Use this for initialization
	void Start () {
        pOne.text = padding + "P1: " + pOneKills.ToString();
        pTwo.text = padding + "P2: " + pOneKills.ToString();
    }
	
	// Update is called once per frame
	void Update () {
    }

    public void UpdatePlayerScore(string playerNum)
    {
        if (playerNum == "1")
        {
            pOneKills += 1;
            pOne.text = padding + "P1: " + pOneKills.ToString();
        }
        else
        {
            pTwoKills += 1;
            pTwo.text = padding + "P2: " + pTwoKills.ToString();
        }

    }
}
