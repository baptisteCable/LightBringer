using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent (typeof (PlayerMotor))]
    public class PlayerController : MonoBehaviour
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
        public const int IN_TEST = 7;

        private string[] inputButtons;

        // Queue and pressed button
        [HideInInspector] public int queue = IN_CANCEL;
        [HideInInspector] public int pressedButton = IN_CANCEL;

        // Set by the client, send to the server when changed
        [HideInInspector] public Vector2 desiredMove;
        [HideInInspector] public Vector3 pointedWorldPoint;

        public Camera cam;

        // Components
        private PlayerMotor pm;

        private void Start ()
        {
            pm = GetComponent<PlayerMotor> ();

            desiredMove = Vector2.zero;

            inputButtons = new string[8];
            inputButtons[IN_AB_ESC] = "AbEsc";
            inputButtons[IN_AB_1] = "Ab1";
            inputButtons[IN_AB_2] = "Ab2";
            inputButtons[IN_AB_DEF] = "AbDef";
            inputButtons[IN_AB_OFF] = "AbOff";
            inputButtons[IN_AB_ULT] = "AbUlt";
            inputButtons[IN_CANCEL] = "Cancel";
            inputButtons[IN_TEST] = "TestButton";
        }

        private void Update ()
        {
            ComputePointedWorldPoint ();
            DesiredMove ();
            AbilityInputAndQueue ();
        }

        private void AbilityInputAndQueue ()
        {
            pressedButton = IN_NONE;

            for (int i = 0; i < inputButtons.Length; i++)
            {
                if (Input.GetButtonDown (inputButtons[i]))
                {
                    queue = i;
                }

                if (Input.GetButton (inputButtons[i]))
                {
                    pressedButton = i;
                }
            }

            // Clear queue if CD not up
            if (queue != IN_NONE && queue < pm.abilities.Length && pm.abilities[queue].state != Abilities.AbilityState.cooldownUp)
            {
                queue = IN_NONE;
            }
        }

        // Get desired move from local player input
        private void DesiredMove ()
        {
            // Move input
            float v = Input.GetAxisRaw ("Vertical");
            float h = Input.GetAxisRaw ("Horizontal");
            Vector2 move = new Vector2 (v + h, v - h);

            if (move.magnitude < .01f)
            {
                move = Vector2.zero;
            }
            else
            {
                move.Normalize ();
            }

            if (move != desiredMove)
            {
                desiredMove = move;
            }
        }

        private void ComputePointedWorldPoint ()
        {
            if (cam != null)
            {
                Ray mouseRay = cam.ScreenPointToRay (Input.mousePosition);

                if (Physics.Raycast(mouseRay, out RaycastHit hit, 200, LayerMask.GetMask("Environment")))
                {
                    pointedWorldPoint = hit.point;
                }
            }
        }
    }
}

