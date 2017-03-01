using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone_Menu : MonoBehaviour {

    public Transform target;
    public Camera gc;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 wantedPos = gc.WorldToScreenPoint(target.position);
        transform.position = wantedPos;
        Vector2 scale = transform.localScale;
        scale.x = 1;
        transform.localScale = scale;
    }
}
