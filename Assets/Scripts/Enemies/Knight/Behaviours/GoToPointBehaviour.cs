using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class GoToPointBehaviour : EnemyBehaviour
    {
        Vector3 target;

        public GoToPointBehaviour(KnightMotor enemyMotor, Vector3 target) : base(enemyMotor)
        {
            this.target = target;
        }

        public override void Init()
        {
            em.agent.SetDestination(target);
            em.agent.isStopped = false;
        }

        public override void Run()
        {
            if (em.agent.remainingDistance < em.agent.stoppingDistance)
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