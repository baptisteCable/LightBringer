using LightBringer.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies
{
    public abstract class CollisionBehaviour : EnemyBehaviour, CollisionAbility
    {
        // Collider list
        protected Dictionary<Collider, float> cols;

        // Colliders GO
        public GameObject[] actGOs;
        protected AbilityColliderTrigger[] acts;


        float stopDist;
        Transform target;

        public CollisionBehaviour (Motor enemyMotor) : base (enemyMotor)
        {
        }

        protected void StartCollisionParts ()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsStartTime (i))
                {
                    StartCollisionPart (i);
                }
            }
        }

        protected virtual void StartCollisionPart (int i)
        {
            StartPart (i);
            //actGOs[i].SetActive(true);
            acts[i].SetAbility (this);
            cols = new Dictionary<Collider, float> ();
        }

        protected virtual void RunCollisionParts ()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsRunTime (i))
                {
                    RunCollisionPart (i);
                }
            }
        }

        protected virtual void RunCollisionPart (int part)
        {
            if (IsEndTime (part))
            {
                EndCollisionPart (part);
            }
        }

        protected void EndCollisionPart (int i)
        {
            EndPart (i);
            acts[i].UnsetAbility ();
            //actGOs[i].SetActive(false);
        }

        public override void Abort ()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                acts[i].UnsetAbility ();
                //actGOs[i].SetActive(false);
            }

            base.Abort ();
        }

        public abstract void OnColliderEnter (AbilityColliderTrigger abilityColliderTrigger, Collider col);

        public virtual void OnColliderStay (AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
        }

    }
}