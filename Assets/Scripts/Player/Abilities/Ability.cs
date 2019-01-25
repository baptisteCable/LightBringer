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
        //public float castingTime;
        public float castStartTime;
        public float castEndTime;
        public float channelDuration;
        public float channelStartTime;
        public float channelEndTime;
        public bool channelingCancellable;
        public bool castingCancellable;
        public bool locked;
        protected Character character;

        public Ability(float coolDownDuration, float channelingDuration, float castingDuration, Character character, bool channelingCancellable, bool castingCancellable)
        {
            coolDownUp = true;
            locked = false;
            this.coolDownDuration = coolDownDuration;
            this.channelDuration = channelingDuration;
            this.castDuration = castingDuration;
            this.character = character;
            this.channelingCancellable = channelingCancellable;
            this.castingCancellable = castingCancellable;
        }

        public virtual void CancelChanelling()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            character.currentChanneling = null;

            // Cooldown
            coolDownRemaining = coolDownDuration * CANCELLING_CC_FACTOR;

            // animation
            character.animator.Play("TopIdle");
            character.animator.Play("BotIdle");
        }

        public virtual void AbortChanelling()
        {
           // Movement restrictions
            resetMovementRestrictions();

            // current ability
            character.currentChanneling = null;

            // Cooldown
            coolDownRemaining = coolDownDuration;

            // animation
            character.animator.Play("TopIdle");
            character.animator.Play("BotIdle");
        }

        public virtual void AbortCasting()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            character.currentAbility = null;

            // Cooldown
            coolDownRemaining = coolDownDuration;

            // animation
            if (!character.psm.isInterrupted)
            {
                character.animator.Play("TopIdle");
                character.animator.Play("BotIdle");
            }
        }

        public virtual void End()
        {
            // Movement restrictions
            resetMovementRestrictions();

            // current ability
            character.currentAbility = null;

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

            character.currentChanneling = this;
        }

        public virtual void StartAbility()
        {

            castStartTime = Time.time;
            castEndTime = Time.time + castDuration;

            character.currentAbility = this;
            character.currentChanneling = null;
        }

        protected void resetMovementRestrictions()
        {
            character.abilityMoveMultiplicator = 1f;
            character.abilityMaxRotation = -1f;
        }

        public virtual void ComputeSpecial()
        {
        }

        protected bool CannotStartStandard()
        {
            return
                    !coolDownUp ||
                    character.currentAbility != null ||
                    character.currentChanneling != null ||
                    character.psm.isInterrupted ||
                    character.psm.isStunned ||
                    locked ||
                    !available;
        }

        protected bool JumpIntialisationValid()
        {
            if (
                    !coolDownUp ||
                    character.psm.isRooted ||
                    character.psm.isInterrupted ||
                    character.psm.isStunned ||
                    locked ||
                    !available
                )
            {
                return false;
            }

            character.Cancel();

            return character.currentAbility == null && character.currentChanneling == null;
        }

        public virtual void SpecialCancel()
        {
            Debug.LogError("No special cancel for this ability: " + this.GetType());
        }
    }
}

