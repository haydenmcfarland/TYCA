using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoad : MonoBehaviour {
    public void LoadLevel(string action) {
        Navigator.Instance.LoadLevel(action);
    }
	
}
