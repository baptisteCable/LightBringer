using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player
{
    public class PlayerController : NetworkBehaviour
    {
        // Set by the client, send to the server when changed
        [HideInInspector]
        public Vector2 desiredMove;

        private void Start()
        {
            desiredMove = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            Move();
        }

        // Get desired move from local player input
        private void Move()
        {
            // Move input
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");
            Vector2 move = new Vector2(v + h, v - h);

            if (move.magnitude < .01f)
            {
                move = Vector2.zero;
            }
            else
            {
                move.Normalize();
            }

            if (move != desiredMove)
            {
                desiredMove = move;
                if (!isServer)
                {
                    CmdSetDesiredMove(move);
                }
            }
        }

        [Command]
        private void CmdSetDesiredMove(Vector2 move)
        {
            desiredMove = move;
        }
    }
}

