using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class WaitBehaviour : EnemyBehaviour
    {
        public override bool isAction { get { return false; } }

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