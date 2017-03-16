using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Broadcast : NetworkBehaviour {
    Text broadcastText;
	// Use this for initialization
	void Start () {
        broadcastText = GetComponent<Text>();
	}
	
	public void SetText(string text) {
        broadcastText.text = text;
    }
}
