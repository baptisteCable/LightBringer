using UnityEngine;
using LightBringer.Enemies;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbDef : Ability
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION = 8f;
        private const float CHANNELING_DURATION = 6f / 60f;
        private const float ABILITY_DURATION = 0f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = 0f;

        private const float COUNTER_DURATION = 1.2f;

        //Game objects 
        private LightSword sword;

        public AbDef(Character character, LightSword sword) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
        }

        public override void StartChanneling()
        {
            if (CannotStartStandard())
            {
                return;
            }

            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            character.animator.Play("BotAbDef");
            character.animator.Play("TopAbDef");
        }

        public override void StartAbility()
        {
            base.StartAbility();

            character.psm.AddAndStartState(new LightLongSwordCounter(COUNTER_DURATION, sword));
        }

    }
}
