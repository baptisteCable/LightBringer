using LightBringer.Player.Class;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbDef : Ability
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;
        private const bool PARALLELIZABLE = false;

        // const
        private const float COOLDOWN_DURATION = 16f;
        private const float CHANNELING_DURATION = 6f / 60f;
        private const float ABILITY_DURATION = 0f / 60f;

        private const float COUNTER_DURATION = 1.2f;

        // Inherited motor
        LightLongSwordMotor lightMotor;

        public AbDef(LightLongSwordMotor playerMotor, int id) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, playerMotor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE, PARALLELIZABLE, id)
        {
            lightMotor = playerMotor;
        }

        public override void StartChanneling()
        {
            base.StartChanneling();
            playerMotor.abilityMoveMultiplicator = 0;
            playerMotor.abilityMaxRotation = 0;

            lightMotor.animator.SetBool("isInDefPos", true);
            lightMotor.animator.Play("BotAbDef");
            lightMotor.animator.Play("TopAbDef");
        }

        public override void StartAbility()
        {
            base.StartAbility();

            playerMotor.psm.AddAndStartState(new LightLongSwordCounter(COUNTER_DURATION, lightMotor.sword));
        }

    }
}
