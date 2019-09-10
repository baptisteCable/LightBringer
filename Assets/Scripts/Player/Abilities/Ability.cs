using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public enum AbilityState
    {
        cooldownInProgress = 0,
        cooldownUp = 1,
        channeling = 2,
        casting = 3
    }

    public abstract class Ability
    {
        public const float CANCELLING_CC_FACTOR = .3f;

        public AbilityState state;
        public float coolDownRemaining;
        public float coolDownDuration;
        public float castDuration;
        public float castStartTime;
        public float channelDuration;
        public float channelStartTime;
        public bool channelingCancellable;
        public bool castingCancellable;
        public bool parallelizable;
        public bool available = true;
        public bool locked = false;
        public int id;

        protected PlayerMotor playerMotor;

        public List<GameObject> indicators;

        public Ability(float coolDownDuration, float channelingDuration, float castingDuration, PlayerMotor motor,
            bool channelingCancellable, bool castingCancellable, bool parallelizable, int id)
        {
            state = AbilityState.cooldownUp;
            coolDownRemaining = 0;
            this.coolDownDuration = coolDownDuration;

            channelDuration = channelingDuration;
            castDuration = castingDuration;

            playerMotor = motor;

            this.channelingCancellable = channelingCancellable;
            this.castingCancellable = castingCancellable;
            this.parallelizable = parallelizable;

            this.id = id;

            indicators = new List<GameObject>();
        }

        public virtual void CancelChanelling()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // Cooldown
            coolDownRemaining = coolDownDuration * CANCELLING_CC_FACTOR;
            state = AbilityState.cooldownInProgress;

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

            // Cooldown
            coolDownRemaining = coolDownDuration;
            state = AbilityState.cooldownInProgress;

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

            // Cooldown
            coolDownRemaining = coolDownDuration;
            state = AbilityState.cooldownInProgress;

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

            // Cooldown
            coolDownRemaining = coolDownDuration;
            state = AbilityState.cooldownInProgress;
        }

        public virtual void Channel()
        {
            if (Time.time > channelStartTime + channelDuration)
            {
                StartAbility();
            }
        }

        public virtual void Cast()
        {
            if (Time.time > castStartTime + castDuration)
            {
                End();
            }
        }

        public virtual void StartChanneling()
        {
            state = AbilityState.channeling;
            channelStartTime = Time.time;
        }

        public virtual void StartAbility()
        {
            state = AbilityState.casting;
            castStartTime = Time.time;
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
                    state == AbilityState.cooldownUp &&
                    playerMotor.canStartNonParallelizableAbility() &&
                    !playerMotor.psm.isStunned &&
                    available;
        }

        // Not just a test: can cancel other abilities
        protected bool CanStartEsc()
        {
            if (
                    state != AbilityState.cooldownUp ||
                    playerMotor.psm.isRooted ||
                    playerMotor.psm.isStunned ||
                    !available
                )
            {
                return false;
            }

            playerMotor.Cancel();

            return playerMotor.canStartNonParallelizableAbility();
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

        public abstract string GetDescription();

        public abstract string GetTitle();
    }
}

