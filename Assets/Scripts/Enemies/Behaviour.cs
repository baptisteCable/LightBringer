using UnityEngine;

namespace LightBringer.Enemies
{
    public abstract class Behaviour
    {
        private const float INDICATOR_DISPLAY_TIME = .5f;

        protected Motor em;
        public bool complete = false;
        public float startTime;

        // Parts
        protected Part[] parts;

        protected enum State
        {
            Before = 0,
            IndicatorDisplayed = 1,
            InProgress = 2,
            Terminated = 3
        }

        protected struct Part
        {
            public State state;
            public float startTime;
            public float duration;
            public int indicator;

            public Part(State state, float startTime, float duration, int indicator)
            {
                this.state = state;
                this.startTime = startTime;
                this.duration = duration;
                this.indicator = indicator;
            }
        }

        public Behaviour(Motor enemyMotor)
        {
            em = enemyMotor;
        }

        public abstract void Run();

        public virtual void Init()
        {
            startTime = Time.time;
        }

        public virtual void Abort()
        {
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].indicator != -1)
                    {
                        em.CallForAll(Motor.M_HideIndicator, parts[i].indicator);
                    }
                }
            }

            complete = true;
        }

        public virtual void End()
        {
            complete = true;
        }

        protected void DisplayIndicators()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsDisplayIndicatorTime(i, INDICATOR_DISPLAY_TIME))
                {
                    DisplayIndicator(i, INDICATOR_DISPLAY_TIME);
                }
            }
        }

        protected bool IsDisplayIndicatorTime(int part, float displayTime)
        {
            return Time.time >= startTime + parts[part].startTime - displayTime && parts[part].state == State.Before;
        }

        protected void DisplayIndicator(int part, float loadingTime)
        {
            if (parts[part].indicator != -1)
            {
                em.CallForAll(Motor.M_DisplayIndicator, parts[part].indicator, loadingTime);
            }
            parts[part].state = State.IndicatorDisplayed;
        }

        protected void StartParts()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsStartTime(i))
                {
                    StartPart(i);
                }
            }
        }

        protected bool IsStartTime(int part)
        {
            return Time.time > startTime + parts[part].startTime && parts[part].state == State.IndicatorDisplayed;
        }

        protected virtual void StartPart(int part)
        {
            if (parts[part].indicator != -1)
            {
                em.CallForAll(Motor.M_HideIndicator, parts[part].indicator);
            }
            parts[part].state = State.InProgress;
        }

        protected virtual void RunParts()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsRunTime(i))
                {
                    RunPart(i);
                }
            }
        }

        protected bool IsRunTime(int part)
        {
            return parts[part].state == State.InProgress;
        }

        protected virtual void RunPart(int part)
        {
            if (IsEndTime(part))
            {
                EndPart(part);
            }
        }

        protected bool IsEndTime(int part)
        {
            return parts[part].state == State.InProgress && Time.time >= startTime + parts[part].startTime + parts[part].duration;
        }

        protected virtual void EndPart(int i)
        {
            parts[i].state = State.Terminated;
        }
    }
}