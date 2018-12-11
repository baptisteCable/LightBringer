using UnityEngine;

namespace LightBringer
{
    public abstract class Ability
    {
        public bool coolDownUp;
        public float coolDownRemaining;
        public float coolDownDuration;
        public float castingDuration;
        public float castingTime;
        public float channelingDuration;
        public float channelingTime;
        public bool channelingCancellable;
        public bool castingCancellable;
        protected Character character;

        public Ability(float coolDownDuration, float channelingDuration, float castingDuration, Character character, bool channelingCancellable, bool castingCancellable)
        {
            coolDownUp = true;
            this.coolDownDuration = coolDownDuration;
            this.channelingDuration = channelingDuration;
            this.castingDuration = castingDuration;
            this.character = character;
            this.channelingCancellable = channelingCancellable;
        }

        public abstract void StartChanneling();

        public abstract void Channel();

        public abstract void StartAbility();

        public abstract void DoAbility();

        public abstract void End();

        public abstract void CancelChanelling();
    }
}

