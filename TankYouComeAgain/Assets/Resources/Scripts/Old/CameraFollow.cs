using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform playerTransform;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

		if(playerTransform != null)
            transform.position = playerTransform.position + new Vector3(0,0,-20);
	}

    public void setTargetTransform(Transform target)
    {
        playerTransform = target;
    }
}
