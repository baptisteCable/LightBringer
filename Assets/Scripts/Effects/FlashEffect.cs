using System;
using UnityEngine;

namespace LightBringer.Effects
{
    public class FlashEffect : MonoBehaviour
    {
        [SerializeField] private string[] exclusions = new string[2] { "UI", "NoFlash" };
        [SerializeField] private float duration = .1f;
        [SerializeField] private Transform[] transformsToFlash = new Transform[0];
        [SerializeField] private Color emissionColor = new Color (.2f, .1f, .1f);

        float flashEnd = 0;
        bool flashIsOn = false;

        public virtual void Flash ()
        {
            flashEnd = Time.time + duration;
        }

        private void Update ()
        {
            if (Time.time < flashEnd && !flashIsOn)
            {
                RecFlash (transform, true);
                for (int i = 0; i < transformsToFlash.Length; i++)
                {
                    RecFlash (transformsToFlash[i], true);
                }
                flashIsOn = true;
            }
            else if (flashIsOn && Time.time > flashEnd)
            {
                RecFlash (transform, false);
                for (int i = 0; i < transformsToFlash.Length; i++)
                {
                    RecFlash (transformsToFlash[i], false);
                }
                flashIsOn = false;
            }

        }

        private void RecFlash (Transform tr, bool on)
        {
            if (Array.IndexOf (exclusions, tr.tag) < 0)
            {
                Renderer renderer = tr.GetComponent<Renderer> ();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer> ().material;

                    if (on)
                    {
                        mat.EnableKeyword ("_EMISSION");
                        mat.SetColor ("_EmissionColor", emissionColor);
                    }
                    else
                    {
                        mat.DisableKeyword ("_EMISSION");
                    }
                }
            }

            foreach (Transform child in tr)
            {
                RecFlash (child, on);
            }
        }
    }
}
