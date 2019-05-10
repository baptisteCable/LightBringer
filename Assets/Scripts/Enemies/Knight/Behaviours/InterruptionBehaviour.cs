using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class InterruptionBehaviour : EnemyBehaviour
    {
        private const float DURATION = .49f;

        private float duration;

        public InterruptionBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("Interruption", -1, 0);
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