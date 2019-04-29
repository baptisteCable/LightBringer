using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class StartRageBehaviour : EnemyBehaviour
    {
        private const float DURATION = 1.7f;

        private const float SHIELD_ON_GROUND = 39f / 60f;

        private KnightMotor km;
        private bool effectStarted = false;

        public StartRageBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
            km = enemyMotor;
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("StartRage", -1, 0);
        }

        public override void Run()
        {
            if (Time.time >= startTime + SHIELD_ON_GROUND && !effectStarted)
            {
                effectStarted = true;
                km.startRagePs.Play(true);
                km.rage.StartRage();
            }


            if (Time.time >= startTime + DURATION)
            {
                End();
            }
        }
    }
}