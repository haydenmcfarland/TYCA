using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    public const int MAX_PLAYERS = 4;
    public const string TIME_STR = "Time Remaining: ";
    public static Game instance;
    public float gameDuration = 120f;
    public GameObject playerStats;
    public KeyCode exitKey = KeyCode.Escape;
    public KeyCode muteKey = KeyCode.M;
    public KeyCode scoreboardKey = KeyCode.Tab;
    public bool muted = false;
    public Player[] players = new Player[MAX_PLAYERS];
    GameObject[] playerStatsGOs = new GameObject[MAX_PLAYERS];
    int currIndex = 0;
    float timer;
    bool ended = false;
    Text timerText;
    Text gameOverText;
    GameObject scoreboard;
    GameObject playerStatsHolder;

    // Use this for initialization
    void Start() {
        instance = this;
        timer = gameDuration;
        timerText = transform.Find("Canvas/Timer Text").gameObject.GetComponent<Text>();
        gameOverText = transform.Find("Canvas/Game Over Text").gameObject.GetComponent<Text>();
        scoreboard = transform.Find("Canvas/Scoreboard").gameObject;
        playerStatsHolder = transform.Find("Canvas/Scoreboard/Player Stats").gameObject;
        for (int i = 0; i < MAX_PLAYERS; ++i) {
            playerStatsGOs[i] = Instantiate(playerStats);
            playerStatsGOs[i].transform.SetParent(playerStatsHolder.transform, false);
        }
    }

    void Update() {
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
        timer -= Time.deltaTime;
        timerText.text = TIME_STR + TimeToString(timer);
    }
    public int AssignId() {
        // used by the Game class that is on the server, so that each Player will get a unique id
        return currIndex++;
    }
    public void RegisterPlayer(Player p) {
        // adds the player to the list
        players[p.id] = p;
    }

    public bool GameOver() {
        return timer <= 0;
    }

    IEnumerator EndOfGame() {
        // Coroutine called when the game ends
        Player[] winner = DetermineWinner();
        if (winner.Length == 1) {
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

    string TimeToString(float time) {
        // turns float in seconds to string in format 'mm:ss.s'
        int minutes = (int)Mathf.Floor(time / 60f);
        minutes = Mathf.Clamp(minutes, 0, minutes);
        float seconds = time - minutes * 60f;
        string secondsStr = (seconds < 10 ? "0" : "") + seconds.ToString("F1");
        return minutes + ":" + secondsStr;
    }

    Player[] DetermineWinner() {
        // determines the winner based on who has the highest score and returns that Player
        Player[] winner = new Player[MAX_PLAYERS];
        int maxScore = 0;
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
