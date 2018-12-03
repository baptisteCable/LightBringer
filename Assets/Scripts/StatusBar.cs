using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour {

    public GameObject character;
    public Camera cam;

    void Start () {
		
	}
	
	void Update () {
        transform.position = cam.WorldToScreenPoint(character.transform.position + new Vector3(0, 1f, 0)) + new Vector3(0,60,0);
	}
}
