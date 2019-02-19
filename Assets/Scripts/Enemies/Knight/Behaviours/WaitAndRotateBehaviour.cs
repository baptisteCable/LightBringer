using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class WaitAndRotateBehaviour : KnightBehaviour
    {
        private Transform target;
        private float duration;

        public WaitAndRotateBehaviour(KnightMotor enemyMotor, float duration, Transform target) : base(enemyMotor)
        {
            this.duration = duration;
            this.target = target;
        }

        public override void Run()
        {
            em.RotateTowards(target.position);

            duration -= Time.deltaTime;

            if (duration < 0)
            {
                End();
            }
        }
    }
}