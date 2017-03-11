using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeathFlag : NetworkBehaviour {
    public float lifetime = 5f;
	// Use this for initialization
	void Start () {
        Destroy(gameObject, lifetime);
	}
}
