using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public abstract class Ability
    {
        public const float CANCELLING_CC_FACTOR = .3f;

        public bool available = true;
        public bool coolDownUp;
        public float coolDownRemaining;
        public float coolDownDuration;
        public float castDuration;
        public float castStartTime;
        public float castEndTime;
        public float channelDuration;
        public float channelStartTime;
        public float channelEndTime;
        public bool channelingCancellable;
        public bool castingCancellable;
        public bool locked;

        protected PlayerMotor playerMotor;

        protected List<GameObject> indicators;

        public Ability(float coolDownDuration, float channelingDuration, float castingDuration, PlayerMotor motor, bool channelingCancellable, bool castingCancellable)
        {
            coolDownUp = true;
            locked = false;
            this.coolDownDuration = coolDownDuration;
            channelDuration = channelingDuration;
            castDuration = castingDuration;
            playerMotor = motor;
            this.channelingCancellable = channelingCancellable;
            this.castingCancellable = castingCancellable;

            indicators = new List<GameObject>();
        }

        public virtual void CancelChanelling()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            playerMotor.currentChanneling = null;

            // Cooldown
            coolDownRemaining = coolDownDuration * CANCELLING_CC_FACTOR;

            // animation
            playerMotor.animator.Play("TopIdle");
            playerMotor.animator.Play("BotIdle");

            // indicators
            DestroyIndicators();
        }

        public virtual void AbortChanelling()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            playerMotor.currentChanneling = null;

            // Cooldown
            coolDownRemaining = coolDownDuration;

            // animation
            playerMotor.animator.Play("TopIdle");
            playerMotor.animator.Play("BotIdle");

            // indicators
            DestroyIndicators();
        }

        public virtual void AbortCasting()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            playerMotor.currentAbility = null;

            // Cooldown
            coolDownRemaining = coolDownDuration;

            // animation
            if (!playerMotor.psm.isStunned)
            {
                playerMotor.animator.Play("TopIdle");
                playerMotor.animator.Play("BotIdle");
            }

            // indicators
            DestroyIndicators();
        }

        public virtual void End()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            playerMotor.currentAbility = null;

            // Cooldown
            coolDownRemaining = coolDownDuration;
        }

        public virtual void Channel()
        {
            if (Time.time > channelEndTime)
            {
                StartAbility();
            }
        }

        public virtual void Cast()
        {
            if (Time.time > castEndTime)
            {
                End();
            }
        }

        public virtual void StartChanneling()
        {
            channelStartTime = Time.time;
            channelEndTime = Time.time + channelDuration;

            playerMotor.currentChanneling = this;
        }

        public virtual void StartAbility()
        {

            castStartTime = Time.time;
            castEndTime = Time.time + castDuration;

            playerMotor.currentAbility = this;
            playerMotor.currentChanneling = null;
        }

        protected void resetMovementRestrictions()
        {
            playerMotor.abilityMoveMultiplicator = 1f;
            playerMotor.abilityMaxRotation = -1f;
        }

        public virtual void ComputeSpecial()
        {
        }

        public virtual bool CanStart()
        {
            return
                    coolDownUp &&
                    playerMotor.currentAbility == null &&
                    playerMotor.currentChanneling == null &&
                    !playerMotor.psm.isStunned &&
                    !locked &&
                    available;
        }

        // Not just a test: can cancel other abilities
        protected bool CanStartEsc()
        {
            if (
                    !coolDownUp ||
                    playerMotor.psm.isRooted ||
                    playerMotor.psm.isStunned ||
                    locked ||
                    !available
                )
            {
                return false;
            }

            playerMotor.Cancel();

            return playerMotor.currentAbility == null && playerMotor.currentChanneling == null;
        }

        public virtual void SpecialCancel()
        {
            Debug.LogError("No special cancel for this ability: " + this.GetType());
        }

        private void DestroyIndicators()
        {
            foreach (GameObject go in indicators)
            {
                GameObject.Destroy(go);
            }
            indicators.Clear();
        }
    }
}

