using UnityEngine;

namespace LightBringer.Player
{
    public abstract class State
    {
        public float startTime;
        public float endTime = 0;
        public PlayerStatusManager psm;
        public bool complete = false;
        public bool cancellable;

        public State(bool cancellable, float duration = 0)
        {
            if (duration > 0)
            {
                endTime = Time.time + duration;
            }
            else
            {
                endTime = 0;
            }

            this.cancellable = cancellable;
        }

        public virtual Damage AlterTakenDamage(Damage dmg, EnemyMotor dealer, Vector3 origin)
        {
            return dmg;
        }

        public virtual bool IsAffectedBy(Damage dmg, EnemyMotor dealer, Vector3 origin)
        {
            return true;
        }

        public virtual Damage AlterDealtDamage(Damage dmg)
        {
            return dmg;
        }

        public virtual void Start(PlayerStatusManager psm)
        {
            this.psm = psm;
            startTime = Time.time;
        }

        public virtual void Stop()
        {
            complete = true;
        }

        public virtual void Update()
        {
            if (endTime > 0 && Time.time > endTime)
            {
                Stop();
            }
        }

        public virtual void Cancel()
        {
            if (cancellable)
            {
                //TODO cancel anim

                complete = true;
            }
        }
    }
}

