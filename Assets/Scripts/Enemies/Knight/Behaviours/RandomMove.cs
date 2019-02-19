using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class RandomMove : KnightBehaviour
    {
        private Transform target;

        public RandomMove(KnightMotor enemyMotor, Transform target) : base(enemyMotor)
        {
            this.target = target;
        }

        public override void Init()
        {
            float rotation = Random.value * 180;
            Vector3 direction = Quaternion.Euler(0, rotation, 0) * Vector3.forward;
            direction *= Random.value * 8f + 3f;
            
            Vector3 origin = em.transform.position;
            if (target != null)
            {
                origin = (em.transform.position + target.position) / 2f;
                em.DisableAgentRotation();
            }

            em.agent.SetDestination(origin + direction);
            em.agent.isStopped = false;
        }

        public override void Run()
        {
            if (target != null)
            {
                em.DelayedRotateTowards(target.transform.position, .3f);
            }

            if (em.agent.remainingDistance < em.agent.stoppingDistance)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            em.agent.isStopped = true;
            em.EnableAgentRotation();
        }
    }
}