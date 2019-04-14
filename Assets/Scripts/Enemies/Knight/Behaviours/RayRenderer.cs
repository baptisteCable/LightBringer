using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class RayRenderer : MonoBehaviour
    {
        private const float VANISH_DURATION = .1f;
        private const float PARTICLE_DELAY = .5f;

        [SerializeField] private ParticleSystem ps1;
        [SerializeField] private ParticleSystem ps2;
        [SerializeField] private LineRenderer line;
        private float endTime;
        private float selfDestroyTime;

        public void SetupAndStart(float duration, float length)
        {
            endTime = Time.time + duration;
            selfDestroyTime = Time.time + duration + VANISH_DURATION;

            // Line length
            line.SetPosition(0, new Vector3(0, 0, length));

            // Duration
            ParticleSystem.MainModule main1 = ps1.main;
            main1.duration = duration + PARTICLE_DELAY;
            ParticleSystem.MainModule main2 = ps2.main;
            main2.duration = duration + PARTICLE_DELAY;

            // Start life time
            main1.startLifetime = duration + PARTICLE_DELAY;
            main2.startLifetime = duration + PARTICLE_DELAY;

            // Emission cone length
            ParticleSystem.ShapeModule shape1 = ps1.shape;
            shape1.length = Mathf.Max(length - (duration + PARTICLE_DELAY) * ps1.main.startSpeed.constant, .1f);
            ParticleSystem.ShapeModule shape2 = ps2.shape;
            shape2.length = Mathf.Max(length - (duration + PARTICLE_DELAY) * ps1.main.startSpeed.constant, .1f);

            // burst (.5 partivle per unit)
            ps1.emission.SetBurst(0, new ParticleSystem.Burst(0, (short)(length / 2)));
            ps2.emission.SetBurst(0, new ParticleSystem.Burst(0, (short)(length / 2)));

            // Play ps1 and ps2
            ps1.Play();
            ps2.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time > endTime)
            {
                line.widthMultiplier = Mathf.Max(1 - (Time.time - endTime) / VANISH_DURATION, 0);
            }

            // Self destroy
            if (Time.time > selfDestroyTime)
            {
                Destroy(gameObject);
            }
        }
    }
}