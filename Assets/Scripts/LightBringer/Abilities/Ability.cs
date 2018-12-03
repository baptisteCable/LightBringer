using UnityEngine;

namespace LightBringer
{
    public abstract class Ability
    {
        public bool coolDownUp { get; set; }
        public float coolDownRemaining { get; set; }
        public float coolDownDuration { get; set; }
        public float abilityDuration { get; set; }
        public float abilityTime { get; set; }
        public float channelingDuration { get; set; }
        public float channelingTime { get; set; }
        public bool channelingCancellable { get; set; }
        protected Character character;

        public Ability(float coolDownDuration, float channelingDuration, float abilityDuration, Character character, bool channelingCancellable)
        {
            coolDownUp = true;
            this.coolDownDuration = coolDownDuration;
            this.channelingDuration = channelingDuration;
            this.abilityDuration = abilityDuration;
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

