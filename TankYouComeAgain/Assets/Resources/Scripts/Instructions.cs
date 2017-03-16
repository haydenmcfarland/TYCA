using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Instructions : MonoBehaviour {

    public Canvas instructionsCanvas;

    // Use this for initialization
    void Start() {
        instructionsCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update() {

    }

    public void Disable()
    {
        instructionsCanvas.enabled = false;
    }

    public void Enable()
    {
        instructionsCanvas.enabled = true;
    }
}
