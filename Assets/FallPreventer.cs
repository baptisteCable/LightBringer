﻿using UnityEngine;

namespace LightBringer
{
    [RequireComponent (typeof (Collider))]
    public class FallPreventer : MonoBehaviour
    {
        private void OnTriggerEnter (Collider other)
        {
            if (other.tag == "Player" || other.tag == "Enemy")
            {
                // message
                Debug.Log("Fall preventer triggered.");

                // Cast a ray from sky
                Physics.Raycast(
                    other.transform.position + 50 * Vector3.up,
                    Vector3.down,
                    out RaycastHit hit,
                    70,
                    LayerMask.GetMask("Environment"));

                // Drop the entity on the ground
                other.transform.position = hit.point;
            }
        }
    }
}
