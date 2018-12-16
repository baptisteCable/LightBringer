using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float lookingHeight = 0f;
    public Plane lookingPlane;
    public static float projectorHeight = 8f;
    public Vector3 camPositionFromPlayer;
    public bool staticCamera;
    public Vector3 lookedPoint;

    public static GameManager gm;

    // Use this for initialization
    void Start () {
        if (gm != null)
        {
            throw new System.Exception("Multiple game managers");
        }
        gm = this;
		lookingPlane = new Plane(new Vector3(0, 1, 0), new Vector3(0, lookingHeight, 0));
        camPositionFromPlayer = new Vector3(-4.8f, 18f, -4.8f);
    }
}
