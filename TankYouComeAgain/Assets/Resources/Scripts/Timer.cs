using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
    public const string TIME_STR = "Time Remaining: ";
    public float duration;
    public Font font;
    public int fontSize;
    public Vector2 minAnchor;
    public Vector2 maxAnchor;
    Text timerText;
    float timeLeft;
    GameObject canvas;
	// Use this for initialization
	void Start () {
        timeLeft = duration;
        canvas = transform.Find("Canvas").gameObject;
        if (!canvas) {
            canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.parent = transform.parent;
        }
        
        GameObject timerTextGO = new GameObject("Timer Text");
        timerText = timerTextGO.AddComponent<Text>();
        timerText.font = font;
        timerText.fontSize = fontSize;
        RectTransform rect = timerTextGO.GetComponent<RectTransform>();
        timerTextGO.transform.SetParent(canvas.transform, false);
        rect.anchorMin = minAnchor;
        rect.anchorMax = maxAnchor;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
	
	// Update is called once per frame
	void Update () {
        if (!Expired()) {
            timeLeft -= Time.deltaTime;
            timerText.text = TIME_STR + TimeToString(timeLeft);
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
