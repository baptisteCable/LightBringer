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
                CallForAll(M_GrowUp);
            }
        }

        void Update()
        {
            if (isServer && Time.time > destructionTime && !destructionPlanned)
            {
                CallForAll(M_SelfDestroy);
            }
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

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_GrowUp: GrowUp(); return true;
                case M_SelfDestroy: SelfDestroy(); return true;
                case M_Absorb: Absorb(); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // called by id
        public const int M_GrowUp = 0;
        private void GrowUp()
        {
            GetComponent<Animator>().Play("GrowUp");
            pointLight.SetActive(true);
        }

        // called by id
        public const int M_SelfDestroy = 1;
        private void SelfDestroy()
        {
            GetComponent<Animator>().Play("SelfDestroy");
            DestroyLZ();
        }

        // called by id
        public const int M_Absorb = 2;
        private void Absorb()
        {
            GetComponent<Animator>().Play("Absorb");
            DestroyLZ();
        }
    }

}
