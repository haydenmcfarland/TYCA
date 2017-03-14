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
        Player winner = DetermineWinner();
        if (winner) {
            gameOverText.text = winner.playerName + " wins!";
        }
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Menu");
    }

    public void UpdateStats(Player p) {
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
        int minutes = (int)Mathf.Floor(time / 60f);
        minutes = Mathf.Clamp(minutes, 0, minutes);
        float seconds = time - minutes * 60f;
        string secondsStr = (seconds < 10 ? "0" : "") + seconds.ToString("F1");
        return minutes + ":" + secondsStr;
    }

    Player DetermineWinner() {
        Player winner = null;
        int maxScore = 0;
        foreach(Player p in players) {
            if(p && p.kills - p.deaths > maxScore) {
                winner = p;
                maxScore = p.kills - p.deaths;
            }
        }
        return winner;
    }
}
