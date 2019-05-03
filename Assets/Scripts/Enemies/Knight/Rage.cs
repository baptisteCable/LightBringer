using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Rage : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer[] crystals;
        [SerializeField] ParticleSystem ragePs;
        [SerializeField] ParticleSystem exhaustionPs;
        [SerializeField] Light[] lights;

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

        public void StartRage()
        {
            ragePs.Play();

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor("_EmissionColor", new Color(1, 0, 0));
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = new Color(1, 0, 0);
            }
        }

        public void StartExhaustion()
        {
            ragePs.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            exhaustionPs.Play();

            for (int i = 0; i < crystals.Length; i++)
            {
                crystals[i].material.SetColor("_EmissionColor", new Color(.25f, .125f, .06f));
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = new Color(.25f, .125f, .06f);
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