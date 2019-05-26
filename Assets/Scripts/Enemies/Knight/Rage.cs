using System.Collections;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Rage : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer[] crystals = null;
        [SerializeField] ParticleSystem ragePs = null;
        [SerializeField] ParticleSystem exhaustionPs = null;
        [SerializeField] Light[] lights = null;

        [SerializeField] Color rageColor = default;
        [SerializeField] Color exhaustionColor = default;

        private Color[] lightInitialColors;
        private Color[] crystalInitialColors;

        private void Start()
        {
            lightInitialColors = new Color[lights.Length];
            crystalInitialColors = new Color[crystals.Length];

            for (int i = 0; i < crystals.Length; i++)
            {
                crystalInitialColors[i] = crystals[i].material.GetColor("_EmissionColor");
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lightInitialColors[i] = lights[i].color;
            }
        }

        public void StartRageDelayed(float delay)
        {
            StartCoroutine(DelayedRage(delay));
        }

        IEnumerator DelayedRage(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartRage();
        }

        public void StartRage()
        {
            ragePs.Play();

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor("_EmissionColor", rageColor);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = rageColor;
            }
        }

        public void StartExhaustion()
        {
            ragePs.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            exhaustionPs.Play();

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor("_EmissionColor", exhaustionColor);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = exhaustionColor;
            }
        }

        public void StopExhaustion()
        {
            exhaustionPs.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor("_EmissionColor", crystalInitialColors[i]);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = lightInitialColors[i];
            }
        }
    }
}