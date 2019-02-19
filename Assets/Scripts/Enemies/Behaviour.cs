using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies.Knight;
using UnityEngine;

namespace LightBringer.Enemies
{
    public abstract class Behaviour
    {
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
            public GameObject indicator;

            public Part(State state, float startTime, float duration, GameObject indicator)
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

        protected void DisplayIndicator(GameObject indicator, float loadingTime)
        {
            indicator.SetActive(true);
            indicator.GetComponent<IndicatorLoader>().Load(loadingTime);
        }
    }
}