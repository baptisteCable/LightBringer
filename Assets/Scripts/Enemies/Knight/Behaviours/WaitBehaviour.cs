using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class WaitBehaviour : EnemyBehaviour
    {
        private const float DURATION = .3f;
        private const float DURATION_RAGE = .1f;

        private float duration;

        public WaitBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
        }

        public override void Init()
        {
            base.Init();

            if (em.statusManager.mode == Mode.Rage)
            {
                duration = DURATION_RAGE;
            }
            else
            {
                duration = DURATION;
            }
        }

        public override void Run()
        {
            if (Time.time >= startTime + duration)
            {
                End();
            }
        }
    }
}