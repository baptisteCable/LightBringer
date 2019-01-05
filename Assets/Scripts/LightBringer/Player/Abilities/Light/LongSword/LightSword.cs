﻿using UnityEngine;
namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class LightSword : MonoBehaviour
    {
        private GameObject particles;
        private GameObject glow1;
        private GameObject glow2;

        public bool isLoaded;

        void Start()
        {
            particles = transform.Find("FxParticles").gameObject;
            glow1 = transform.Find("Glow1").gameObject;
            glow2 = transform.Find("Glow2").gameObject;

            Unload();
        }

        public void Load()
        {
            isLoaded = true;
            particles.SetActive(true);
            glow1.SetActive(true);
            glow2.SetActive(true);
        }

        public void Unload()
        {
            isLoaded = false;
            particles.SetActive(false);
            glow1.SetActive(false);
            glow2.SetActive(false);
        }
    }
}

