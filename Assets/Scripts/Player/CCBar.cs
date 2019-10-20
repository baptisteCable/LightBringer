using UnityEngine;
using UnityEngine.UI;


namespace LightBringer.Player
{
    [RequireComponent (typeof (Text))]
    public class CCBar : MonoBehaviour
    {
        private Text ccText;
        public PlayerStatusManager psm;

        private void Start ()
        {
            ccText = GetComponent<Text> ();
        }

        void Update ()
        {
            if (psm.isStunned)
            {
                ccText.text = "Stunned";
            }
            else if (psm.isRooted)
            {
                ccText.text = "Rooted";
            }
            else
            {
                ccText.text = "";
            }
        }
    }
}