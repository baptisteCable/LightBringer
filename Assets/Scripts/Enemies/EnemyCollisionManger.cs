using UnityEngine;

namespace LightBringer.Enemies
{
    [RequireComponent (typeof (Collider))]
    public class EnemyCollisionManger : MonoBehaviour
    {
        private void OnTriggerStay (Collider other)
        {
            CharacterController body = other.GetComponent<CharacterController> ();

            if (other.tag == "Player" && body != null)
            {
                Vector3 playerDirection = other.transform.position - transform.position;
                Vector3 pushDirection = transform.right;

                if (Vector3.Dot (playerDirection, pushDirection) < 0)
                {
                    pushDirection *= -1;
                }

                body.SimpleMove (pushDirection * 40);
            }
        }
    }
}