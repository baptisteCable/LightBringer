using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusBar : MonoBehaviour {

    public GameObject enemy;
    public float displayHeight;
    
    void Start () {
		
	}
	
	void Update () {
        transform.position = Camera.main.WorldToScreenPoint(enemy.transform.position + new Vector3(0, displayHeight - .6f, 0)) + new Vector3(0,60,0);
	}
}
