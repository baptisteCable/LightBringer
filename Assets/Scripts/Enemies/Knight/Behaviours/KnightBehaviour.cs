namespace LightBringer.Enemies.Knight
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

        public virtual void Init() { }

        public virtual void Abort()
        {
            complete = true;
        }
    }
}