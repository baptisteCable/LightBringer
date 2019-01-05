using UnityEngine;

namespace LightBringer.Player.Abilities.Light
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    public class LightZone : MonoBehaviour
    {
        private const float DURATION = 8f;

        private float destructionTime;

        void Start()
        {
            destructionTime = Time.time + DURATION;
        }

        void Update()
        {
            if (Time.time > destructionTime)
            {
                SelfDestroy();
            }
        }

        public void SelfDestroy()
        {
            GetComponent<Animator>().Play("SelfDestroy");
            Destroy(gameObject, 13f / 60f);
        }

        public void Absorb()
        {
            GetComponent<Collider>().enabled = false;
            transform.Find("FxParticules").GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
            GetComponent<Animator>().Play("Absorb");
            Destroy(gameObject, 4f);
        }
    }

}
