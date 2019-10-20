using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class WaitExhaustionBehaviour : EnemyBehaviour
    {
        private const float DURATION = 3.5f;

        private float duration;

        public WaitExhaustionBehaviour (KnightMotor enemyMotor) : base (enemyMotor)
        {
        }

        public override void Init ()
        {
            base.Init ();

            em.anim.SetBool ("IdleExhausted", true);
        }

        public override void Run ()
        {
            if (Time.time >= startTime + DURATION)
            {
                End ();
                em.anim.SetBool ("IdleExhausted", false);
            }
        }
    }
}