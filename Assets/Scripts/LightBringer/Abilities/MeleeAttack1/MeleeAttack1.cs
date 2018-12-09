using UnityEngine;
using System.Collections.Generic;

namespace LightBringer
{
    public class MeleeAttack1 : CollisionAbility
    {
        private const float COOLDOWN_DURATION = .01f;
        private const float ABILITY_DURATION = .2f;
        private const float CHANNELING_DURATION = .4f;
        private const bool CHANNELING_CANCELLABLE = true;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float DOING_MOVE_MULTIPLICATOR = .3f;
        private const float DAMAGE = 2f;
        
        private WeaponCollider weaponCollider;
        private List<Collider> enemies;


        private bool triggerCreated;
        
        public MeleeAttack1(Character character, GameObject weapon) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE)
        {
            this.weaponCollider = weapon.GetComponent<WeaponCollider>();
        }

        
        public override void StartChanneling()
        {
            channelingTime = 0;
            character.currentChanneling = this;
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelMeleeAttack1");
        }

        public override void Channel()
        {
            channelingTime += Time.deltaTime;

            if (channelingTime > channelingDuration)
            {
                StartAbility();
            }
        }

        public override void StartAbility()
        {
            // animation
            character.animator.SetBool("startMeleeAttack1", true);

            // No more rotation
            character.canRotate = false;
            
            character.currentAbility = this;
            character.currentChanneling = null;
            character.abilityMoveMultiplicator = DOING_MOVE_MULTIPLICATOR;
            abilityTime = 0;

            // enemy list
            enemies = new List<Collider>();

            // activate collider
            weaponCollider.SetAbility(this);
        }

        public override void DoAbility()
        {
            abilityTime += Time.deltaTime;

            if (abilityTime > abilityDuration)
            {
                End();
            }
        }

        public override void End()
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
        }

        public override void CancelChanelling()
        {
            character.currentChanneling = null;

            // animation
            character.animator.Play("NoAction");
        }

        public override void OnCollision(Collider col)
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
            character.Interrupt();
        }
    }
}