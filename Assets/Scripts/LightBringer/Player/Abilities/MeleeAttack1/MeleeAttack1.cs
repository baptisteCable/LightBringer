using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Player.Abilities
{
    public class MeleeAttack1 : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = true;

        // const
        private const float COOLDOWN_DURATION = .01f;
        private const float ABILITY_DURATION = .2f;
        private const float CHANNELING_DURATION = .4f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = .3f;
        private const float DAMAGE = 2f;
        
        private const float INTERRUPT_DURATION = .6f;

        private AbilityColliderTrigger weaponCollider;
        private List<Collider> enemies;


        private bool triggerCreated;
        
        public MeleeAttack1(Character character, GameObject weapon) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.weaponCollider = weapon.GetComponent<AbilityColliderTrigger>();
        }

        
        public override void StartChanneling()
        {
            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelMeleeAttack1");
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // animation
            character.animator.SetBool("startMeleeAttack1", true);

            // No more rotation
            character.canRotate = false;
            character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR;

            // enemy list
            enemies = new List<Collider>();

            // activate collider
            weaponCollider.SetAbility(this);
        }

        public override void End()
        {
            base.End();

            // animation
            character.animator.SetBool("startMeleeAttack1", false);

            // desactivate collider
            weaponCollider.UnsetAbility();
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            // desactivate collider
            weaponCollider.UnsetAbility();
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if (col.tag == "Enemy")
            {
                if (!enemies.Contains(col))
                {
                    enemies.Add(col);
                    col.GetComponent<DamageController>().TakeDamage(DAMAGE);
                }
            }

            if (col.tag == "Shield")
            {
                Interrupt();
            }
        }

        public void Interrupt()
        {
            // movement back
            character.canRotate = true;
            character.abilityMoveMultiplicator = 1f;

            character.currentAbility = null;
            coolDownRemaining = coolDownDuration;

            // animation
            character.animator.SetBool("startMeleeAttack1", false);

            // desactivate collider
            weaponCollider.UnsetAbility();

            // Interrupt character
            character.psm.Interrupt(INTERRUPT_DURATION);
        }
    }
}