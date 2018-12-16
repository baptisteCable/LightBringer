using UnityEngine;

namespace LightBringer
{
    [RequireComponent(typeof(Collider))]
    public class AbilityColliderTrigger : MonoBehaviour {

        private Collider col;
        private CollisionAbility currentAbility;
        private bool forcedStart = false;

        void Start()
        {
            if (!forcedStart)
            {
                RunAtStart();
            }
        }

        public void SetAbility(CollisionAbility ability)
        {
            col.enabled = true;
            currentAbility = ability;
        }

        public void UnsetAbility()
        {
            col.enabled = false;
            currentAbility = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            currentAbility.OnCollision(this, other);
        }

        public void ForcedStart()
        {
            RunAtStart();
            forcedStart = true;
        }

        private void RunAtStart()
        {
            col = GetComponent<Collider>();
            col.enabled = false;
            currentAbility = null;
        }
    }
}


