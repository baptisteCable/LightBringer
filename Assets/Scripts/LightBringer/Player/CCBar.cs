using UnityEngine;

namespace LightBringer.Player
{
    public class CCBar : MonoBehaviour {
        private GameObject rooted;
        private GameObject stunned;
        private GameObject interrupted;
        public PlayerStatusManager psm;

        private void Start()
        {
            rooted = transform.Find("Rooted").gameObject;
            stunned = transform.Find("Stunned").gameObject;
            interrupted = transform.Find("Interrupted").gameObject;
        }

        void Update() {
            rooted.SetActive(psm.isRooted);
            stunned.SetActive(psm.isStunned);
            interrupted.SetActive(psm.isInterrupted);
        }
    }
}