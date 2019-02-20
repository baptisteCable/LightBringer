using System.Collections.Generic;
using LightBringer.Abilities;
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

        protected void StartCollisionParts()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsStartTime(i))
                {
                    StartCollisionPart(i);
                }
            }
        }

        protected virtual void StartCollisionPart(int i)
        {
            StartPart(i);
            actGOs[i].SetActive(true);
            acts[i].SetAbility(this);
            cols = new List<Collider>();
        }

        protected virtual void RunCollisionParts()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsRunTime(i))
                {
                    RunCollisionPart(i);
                }
            }
        }

        protected virtual void RunCollisionPart(int part)
        {
            if (IsEndTime(part))
            {
                EndCollisionPart(part);
            }
        }

        protected void EndCollisionPart(int i)
        {
            EndPart(i);
            actGOs[i].SetActive(false);
            acts[i].UnsetAbility();
        }

        public abstract void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col);
    }
}