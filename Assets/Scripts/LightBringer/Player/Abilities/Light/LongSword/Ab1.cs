using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class Ab1 : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION = 0f;
        private const float ABILITY_DURATION_AB = 6f / 60f;
        private const float ABILITY_DURATION_C = 6f / 60f;
        private const float CHANNELING_DURATION_AB = 20f / 60f;
        private const float CHANNELING_DURATION_C = 30f / 60f;
        private const float LIGHT_TIME = 3f / 60f;

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

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        // Prefabs
        private GameObject lightZonePrefab;
        private GameObject abTriggerPrefab;
        private GameObject cTriggerPrefab;
        private GameObject lightSpawnEffetPrefab;
        private GameObject impactEffetPrefab;

        // GameObjects
        private LightSword sword;
        private GameObject trigger;

        // Misc
        private bool lightSpawned;

        // Effects
        private ParticleSystem slashAEffect, slashBEffect;

        public Ab1(Character character, LightSword sword) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION_AB, ABILITY_DURATION_AB, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            lightZonePrefab = Resources.Load("Player/Light/LightZone/LightZone") as GameObject;
            abTriggerPrefab = Resources.Load("Player/Light/LongSword/Ab1/Ab1ab") as GameObject;
            cTriggerPrefab = Resources.Load("Player/Light/LongSword/Ab1/Ab1c") as GameObject;
            lightSpawnEffetPrefab = Resources.Load("Player/Light/LongSword/Ab1/LightSpawnEffect") as GameObject;
            impactEffetPrefab = Resources.Load("Player/Light/LongSword/ImpactEffect") as GameObject;

            // Create slash objects
            GameObject slashAGO = GameObject.Instantiate(
                Resources.Load("Player/Light/LongSword/Ab1/Ab1aSlash") as GameObject,
                character.characterContainer);
            slashAEffect = slashAGO.GetComponent<ParticleSystem>();
            GameObject slashBGO = GameObject.Instantiate(
                Resources.Load("Player/Light/LongSword/Ab1/Ab1bSlash") as GameObject,
                character.characterContainer);
            slashBEffect = slashBGO.GetComponent<ParticleSystem>();

        }


        public override void StartChanneling()
        {
            if (CannotStartStandard())
            {
                return;
            }

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
                channelDuration = CHANNELING_DURATION_AB;
                castDuration = ABILITY_DURATION_AB;
            }
            else
            {
                channelDuration = CHANNELING_DURATION_C;
                castDuration = ABILITY_DURATION_C;
                lightSpawned = false;
            }

            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            base.StartChanneling();


            // animation
            if (currentAttack == 1)
            {
                character.animator.Play("BotAb1a", -1, 0);
                character.animator.Play("TopAb1a", -1, 0);
            }
            else if (currentAttack == 2)
            {
                character.animator.Play("BotAb1b");
                character.animator.Play("TopAb1b");
            }
            else if (currentAttack == 3)
            {
                character.animator.Play("BotAb1c");
                character.animator.Play("TopAb1c");
            }
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // Trail effect
            SlashEffect();


            // No more rotation
            character.abilityMaxRotation = CASTING_ROTATION_SPEED;

            // collider list
            encounteredCols = new Dictionary<Collider, Vector3>();

            if (currentAttack < 3)
            {
                character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR_AB;
                CreateTriggerAB();
            }
            else
            {
                character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR_C;
            }
        }

        private void SlashEffect()
        {
            if (currentAttack == 1)
            {
                slashAEffect.Play();
            }
            if (currentAttack == 2)
            {
                slashBEffect.Play();
            }
        }

        public override void Cast()
        {
            base.Cast();

            if (currentAttack == 3 && Time.time > castStartTime + LIGHT_TIME && !lightSpawned)
            {
                lightSpawned = true;
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

            //Cancel combo
            comboTime = 0f;

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }
        }

        public override void AbortChanelling()
        {
            base.AbortChanelling();
            
            //Cancel combo
            comboTime = 0f;
        }

        public override void CancelChanelling()
        {
            base.CancelChanelling();

            //Cancel combo
            comboTime = 0f;
        }

        private void ApplyAllDamageAB()
        {
            Collider closestCol = null;
            float minDist = float.PositiveInfinity;
            float dist;

            int id = Random.Range(int.MinValue, int.MaxValue);

            // find the closest collider (and the extraDamage ones)
            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                if (pair.Key != null)
                {

                    // Extra damage: deal damage
                    DamageTaker dt = pair.Key.GetComponent<DamageTaker>();
                    if (dt != null && dt.extraDmg)
                    {
                        ApplyDamageAB(pair.Key, pair.Value, id);
                    }
                    else
                    {
                        dist = (pair.Key.ClosestPoint(pair.Value) - pair.Value).magnitude;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestCol = pair.Key;
                        }
                    }
                }
            }

            if (closestCol != null)
            {
                if (closestCol.tag == "Enemy")
                {
                    ApplyDamageAB(closestCol, encounteredCols[closestCol], id);
                }
                else if (closestCol.tag == "Shield")
                {
                    // Interrupt character
                    character.psm.ApplyCrowdControl(
                        new CrowdControl(CrowdControlType.Interrupt, DamageType.Self, DamageElement.None),
                        INTERRUPT_DURATION
                    );
                }
            }

        }

        private void ApplyDamageAB(Collider col, Vector3 origin, int id)
        {
            Vector3 impactPoint = col.ClosestPoint(origin);

            Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE_AB, DamageType.Melee, DamageElement.Light));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, character, character.transform.position, id);

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
            int id = Random.Range(int.MinValue, int.MaxValue);

            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                if (pair.Key.tag == "Enemy")
                {
                    Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE_C, DamageType.AreaOfEffect, DamageElement.Light));
                    pair.Key.GetComponent<DamageTaker>().TakeDamage(dmg, character, pair.Value, id);
                }
            }
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy" || col.tag == "Shield") && !encounteredCols.ContainsKey(col))
            {
                if (currentAttack < 3)
                {
                    encounteredCols.Add(col, character.transform.position + Vector3.up);
                }
                else
                {
                    encounteredCols.Add(col, act.transform.position);
                }
            }
        }
    }
}