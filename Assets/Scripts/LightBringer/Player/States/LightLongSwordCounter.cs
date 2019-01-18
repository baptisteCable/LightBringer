using LightBringer.Enemies;
using UnityEngine;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class LightLongSwordCounter : State
    {
        private const bool CANCELLABLE = true;

        private const float IMMATERIAL_DURATION = 1f;
        private const float HASTE_DURATION = 2f;

        private LightSword sword;

        public LightLongSwordCounter(float duration, LightSword sword) : base(CANCELLABLE, duration)
        {
            this.sword = sword;
        }

        public override Damage AlterTakenDamage(Damage dmg, Motor dealer, Vector3 origin)
        {
            if (!complete && (dmg.type == DamageType.Melee || dmg.type == DamageType.RangeInstant || dmg.type == DamageType.Projectile))
            {
                psm.AddAndStartState(new Immaterial(IMMATERIAL_DURATION + 20f));
                psm.AddAndStartState(new Haste(HASTE_DURATION));

                if (!sword.isLoaded)
                {
                    sword.Load();
                }

                Stop();
                dmg.amount = 0;

                return dmg;
            }
            else
            {
                return dmg;
            }
        }

        public override void Start(PlayerStatusManager psm)
        {
            base.Start(psm);

            psm.hasteMoveMultiplicator = 0f;

            // Lock all abilities
            psm.character.LockAbilitiesExcept(true);

            // TODO Effect
        }

        public override void Stop()
        {
            base.Stop();

            psm.hasteMoveMultiplicator = 1f;

            // Unlock all abilities
            psm.character.LockAbilitiesExcept(false);

            // TODO Effect end
        }

        public override void Cancel()
        {
            base.Cancel();

            Stop();
        }
    }
}
