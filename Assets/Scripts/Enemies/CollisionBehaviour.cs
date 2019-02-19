using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies
{
    public abstract class CollisionBehaviour : Behaviour, CollisionAbility
    {
        private const float INDICATOR_DISPLAY_TIME = .5f;
        private const float DURATION = 2.9f;
        private const float CHARGE_RANGE = 20f;

        private const float DMG_CHECKPOINT_1_START = 60f / 60f;
        private const float DMG_CHECKPOINT_1_END = 63f / 60f;
        private const float POS_CHECKPOINT_1_START = 72f / 60f;
        private const float POS_CHECKPOINT_1_END = 80f / 60f;
        private const float DMG_CHECKPOINT_2_START = 90f / 60f;
        private const float DMG_CHECKPOINT_2_END = 92f / 60f;
        private const float DMG_CHECKPOINT_3_START = 110f / 60f;
        private const float DMG_CHECKPOINT_3_END = 152f / 60f;

        // Collider list
        protected List<Collider> cols;

        // Colliders GO
        public GameObject[] actGOs;
        protected AbilityColliderTrigger[] acts;


        float stopDist;
        Transform target;

        public CollisionBehaviour(Motor enemyMotor) : base(enemyMotor)
        {
        }

        protected bool InitPart(int i)
        {
            if (Time.time > startTime + parts[i].startTime && parts[i].state == State.IndicatorDisplayed)
            {
                // Indicator 1
                parts[i].indicator.SetActive(false);

                // collision
                actGOs[i].SetActive(true);
                acts[i].SetAbility(this);
                cols = new List<Collider>();

                // state
                parts[i].state = State.InProgress;

                return true;
            }

            return false;
        }

        public abstract void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col);
    }
}