using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Mutable : MonoBehaviour {
    AudioSource clip;
	// Use this for initialization
	void Start () {
        clip = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Game.instance) {
            clip.mute = Game.instance.muted;
        }
    }
}
