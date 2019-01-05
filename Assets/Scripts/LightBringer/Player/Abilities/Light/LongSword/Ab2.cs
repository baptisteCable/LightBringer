using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class Ab2 : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION = 2f;
        private const float CHANNELING_DURATION = 18f / 60f;
        private const float ABILITY_DURATION = 6f / 60f;
        private const float DMG_TIME = 5f / 60f;
        private const float TRIGGER_DURATION = 4f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = 0f;
        private const float CASTING_MOVE_MULTIPLICATOR = 0f;
        private const float DAMAGE_UNLOADED = 10f;
        private const float DAMAGE_LOADED = 25f;

        private const float DASH_DISTANCE = 4f;

        private const float INTERRUPT_DURATION = .6f;

        // Colliders
        private List<Collider> encounteredCols;

        // Prefabs
        private GameObject triggerPrefab;

        // GameObjects
        private LightSword sword;
        private GameObject trigger;
        private Transform characterContainer;

        public Ab2(Character character, LightSword sword) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            triggerPrefab = Resources.Load("Player/Light/LongSword/Ab2/Trigger") as GameObject;

            characterContainer = character.gameObject.transform.Find("CharacterContainer");
        }


        public override void StartChanneling()
        {
            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            character.animator.Play("Ab2");

            LoadLight();

            encounteredCols = new List<Collider>();

            // TODO : indicator
        }

        private void LoadLight()
        {
            if (!sword.isLoaded)
            {
                Collider[] colliders = Physics.OverlapSphere(character.transform.position, .5f);
                LightZone closestZone = null;
                float shortestDistance = 10000f;

                foreach (Collider col in colliders)
                {
                    LightZone zone = col.GetComponent<LightZone>();
                    if (zone != null)
                    {
                        float distance = (character.transform.position - zone.transform.position).magnitude;
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            closestZone = zone;
                        }
                    }
                }

                if (closestZone != null)
                {
                    closestZone.Absorb();
                    sword.Load();
                }
            }            
        }

        public override void StartAbility()
        {
            base.StartAbility();
            
            // No more rotation
            character.abilityMaxRotation = 0f;

            CreateTrigger();

            character.SetMovementMode(MovementMode.Ability);
            
        }

        public override void Cast()
        {
            base.Cast();

            // movement
            character.rb.velocity = characterContainer.forward * DASH_DISTANCE / ABILITY_DURATION;
        }

        private void CreateTrigger()
        {
            trigger = GameObject.Instantiate(triggerPrefab);
            trigger.transform.SetParent(characterContainer);
            trigger.transform.localPosition = new Vector3(0f, .1f, 0f);
            trigger.transform.localRotation = Quaternion.identity;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        public override void End()
        {
            base.End();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            if (sword.isLoaded)
            {
                sword.Unload();
            }

            character.SetMovementMode(MovementMode.Player);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            character.SetMovementMode(MovementMode.Player);
        }

        private void ApplyDamage(Collider col)
        {
            float damage = DAMAGE_UNLOADED;
            if (sword.isLoaded)
            {
                damage = DAMAGE_LOADED;
            }

            col.GetComponent<DamageController>().TakeDamage(damage);
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy" || col.tag == "Shield") && !encounteredCols.Contains(col))
            {
                encounteredCols.Add(col);
                if (col.tag == "Enemy")
                {
                    ApplyDamage(col);
                }
                else if (col.tag == "Shield")
                {
                    // Interrupt character
                    character.psm.Interrupt(INTERRUPT_DURATION);
                }
            }
        }
    }
}