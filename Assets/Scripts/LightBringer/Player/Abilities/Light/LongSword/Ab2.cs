using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = 0f;
        private const float DAMAGE_UNLOADED = 10f;
        private const float DAMAGE_LOADED = 25f;

        private const float DASH_DISTANCE = 4f;

        private const float INTERRUPT_DURATION = .6f;

        // Colliders
        private List<Collider> encounteredCols;
        private Dictionary<Collider, float> newCols;

        // Prefabs
        private GameObject triggerPrefab;
        private GameObject impactEffetPrefab;
        private GameObject loadedImpactEffetPrefab;

        // GameObjects
        private LightSword sword;
        private GameObject trigger;
        private Transform characterContainer;

        // Indicator
        private GameObject indicatorPrefab;

        // Status
        private bool interrupted = false;

        public Ab2(Character character, LightSword sword) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            triggerPrefab = Resources.Load("Player/Light/LongSword/Ab2/Trigger") as GameObject;
            impactEffetPrefab = Resources.Load("Player/Light/LongSword/ImpactEffect") as GameObject;
            loadedImpactEffetPrefab = Resources.Load("Player/Light/LongSword/Ab2/LoadedImpactEffect") as GameObject;
            indicatorPrefab = Resources.Load("Player/Light/LongSword/Ab2/Ab2Indicator") as GameObject;

            characterContainer = character.gameObject.transform.Find("CharacterContainer");
        }


        public override void StartChanneling()
        {
            if (CannotStartStandard())
            {
                return;
            }

            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            interrupted = false;

            character.animator.Play("Ab2");

            LoadLight();

            encounteredCols = new List<Collider>();

            // Indicator
            DisplayIndicator();
        }

        private void DisplayIndicator()
        {
            GameObject indicator = GameObject.Instantiate(indicatorPrefab, characterContainer);
            GameObject.Destroy(indicator, channelingDuration);
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

            newCols = new Dictionary<Collider, float>();
        }

        public override void Cast()
        {
            base.Cast();

            // movement
            character.rb.velocity = characterContainer.forward * DASH_DISTANCE / ABILITY_DURATION;

            ApplyEffectToNew();
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

        private void ApplyEffectToNew()
        {
            while (newCols.Count > 0 && !interrupted)
            {
                Collider col = newCols.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
                ApplyEffect(col);
                newCols.Remove(col);
            }
        }

        private void ApplyEffect(Collider col)
        {
            if (col.tag == "Enemy")
            {
                ApplyDamage(col);
            }
            else if (col.tag == "Shield")
            {
                // Interrupt character
                character.psm.Interrupt(INTERRUPT_DURATION);

                if (sword.isLoaded)
                {
                    sword.Unload();
                }

                interrupted = true;
            }
        }

        private void ApplyDamage(Collider col)
        {
            Vector3 impactPoint = col.ClosestPoint(character.transform.position + Vector3.up);
            Quaternion impactRotation = Quaternion.LookRotation(character.transform.position + Vector3.up - impactPoint, Vector3.up);

            // base damage
            float damageAmount = DAMAGE_UNLOADED;
            if (sword.isLoaded)
            {
                // damage update
                damageAmount = DAMAGE_LOADED;

                // Effect
                GameObject loadedImpactEffect = GameObject.Instantiate(loadedImpactEffetPrefab, null);
                loadedImpactEffect.transform.position = impactPoint;
                loadedImpactEffect.transform.rotation = impactRotation;
                GameObject.Destroy(loadedImpactEffect, 1f);
            }

            // Apply damage
            Damage dmg = character.psm.AlterDealtDamage(new Damage(damageAmount, DamageType.Melee, DamageElement.Light));
            col.GetComponent<StatusController>().TakeDamage(dmg, character);

            // Effect
            GameObject impactEffect = GameObject.Instantiate(impactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            impactEffect.transform.rotation = impactRotation;
            GameObject.Destroy(impactEffect, 1f);
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy" || col.tag == "Shield") && !encounteredCols.Contains(col))
            {
                encounteredCols.Add(col);
                float distance = (col.ClosestPoint(character.transform.position) - character.transform.position).magnitude;
                newCols.Add(col, distance);
            }
        }
    }
}