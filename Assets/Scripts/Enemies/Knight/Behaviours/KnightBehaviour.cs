using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public abstract class KnightBehaviour
    {
        protected KnightMotor em;
        public bool complete = false;
        public float startTime;

        public KnightBehaviour(KnightMotor enemyMotor)
        {
            em = enemyMotor;
        }

        public abstract void Run();

        public virtual void Init() {
            startTime = Time.time;
        }

        public virtual void Abort()
        {
            complete = true;
        }

        public virtual void End()
        {
            complete = true;
        }
    }
}