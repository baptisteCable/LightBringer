using LightBringer.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerController : NetworkBehaviour
    {
        // Input const
        public const int IN_NONE = -1;
        public const int IN_AB_ESC = 0;
        public const int IN_AB_1 = 1;
        public const int IN_AB_2 = 2;
        public const int IN_AB_DEF = 3;
        public const int IN_AB_OFF = 4;
        public const int IN_AB_ULT = 5;
        public const int IN_CANCEL = 6;

        // Input buttons (TODO: in preferences)
        private string[] inputButtons;

        // Queue and pressed button
        private int oldPressedButton = IN_CANCEL;
        [HideInInspector] public int queue = IN_CANCEL;
        [HideInInspector] public int pressedButton = IN_CANCEL;

        // Set by the client, send to the server when changed
        [HideInInspector] public Vector2 desiredMove;
        [HideInInspector] public Vector3 pointedWorldPoint;

        private Vector2 localMove;

        public Camera cam;

        [SerializeField]
        private NetworkSynchronization ns;

        private float lastSyncTime = 0;

        // Components
        private PlayerMotor pm;

        private void Start()
        {
            pm = GetComponent<PlayerMotor>();

            desiredMove = Vector2.zero;

            // TODO: Put in in preferences
            inputButtons = new string[7];
            inputButtons[IN_AB_ESC] = "AbEsc";
            inputButtons[IN_AB_1] = "Ab1";
            inputButtons[IN_AB_2] = "Ab2";
            inputButtons[IN_AB_DEF] = "AbDef";
            inputButtons[IN_AB_OFF] = "AbOff";
            inputButtons[IN_AB_ULT] = "AbUlt";
            inputButtons[IN_CANCEL] = "Cancel";
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            ComputePointedWorldPoint();
            SendPointedWorldPointToServer();
            DesiredMove();
            AbilityInputAndQueue();
        }

        private void AbilityInputAndQueue()
        {
            pressedButton = IN_NONE;

            for (int i = 0; i < inputButtons.Length; i++)
            {
                if (Input.GetButtonDown(inputButtons[i]))
                {
                    queue = i;
                }

                if (Input.GetButton(inputButtons[i]))
                {
                    pressedButton = i;
                }
            }

            // If new data, send to server
            if (queue != IN_NONE)
            {
                if (isServer)
                {
                    if (queue < pm.abilities.Length && !pm.abilities[queue].coolDownUp)
                    {
                        queue = IN_NONE;
                    }
                }
                else
                {
                    CmdSendQueue(queue);
                    queue = IN_NONE;
                }
            }

            if (oldPressedButton != pressedButton)
            {
                CmdSendPressedButton(pressedButton);
                oldPressedButton = pressedButton;
            }
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

        private void ComputePointedWorldPoint()
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
        void CmdSendQueue(int queueValue)
        {
            if (!isLocalPlayer)
            {
                queue = queueValue;
            }
        }

        [Command]
        void CmdSendPressedButton(int pressedButtonValue)
        {
            if (!isLocalPlayer)
            {
                pressedButton = pressedButtonValue;
            }
        }

        [Command]
        private void CmdSetDesiredMove(Vector2 move)
        {
            desiredMove = move;
        }

        void SendPointedWorldPointToServer()
        {
            if (isLocalPlayer && !isServer && Time.time > lastSyncTime + ns.syncInterval - .0001f)
            {
                CmdSendPointedWorldPointToServer(pointedWorldPoint);
                lastSyncTime = Time.time;
            }
        }

        [Command]
        void CmdSendPointedWorldPointToServer(Vector3 point)
        {
            if (!isLocalPlayer)
            {
                pointedWorldPoint = point;
            }
        }
    }
}

