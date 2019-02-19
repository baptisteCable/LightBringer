using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Enemies.Knight
{
    public class GoToTargetBehaviour : KnightBehaviour
    {
        float stopDist;
        Transform target;

        public GoToTargetBehaviour(KnightMotor enemyMotor, float stopDist, Transform target) : base(enemyMotor)
        {
            this.stopDist = stopDist;
            this.target = target;
        }

        public override void Init()
        {
            em.agent.SetDestination(target.position);
            em.agent.isStopped = false;
        }

        public override void Run()
        {
            if (!em.agent.pathPending)
            {
                em.agent.SetDestination(target.position);
            }

            if (em.agent.remainingDistance < em.agent.stoppingDistance + stopDist)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            em.agent.isStopped = true;
        }
    }
}