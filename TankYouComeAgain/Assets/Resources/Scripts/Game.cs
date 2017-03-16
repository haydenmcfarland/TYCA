using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    public const int MAX_PLAYERS = 4;
    public const string VOTE_STR = "Votes to Restart: ";
    public static Game instance;
    public GameObject playerStats;
    public KeyCode exitKey = KeyCode.Escape;
    public KeyCode muteKey = KeyCode.M;
    public KeyCode scoreboardKey = KeyCode.Tab;
    public bool muted = false;
    public Player[] players = new Player[MAX_PLAYERS];
    GameObject[] playerStatsGOs = new GameObject[MAX_PLAYERS];
    int numPlayers = 0;
    public Timer timer;
    bool ended = false;
    Enemy enemy;
    Text gameOverText;
    GameObject scoreboard;
    GameObject playerStatsHolder;

    // Use this for initialization
    void Start() {
        instance = this;
        gameOverText = transform.Find("Canvas/Game Over Text").gameObject.GetComponent<Text>();
        scoreboard = transform.Find("Canvas/Scoreboard").gameObject;
        playerStatsHolder = transform.Find("Canvas/Scoreboard/Player Stats").gameObject;
        timer = GetComponent<Timer>();
        for (int i = 0; i < MAX_PLAYERS; ++i) {
            playerStatsGOs[i] = Instantiate(playerStats);
            playerStatsGOs[i].transform.SetParent(playerStatsHolder.transform, false);
        }
    }

    void Update() {
        if (RestartGame()) {
            foreach(Player p in players) {
                if (p) {
                    p.Restart();
                }
            }
        }
        if (GameOver()) {
            if (!ended) {
                StartCoroutine(EndOfGame());
                ended = true;
            }
            return;
        }
        if (Input.GetKeyDown(exitKey)) {
            Destroy(GameObject.Find("LobbyManager"));
            SceneManager.LoadScene("Menu");
        }
        if (Input.GetKeyDown(muteKey)) {
            muted = !muted;
        }
        scoreboard.SetActive(Input.GetKey(scoreboardKey));

    }
    public int AssignId() {
        // used by the Game class that is on the server, so that each Player will get a unique id
        return numPlayers++;
    }
    public void RegisterPlayer(Player p) {
        // adds the player to the list
        players[p.id] = p;
    }

    public bool GameOver() {
        return timer.Expired();
    }

    public bool RestartGame() {
        return GetNumVotes() == GetNumPlayers();    
    }

    public int GetNumVotes() {
        int total = 0;
        for (int i = 0; i < players.Length; ++i) {
            if (players[i] && players[i].voteToRestart) {
                total++;
            }
        }
        return total;
    }

    IEnumerator EndOfGame() {
        // Coroutine called when the game ends
        Player[] winner = DetermineWinner();
        int winners = 0;
        for(int i = 0; i < winner.Length; ++i) {
            if (winner[i]) {
                winners++;
            }
        }
        if (winners == 1) {
            gameOverText.text = winner[0].playerName + " wins!";
        } else {
            string players = "";
            for(int i = 0; i < winner.Length; ++i) {
                if (winner[i]) {
                    players += winner[i].playerName + " and ";
                }
            }
            players = players.Substring(0, players.Length - 4);
            gameOverText.text = players + " win!";
        }
        yield return new WaitForSeconds(5);
        Destroy(GameObject.Find("LobbyManager"));
        SceneManager.LoadScene("Menu");
    }

    public void UpdateStats(Player p) {
        // this is super inefficient but it works lol
        playerStatsGOs[p.id].transform.Find("Name").GetComponent<Text>().text = p.playerName;
        playerStatsGOs[p.id].transform.Find("Kills").GetComponent<Text>().text = "" + p.kills;
        playerStatsGOs[p.id].transform.Find("Deaths").GetComponent<Text>().text = "" + p.deaths;
        playerStatsGOs[p.id].transform.Find("Score").GetComponent<Text>().text = "" + (p.kills - p.deaths);
        playerStatsGOs[p.id].GetComponent<Image>().color = p.playerColor;
    }

    public void PlayClip(AudioSource clip) {
        if (!muted) {
            clip.Play();
        }
    }

    public int GetNumPlayers() {
        // for some reason when I call length of the player list it is giving me 4 always (the capacity of the array), so this dumb code works
        int count = 0;
        for(int i = 0; i < players.Length; ++i) {
            if (players[i]) {
                count++;
            }
        }
        return count;
    }

    Player[] DetermineWinner() {
        // determines the winner based on who has the highest score and returns that Player
        Player[] winner = new Player[MAX_PLAYERS];
        float maxScore = -9999;
        int currIndex = 0;
        foreach(Player p in players) {
            if(p && p.kills - p.deaths > maxScore) {
                currIndex = 0;
                winner[currIndex++] = p;
                maxScore = p.kills - p.deaths;
            } else if(p && p.kills - p.deaths == maxScore) {
                winner[currIndex++] = p;
            }
        }
        return winner;
    }
}
