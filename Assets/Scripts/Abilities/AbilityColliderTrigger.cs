using UnityEngine;

namespace LightBringer.Abilities
{
    [RequireComponent (typeof (Collider))]
    public class AbilityColliderTrigger : MonoBehaviour
    {

        private Collider col;
        private CollisionAbility currentAbility;
        public string abTag;

        void Start ()
        {
            Prepare ();
        }

        public void SetAbility (CollisionAbility ability, string tag = "")
        {
            Prepare ();
            col.enabled = true;
            currentAbility = ability;
            abTag = tag;
        }

        public void UnsetAbility ()
        {
            col.enabled = false;
            currentAbility = null;
            abTag = "";
        }

        private void OnTriggerEnter (Collider other)
        {
            currentAbility.OnColliderEnter (this, other);
        }

        private void OnTriggerStay (Collider other)
        {
            currentAbility.OnColliderStay (this, other);
        }

        private void Prepare ()
        {
            if (col == null)
            {
                col = GetComponent<Collider> ();
                col.enabled = false;
                currentAbility = null;
                abTag = "";
            }
        }
    }
}


