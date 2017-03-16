using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
    public const string TIME_STR = "Time Remaining: ";
    public float duration;
    Text timerText;
    float timeLeft;
	// Use this for initialization
	void Start () {
        timeLeft = duration;
        timerText = transform.Find("Canvas/Timer Text").gameObject.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        timeLeft -= Time.deltaTime;
        timerText.text = TIME_STR + TimeToString(timeLeft);
    }

    string TimeToString(float time) {
        // turns float in seconds to string in format 'mm:ss.s'
        int minutes = (int)Mathf.Floor(time / 60f);
        minutes = Mathf.Clamp(minutes, 0, minutes);
        float seconds = time - minutes * 60f;
        string secondsStr = (seconds < 10 ? "0" : "") + seconds.ToString("F1");
        return minutes + ":" + secondsStr;
    }

    public float GetTimeLeft() {
        return timeLeft;
    }

    public bool Expired() {
        return timeLeft <= 0;
    }

    public void Reset() {
        timeLeft = duration;
    }
}
