using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public const float FLOOR_HEIGHT = 5f;

    public const float GRAVITY = 10f;
    public bool staticCamera;
    
    public int currentFloor = 0;
    public Plane floorPlane;
    public float currentAlt = 0f;

    public static GameManager gm;

    // Training
    [HideInInspector] public bool ignoreCD = false;

    // Use this for initialization
    void Start () {
        if (gm != null)
        {
            throw new System.Exception("Multiple game managers");
        }
        gm = this;
    }
}
