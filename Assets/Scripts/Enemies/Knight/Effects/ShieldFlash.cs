using LightBringer.Effects;
using UnityEngine;

namespace Assets.Scripts.Enemies.Knight.Effects
{
    class ShieldFlash : FlashEffect
    {
        [SerializeField] private ParticleSystem ps = null;

        public override void Flash()
        {
            base.Flash();
            ps.Play();
        }
    }
}
