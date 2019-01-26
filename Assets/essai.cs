using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class essai : MonoBehaviour {

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other.name + " - " + other.tag);
    }
}
