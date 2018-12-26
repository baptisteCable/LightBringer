using UnityEngine;

namespace LightBringer.Knight
{
    public class SideStepsBehaviour : KnightBehaviour
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
            currentDirectionDuration = Random.value * 2 + .5f;
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
            em.RotateTowards(target.position - em.transform.position);

            currentDirectionDuration -= Time.deltaTime;
            if (currentDirectionDuration < 0)
            {
                left = !left;
                currentDirectionDuration = Random.value * 2 + .5f;
            }

            duration -= Time.deltaTime;

            if (duration < 0)
            {
                End();
            }
        }

        public void End()
        {
            complete = true;
            em.SetOverrideAgent(false);
        }
    }
}