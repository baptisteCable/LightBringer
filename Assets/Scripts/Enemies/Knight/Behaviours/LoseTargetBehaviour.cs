using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class LoseTargetBehaviour : Behaviour
    {
        private const float DURATION = 2f;
        private const float TARGET_LOSE_TIME = 1f;

        bool lost = false;

        public LoseTargetBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
        }

        public override void Run()
        {
            if (!lost && Time.time > startTime + TARGET_LOSE_TIME)
            {
                lost = true;
                ((KnightController)em.controller).target = null;
                ((KnightController)em.controller).targetModificationTime = Time.time;
                em.head.NoTarget();
            }

            if (Time.time > startTime + DURATION)
            {
                End();
            }
        }
    }
}