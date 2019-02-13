using LightBringer.Networking;
using UnityEngine;

namespace LightBringer.Player.Abilities.Light
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    public class LightZone : DelayedNetworkBehaviour
    {
        private const float DURATION = 8f;

        private float destructionTime;
        private bool destructionPlanned = false;

        [SerializeField] private GameObject pointLight;

        public bool canBeAbsorbed = true;

        void Start()
        {
            if (isServer)
            {
                destructionTime = Time.time + DURATION;
                CallByName("GrowUp");
            }
        }

        void Update()
        {
            if (isServer && Time.time > destructionTime && !destructionPlanned)
            {
                CallByName("SelfDestroy");
            }
        }

        private void GrowUp()
        {
            GetComponent<Animator>().Play("GrowUp");
            pointLight.SetActive(true);
        }

        private void SelfDestroy()
        {
            GetComponent<Animator>().Play("SelfDestroy");
            DestroyLZ();
        }

        private void Absorb()
        {
            GetComponent<Animator>().Play("Absorb");
            DestroyLZ();
        }

        private void DestroyLZ()
        {
            pointLight.SetActive(false);
            canBeAbsorbed = false;
            GetComponent<Collider>().enabled = false;
            transform.Find("FxParticules").GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Destroy(gameObject, 4f);
            destructionPlanned = true;
        }
    }

}
