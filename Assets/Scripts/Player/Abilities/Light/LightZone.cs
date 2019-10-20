using UnityEngine;

namespace LightBringer.Player.Abilities.Light
{
    [RequireComponent (typeof (Collider))]
    [RequireComponent (typeof (Animator))]
    public class LightZone : MonoBehaviour
    {
        private const float DURATION = 8f;

        private float destructionTime;
        private bool destructionPlanned = false;

        [SerializeField] private GameObject pointLight = null;

        public bool canBeAbsorbed = true;

        void Start ()
        {
            destructionTime = Time.time + DURATION;
            GetComponent<Animator> ().Play ("GrowUp");
            pointLight.SetActive (true);
        }

        void Update ()
        {
            if (Time.time > destructionTime && !destructionPlanned)
            {
                GetComponent<Animator> ().Play ("SelfDestroy");
                DestroyLZ ();
            }
        }

        private void DestroyLZ ()
        {
            pointLight.SetActive (false);
            canBeAbsorbed = false;
            GetComponent<Collider> ().enabled = false;
            transform.Find ("FxParticules").GetComponent<ParticleSystem> ().Stop (false, ParticleSystemStopBehavior.StopEmitting);
            Destroy (gameObject, 4f);
            destructionPlanned = true;
        }

        public void Absorb ()
        {
            GetComponent<Animator> ().Play ("Absorb");
            DestroyLZ ();
        }
    }
}
