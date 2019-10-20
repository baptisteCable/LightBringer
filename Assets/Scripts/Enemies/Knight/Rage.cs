using System.Collections;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Rage : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer[] crystals = null;
        [SerializeField] ParticleSystem ragePs = null;
        [SerializeField] ParticleSystem exhaustionPs = null;
        [SerializeField] Light pointLight = null;

        [SerializeField] Color rageColor = default;
        [SerializeField] Color exhaustionColor = default;

        private Color lightInitialColor;
        private Color[] crystalInitialColors;

        private void Start ()
        {
            crystalInitialColors = new Color[crystals.Length];

            for (int i = 0; i < crystals.Length; i++)
            {
                crystalInitialColors[i] = crystals[i].material.GetColor ("_EmissionColor");
            }

            lightInitialColor = pointLight.color;
        }

        public void StartRageDelayed (float delay)
        {
            StartCoroutine (DelayedRage (delay));
        }

        IEnumerator DelayedRage (float delay)
        {
            yield return new WaitForSeconds (delay);
            StartRage ();
        }

        public void StartRage ()
        {
            ragePs.Play ();

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor ("_EmissionColor", rageColor);
            }

            pointLight.color = rageColor;
            pointLight.intensity = 2;
        }

        public void StartExhaustion ()
        {
            ragePs.Stop (true, ParticleSystemStopBehavior.StopEmitting);
            exhaustionPs.Play ();

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor ("_EmissionColor", exhaustionColor);
            }

            pointLight.color = exhaustionColor;
        }

        public void StopExhaustion ()
        {
            exhaustionPs.Stop (true, ParticleSystemStopBehavior.StopEmitting);

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor ("_EmissionColor", crystalInitialColors[i]);
            }

            pointLight.color = lightInitialColor;
            pointLight.intensity = 1;
        }
    }
}