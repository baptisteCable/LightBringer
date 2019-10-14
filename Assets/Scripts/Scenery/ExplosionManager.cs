using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Scenery
{
    public class ExplosionManager : MonoBehaviour
    {
        private const int MAX_EXPLOSION = 20;

        [SerializeField]
        Material[] materials = null;

        AnimationCurve curve;
        List<Explosion> explosions;

        public static ExplosionManager singleton;

        void Start()
        {
            if (singleton == null)
            {
                singleton = this;
            }
            else
            {
                Debug.LogError("More than one explosionManager...");
            }

            curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.1f, 1);
            curve.AddKey(.5f, -.333333f);
            curve.AddKey(.75f, 0.166667f);
            curve.AddKey(1f, -0.111111f);
            curve.AddKey(1.25f, 0.083333f);
            curve.AddKey(1.5f, 0);

            explosions = new List<Explosion>();
        }

        // Update is called once per frame
        void Update()
        {
            while (explosions.Count > MAX_EXPLOSION)
            {
                explosions.RemoveAt(0);
            }

            if (explosions.Count > 0)
            {
                float[] amplitudes = new float[MAX_EXPLOSION];
                float[] xCenters = new float[MAX_EXPLOSION];
                float[] zCenters = new float[MAX_EXPLOSION];
                for (int i = 0; i < explosions.Count; i++)
                {
                    if (Time.time - explosions[i].startTime > 1.5f)
                    {
                        explosions.RemoveAt(i);
                        i--;
                        continue;
                    }
                    amplitudes[i] = curve.Evaluate(Time.time - explosions[i].startTime) * explosions[i].power;
                    xCenters[i] = explosions[i].center.x;
                    zCenters[i] = explosions[i].center.y;
                }

                foreach (Material mat in materials)
                {
                    mat.SetFloatArray("_Amplitude", amplitudes);
                    mat.SetFloatArray("_XCenter", xCenters);
                    mat.SetFloatArray("_ZCenter", zCenters);
                    mat.SetFloat("_ExplosionCount", explosions.Count);
                }
            }
        }

        public void Explode(Vector2 pos, float power)
        {
            explosions.Add(new Explosion(pos, Time.time, power));
        }
    }
}
