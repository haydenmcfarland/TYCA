using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    public const int MAX_PLAYERS = 4;
    public static Game instance;
    public KeyCode exit = KeyCode.Escape;
    public Player[] players = new Player[MAX_PLAYERS];
    int currIndex = 0;

	// Use this for initialization
	void Start () {
        instance = this;
    }

    void Update() {
        if (Input.GetKeyDown(exit)) {
            SceneManager.LoadScene("Menu");
        }
    }

    public int RegisterPlayer(Player p) {
        // adds the player to the list and returns their id
        int id = currIndex;
        players[currIndex++] = p;
        return id;
    }

    public bool GameOver() {
        // Win condition goes here
        return false;
    }
}
