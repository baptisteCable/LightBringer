using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float lookingHeight = 0f;
    public Plane lookingPlane;

	// Use this for initialization
	void Start () {
		lookingPlane = new Plane(new Vector3(0, 1, 0), new Vector3(0, lookingHeight, 0));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
