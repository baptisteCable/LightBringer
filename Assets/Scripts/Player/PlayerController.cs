using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player
{
    public class PlayerController : NetworkBehaviour
    {
        // Set by the client, send to the server when changed
        [HideInInspector]
        public Vector2 desiredMove;
        public Vector3 pointedWorldPoint;

        private Vector2 localMove;

        public Camera cam;

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

            DesiredMove();

        }

        private void Update()
        {
            ComputeAndSendPointedWorldPoint();
        }

        // Get desired move from local player input
        private void DesiredMove()
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

        private void ComputeAndSendPointedWorldPoint()
        {
            if (cam != null)
            {
                Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

                float distance;

                Vector3 point = Vector3.zero;

                if (GameManager.gm.floorPlane.Raycast(mouseRay, out distance))
                {
                    pointedWorldPoint = mouseRay.GetPoint(distance);
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

