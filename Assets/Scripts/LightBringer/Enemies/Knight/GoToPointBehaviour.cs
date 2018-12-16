using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Knight
{
    public class GoToPointBehaviour : KnightBehaviour
    {
        float stopDist;
        Transform target;

        public GoToPointBehaviour(KnightMotor enemyMotor, float stopDist, Transform target) : base(enemyMotor)
        {
            this.stopDist = stopDist;
            em.agent.SetDestination(target.position);
            this.target = target;
        }

        public override void Run()
        {
            if (!em.agent.pathPending)
            {
                em.agent.SetDestination(target.position);
            }

            if (em.agent.remainingDistance < em.agent.stoppingDistance + stopDist)
            {
                em.agent.isStopped = true;
                End();
            }
        }

        public void End()
        {
            complete = true;
        }
    }
}