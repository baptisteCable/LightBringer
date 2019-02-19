using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class WaitBehaviour : KnightBehaviour
    {
        private float duration;

        public WaitBehaviour(KnightMotor enemyMotor, float duration) : base(enemyMotor)
        {
            this.duration = duration;
        }

        public override void Run()
        {
            duration -= Time.deltaTime;

            if (duration < 0)
            {
                End();
            }
        }
    }
}