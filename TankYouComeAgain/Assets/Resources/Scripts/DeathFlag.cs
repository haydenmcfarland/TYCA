using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeathFlag : NetworkBehaviour {
    public float lifetime = 5f;
    AudioSource clip;
	// Use this for initialization
	void Start () {
        clip = GetComponent<AudioSource>();
        Game.instance.PlayClip(clip);
        Destroy(gameObject, lifetime);
	}
}
