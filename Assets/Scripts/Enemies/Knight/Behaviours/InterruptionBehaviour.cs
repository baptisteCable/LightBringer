using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class InterruptionBehaviour : EnemyBehaviour
    {
        private const float DURATION = 1f;

        private float duration;
        Vector3 dmgOrigin;

        public InterruptionBehaviour(KnightMotor enemyMotor, Vector3 dmgOrigin) : base(enemyMotor)
        {
            this.dmgOrigin = dmgOrigin;
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("Interruption", -1, 0);

            // Rotate to face dmg origin
            float angle = Vector3.SignedAngle(em.transform.position, dmgOrigin - em.transform.position, Vector3.up);
            em.transform.localRotation = Quaternion.Euler(
                em.transform.localRotation.x,
                em.transform.localRotation.y + angle,
                em.transform.localRotation.z);
        }

        public override void Run()
        {
            if (Time.time >= startTime + DURATION)
            {
                End();
            }
        }
    }
}