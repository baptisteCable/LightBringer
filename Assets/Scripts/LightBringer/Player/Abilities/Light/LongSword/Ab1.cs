using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class Ab1 : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION = 0f;
        private const float ABILITY_DURATION = 6f / 60f;
        private const float CHANNELING_DURATION_AB = 21f / 60f;
        private const float CHANNELING_DURATION_C = 30f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR_AB = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR_C = 0;
        private const float CASTING_ROTATION_SPEED = 0;
        private const float DAMAGE_AB = 10f;
        private const float DAMAGE_C = 12f;

        private const float INTERRUPT_DURATION = .6f;
        private const float COMBO_DURATION = .5f;


        // Combo
        public float comboTime = Time.time;
        public int currentAttack = 1;

        private List<Collider> encounteredCols;

        // Prefabs
        private GameObject lightZonePrefab;
        private GameObject abTriggerPrefab;
        private GameObject cTriggerPrefab;
        private GameObject lightSpawnEffetPrefab;
        private GameObject impactEffetPrefab;

        // GameObjects
        private LightSword sword;
        private GameObject trigger;

        public Ab1(Character character, LightSword sword) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION_AB, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            lightZonePrefab = Resources.Load("Player/Light/LightZone/LightZone") as GameObject;
            abTriggerPrefab = Resources.Load("Player/Light/LongSword/Ab1/Ab1ab") as GameObject;
            cTriggerPrefab = Resources.Load("Player/Light/LongSword/Ab1/Ab1c") as GameObject;
            lightSpawnEffetPrefab = Resources.Load("Player/Light/LongSword/Ab1/LightSpawnEffect") as GameObject;
            impactEffetPrefab = Resources.Load("Player/Light/LongSword/ImpactEffect") as GameObject;
        }


        public override void StartChanneling()
        {
            if (CannotStartStandard())
            {
                return;
            }

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

            // Channeling time
            if (currentAttack < 3)
            {
                channelingDuration = CHANNELING_DURATION_AB;
            }
            else
            {
                channelingDuration = CHANNELING_DURATION_C;
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

            // Trail effect
            sword.transform.Find("FxTrail").GetComponent<ParticleSystem>().Play();

            // No more rotation
            character.abilityMaxRotation = CASTING_ROTATION_SPEED;

            // collider list
            encounteredCols = new List<Collider>();

            if (currentAttack < 3)
            {
                character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR_AB;
                CreateTriggerAB();
            }
            else
            {
                character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR_C;
                SpawnLight();
            }
        }

        private void CreateTriggerAB()
        {
            trigger = GameObject.Instantiate(abTriggerPrefab);
            trigger.transform.SetParent(character.gameObject.transform.Find("CharacterContainer"));
            trigger.transform.localPosition = new Vector3(0f, .1f, 0f);
            trigger.transform.localRotation = Quaternion.identity;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        private void SpawnLight()
        {
            Vector3 pos = new Vector3(sword.transform.position.x, .2f, sword.transform.position.z);

            GameObject lightZone = GameObject.Instantiate(lightZonePrefab, null);
            lightZone.transform.position = pos;

            // Particle effect
            GameObject lightSpawn = GameObject.Instantiate(lightSpawnEffetPrefab, null);
            lightSpawn.transform.position = pos;
            GameObject.Destroy(lightSpawn, 1f);

            // Damage zone (trigger)
            trigger = GameObject.Instantiate(cTriggerPrefab, null);
            trigger.transform.position = pos;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        public override void End()
        {
            base.End();

            // Combo
            if (currentAttack < 3)
            {
                ApplyAllDamageAB();
                comboTime = Time.time + COMBO_DURATION;
            }
            else
            {
                ApplyDamageC();
                comboTime = 0f;
            }

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }
        }

        private void ApplyAllDamageAB()
        {
            Collider closestCol = null;
            float minDist = 10000f;
            float dist;
            Vector3 basePoint = character.transform.position + Vector3.up;

            // find the closest collider
            foreach (Collider col in encounteredCols)
            {
                dist = (col.ClosestPoint(basePoint) - basePoint).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closestCol = col;
                }
            }

            if (closestCol != null)
            {
                if (closestCol.tag == "Enemy")
                {
                    ApplyDamageAB(closestCol, closestCol.ClosestPoint(basePoint));
                }
                else if (closestCol.tag == "Shield")
                {
                    // Interrupt character
                    character.psm.Interrupt(INTERRUPT_DURATION);
                }
            }
        }

        private void ApplyDamageAB(Collider col, Vector3 impactPoint)
        {
            Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE_AB, DamageType.Melee, DamageElement.Light));
            col.GetComponent<StatusController>().TakeDamage(dmg, character);

            GameObject impactEffect = GameObject.Instantiate(impactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            if ((character.transform.position + Vector3.up - impactPoint).magnitude > .05f)
            {
                impactEffect.transform.rotation = Quaternion.LookRotation(character.transform.position + Vector3.up - impactPoint, Vector3.up);
            }
            GameObject.Destroy(impactEffect, 1f);
        }

        private void ApplyDamageC()
        {
            foreach (Collider col in encounteredCols)
            {
                if (col.tag == "Enemy")
                {
                    Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE_C, DamageType.AreaOfEffect, DamageElement.Light));
                    col.GetComponent<StatusController>().TakeDamage(dmg, character);
                }
            }
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy" || col.tag == "Shield") && !encounteredCols.Contains(col))
            {
                encounteredCols.Add(col);
            }
        }
    }
}