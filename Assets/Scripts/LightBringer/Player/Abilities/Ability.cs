using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public abstract class Ability
    {
        public const float CANCELLING_CC_FACTOR = .3f;

        public bool coolDownUp;
        public float coolDownRemaining;
        public float coolDownDuration;
        public float castingDuration;
        public float castingTime;
        public float channelingDuration;
        public float channelingTime;
        public bool channelingCancellable;
        public bool castingCancellable;
        public bool locked;
        protected Character character;

        public Ability(float coolDownDuration, float channelingDuration, float castingDuration, Character character, bool channelingCancellable, bool castingCancellable)
        {
            coolDownUp = true;
            locked = false;
            this.coolDownDuration = coolDownDuration;
            this.channelingDuration = channelingDuration;
            this.castingDuration = castingDuration;
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
            character.animator.Play("NoAction");
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
            character.animator.Play("NoAction");
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
                character.animator.Play("NoAction");
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
            channelingTime += Time.deltaTime;

            if (channelingTime > channelingDuration)
            {
                StartAbility();
            }
        }

        public virtual void Cast()
        {
            castingTime += Time.deltaTime;

            if (castingTime > castingDuration)
            {
                End();
            }
        }

        public virtual void StartChanneling()
        {
            channelingTime = 0;
            character.currentChanneling = this;
        }

        public virtual void StartAbility()
        {
            character.currentAbility = this;
            character.currentChanneling = null;
            castingTime = 0;
        }

        protected void resetMovementRestrictions()
        {
            character.abilityMoveMultiplicator = 1f;
            character.abilityMaxRotation = -1f;
        }

        public virtual void ComputeSpecial()
        {
        }

        protected void SetLockedOtherAbilities(bool locked)
        {
            for (int i = 0; i < character.abilities.Length; i++)
            {
                if (character.abilities[i] != this)
                {
                    character.abilities[i].locked = locked;
                }
            }
        }

        protected bool CannotStartStandard()
        {
            return
                    !coolDownUp ||
                    character.currentAbility != null ||
                    character.currentChanneling != null ||
                    character.psm.isInterrupted ||
                    character.psm.isStunned ||
                    locked;
        }

        protected bool JumpIntialisation()
        {
            if (
                    !coolDownUp ||
                    character.psm.isRooted ||
                    character.psm.isInterrupted ||
                    character.psm.isStunned ||
                    locked
                )
            {
                return false;
            }

            character.Cancel();

            return character.currentAbility == null;
        }

        public virtual void SpecialCancel()
        {
            Debug.LogError("No special cancel for this ability: " + this.GetType());
        }
    }
}

