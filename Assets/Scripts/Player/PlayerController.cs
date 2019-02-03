using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [HideInInspector]
        public Vector2 desiredMove;

        private void Start()
        {
            desiredMove = Vector2.zero;
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            // Move input
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");
            desiredMove = new Vector2(v + h, v - h);

            if (desiredMove.magnitude < .01f)
            {
                desiredMove = Vector2.zero;
            }
            else
            {
                desiredMove.Normalize();
            }
        }
    }
}

