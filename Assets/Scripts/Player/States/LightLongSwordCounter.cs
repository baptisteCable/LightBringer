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

        public LightLongSwordCounter (float duration, LightSword sword) : base (CANCELLABLE, duration)
        {
            this.sword = sword;
        }

        public override Damage AlterTakenDamage (Damage dmg, Motor dealer, Vector3 origin)
        {
            if (!complete)
            {
                psm.AddAndStartState (new Immaterial (IMMATERIAL_DURATION));
                psm.AddAndStartState (new Haste (HASTE_DURATION));

                if (!sword.isLoaded)
                {
                    sword.Load ();
                }

                Stop ();
                dmg.amount = 0;

                return dmg;
            }
            else
            {
                return dmg;
            }
        }

        public override void Start (PlayerStatusManager psm)
        {
            base.Start (psm);

            psm.moveMultiplicators.Add (this, 0);
            psm.maxRotation.Add (this, 0);

            // Lock all abilities
            psm.playerMotor.LockAbilitiesExcept (true, psm.playerMotor.abilities[PlayerController.IN_AB_ESC]);

            // TODO Effect
        }

        public override void Stop ()
        {
            base.Stop ();

            psm.moveMultiplicators.Remove (this);
            psm.maxRotation.Remove (this);

            // Unlock all abilities
            psm.playerMotor.LockAbilitiesExcept (false);

            // TODO Effect end

            // Animator
            psm.playerMotor.animator.SetBool ("isInDefPos", false);
        }

        public override void Cancel ()
        {
            base.Cancel ();

            Stop ();
        }

        public override bool isAffectedByCC (CrowdControl cc)
        {
            if (!complete)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
