using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimStart : MonoBehaviour
{
    void Start()
    {
        GetComponent<Animator>().Play("MainState", -1, Random.Range(0.0f, 1.0f));
    }
}
