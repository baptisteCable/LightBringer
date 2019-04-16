using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class RayRenderer : MonoBehaviour
    {
        private const float SPAWN_DURATION = .05f;
        private const float VANISH_DURATION = .05f;
        private const float PARTICLE_DURATION = .5f;
        private const float LINE_WIDTH = 7f;

        [SerializeField] private ParticleSystem ps1;
        [SerializeField] private ParticleSystem ps2;
        [SerializeField] private ParticleSystem emiter;
        [SerializeField] private LineRenderer line;
        private float spawnTime = -1;
        private float endTime = -1;
        private float selfHideTime = -1;

        public void SetupAndStart(float duration, float length)
        {
            spawnTime = Time.time + SPAWN_DURATION;
            endTime = Time.time + duration;
            selfHideTime = Time.time + duration + VANISH_DURATION;

            // Line activation
            line.gameObject.SetActive(true);

            // Length
            SetLength(length);

            // Duration
            ParticleSystem.MainModule main1 = ps1.main;
            main1.duration = duration;
            ParticleSystem.MainModule main2 = ps2.main;
            main2.duration = duration;

            // Start life time
            main1.startLifetime = PARTICLE_DURATION;
            main2.startLifetime = PARTICLE_DURATION;

            // emmission: 1 particle per unit for each
            ParticleSystem.EmissionModule emMod1 = ps1.emission;
            emMod1.rateOverTime = (int)length;
            ParticleSystem.EmissionModule emMod2 = ps2.emission;
            emMod2.rateOverTime = (int)length;

            // Emiter ps duration
            ParticleSystem.MainModule mainEmiter = emiter.main;
            mainEmiter.duration = duration;

            // Play ps1, ps2 and emiter
            ps1.Play();
            ps2.Play();
            emiter.Play();
        }

        public void SetLength(float length)
        {
            // Line length
            line.SetPosition(0, new Vector3(0, 0, length));

            // Emission cone length
            ParticleSystem.ShapeModule shape1 = ps1.shape;
            shape1.length = Mathf.Max(length - PARTICLE_DURATION * ps1.main.startSpeed.constant, .1f);
            ParticleSystem.ShapeModule shape2 = ps2.shape;
            shape2.length = Mathf.Max(length - PARTICLE_DURATION * ps1.main.startSpeed.constant, .1f);
        }

        // Update is called once per frame
        void Update()
        {
            // Spawning effect
            if (spawnTime > 0 && Time.time < spawnTime)
            {
                line.widthMultiplier = Mathf.Min(1 - (spawnTime - Time.time) / SPAWN_DURATION, 1) * LINE_WIDTH;
            }
            else if (spawnTime > 0)
            {
                spawnTime = -1;
                line.widthMultiplier = LINE_WIDTH;
            }

            // Vanishing effect
            if (endTime > 0 && Time.time > endTime)
            {
                emiter.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                line.widthMultiplier = Mathf.Max(1 - (Time.time - endTime) / VANISH_DURATION, 0) * LINE_WIDTH;
            }

            // Self destroy
            if (selfHideTime > 0 && Time.time > selfHideTime)
            {
                line.gameObject.SetActive(false);
                selfHideTime = -1;
                endTime = -1;
            }
        }
    }
}