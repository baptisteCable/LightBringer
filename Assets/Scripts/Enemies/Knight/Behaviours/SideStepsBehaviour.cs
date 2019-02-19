using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class SideStepsBehaviour : Behaviour
    {
        private Transform target;
        private float duration;
        private bool left;
        private float currentDirectionDuration;

        public SideStepsBehaviour(KnightMotor enemyMotor, float duration, Transform target) : base(enemyMotor)
        {
            this.duration = duration;
            this.target = target;
        }

        public override void Init()
        {
            em.SetOverrideAgent(true);
            left = (Random.value < .5f);
            NewDirection();
        }

        public override void Run()
        {
            Vector3 mainDir = target.position - em.transform.position;
            Vector3 leftAroundDir = Vector3.Cross(mainDir, Vector3.up);
            if (!left)
            {
                leftAroundDir = -leftAroundDir;
            }
            em.MoveInDirection(leftAroundDir);
            em.RotateTowards(target.position);

            currentDirectionDuration -= Time.deltaTime;
            if (currentDirectionDuration < 0)
            {
                NewDirection();
            }

            duration -= Time.deltaTime;

            if (duration < 0)
            {
                End();
            }
        }

        private void NewDirection()
        {
            left = !left;
            currentDirectionDuration = Random.value * .3f + .3f;
        }

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);
        }
    }
}