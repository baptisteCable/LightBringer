using UnityEngine;

namespace LightBringer
{
    [RequireComponent(typeof(Collider))]
    public class AbilityColliderTrigger : MonoBehaviour {

        private Collider col;
        private CollisionAbility currentAbility;

        void Start()
        {
            col = GetComponent<Collider>();
            col.enabled = false;
            currentAbility = null;
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
    }
}


