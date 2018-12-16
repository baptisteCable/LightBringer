using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Knight
{
    public abstract class KnightBehaviour
    {
        protected KnightMotor em;
        public bool complete = false;

        public KnightBehaviour(KnightMotor enemyMotor)
        {
            em = enemyMotor;
        }

        public abstract void Run();
    }
}