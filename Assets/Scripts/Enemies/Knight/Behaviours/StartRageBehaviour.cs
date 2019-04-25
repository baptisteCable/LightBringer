using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class StartRageBehaviour : EnemyBehaviour
    {
        private const float DURATION = 1.7f;

        public StartRageBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("StartRage", -1, 0);
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