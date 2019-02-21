using System;
using System.Collections;
using UnityEngine;

namespace LightBringer.Effects
{
    public class FlashEffect : MonoBehaviour
    {
        [SerializeField] private string[] exclusions = new string[1] { "UI" };
        [SerializeField] private float duration = .1f;
        [SerializeField] private Transform[] transformsToFlash = new Transform[0];
        [SerializeField] private Color emissionColor = new Color(.2f, .1f, .1f);

        public void Flash()
        {
            StopCoroutine(FlashCoroutine());
            StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            RecFlash(transform, true);
            for (int i = 0; i < transformsToFlash.Length; i++)
            {
                RecFlash(transformsToFlash[i], true);
            }

            yield return new WaitForSeconds(duration);

            RecFlash(transform, false);
            for (int i = 0; i < transformsToFlash.Length; i++)
            {
                RecFlash(transformsToFlash[i], false);
            }
        }

        private void RecFlash(Transform tr, bool on)
        {
            if (Array.IndexOf(exclusions, tr.tag) < 0)
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    if (on)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }

            foreach (Transform child in tr)
            {
                RecFlash(child, on);
            }
        }
    }
}
