using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class EndExhaustionBehaviour : EnemyBehaviour
    {
        private const float DURATION = 1.3f;

        public EndExhaustionBehaviour (KnightMotor enemyMotor) : base (enemyMotor)
        {
        }

        public override void Init ()
        {
            base.Init ();

            em.anim.Play ("EndExhaustion", -1, 0);
        }

        public override void Run ()
        {
            if (Time.time >= startTime + DURATION)
            {
                End ();
            }
        }
    }
}