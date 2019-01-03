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
        private const float COOLDOWN_DURATION = .00f;
        private const float ABILITY_DURATION = 15f / 60f;
        private const float ABILITY_DURATION3 = 31f / 60f;
        private const float CHANNELING_DURATION = 9f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = 1f;
        private const float CASTING_MOVE_MULTIPLICATOR = .7f;
        private const float DAMAGE = 2f;

        private const float INTERRUPT_DURATION = .6f;
        private const float COMBO_DURATION = 1f;

        private const float LIGHT_TIME = 15f / 60f;

        // Combo
        public float comboTime = Time.time;
        public int currentAttack = 1;

        private AbilityColliderTrigger weaponCollider;
        private List<Collider> enemies;

        // Light spawn
        private bool lightSpawned = false;


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

            if (Time.time > comboTime)
            {
                currentAttack = 1;
            }
            else
            {
                currentAttack += 1;
            }

            // animation
            if (currentAttack == 1)
            {
                character.animator.Play("Ab1a");
            }
            else if (currentAttack == 2)
            {
                character.animator.Play("Ab1b");
            }
            else if (currentAttack == 3)
            {
                character.animator.Play("Ab1c");
            }
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // No more rotation
            character.canRotate = false;

            if (currentAttack < 3)
            {
                // enemy list
                enemies = new List<Collider>();

                // activate collider
                weaponCollider.SetAbility(this);

                character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR;

                castingDuration = ABILITY_DURATION;
            }
            else
            {
                character.abilityMoveMultiplicator = 0;
                castingDuration = ABILITY_DURATION3;
                lightSpawned = false;
            }
        }

        public override void Cast()
        {
            base.Cast();

            if (currentAttack == 3 && castingTime > LIGHT_TIME && !lightSpawned)
            {
                SpawnLight();
            }
        }

        private void SpawnLight()
        {
            lightSpawned = true;
            Debug.Log("Spawn Light");
        }

        public override void End()
        {
            base.End();

            // Combo
            if (currentAttack < 3)
            {
                comboTime = Time.time + .2f;
            }
            
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

            // desactivate collider
            weaponCollider.UnsetAbility();

            // Interrupt character
            character.psm.Interrupt(INTERRUPT_DURATION);
        }
    }
}