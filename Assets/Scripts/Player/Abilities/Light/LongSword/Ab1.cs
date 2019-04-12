using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Player.Class;
using UnityEngine;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class Ab1 : CollisionPlayerAbility
    {
        // status const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;
        private const bool PARALLELIZABLE = false;

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

        private const float STUN_DURATION = .2f;
        private const float COMBO_DURATION = .5f;

        // Combo
        public float comboTime = Time.time;
        public int currentAttack = 1;

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        // GameObjects
        private GameObject trigger;

        // Misc
        private bool lightSpawned;

        // Inherited motor
        LightLongSwordMotor lightMotor;

        public Ab1(LightLongSwordMotor playerMotor, int id) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION_AB, ABILITY_DURATION_AB, playerMotor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE, PARALLELIZABLE, id)
        {
            lightMotor = playerMotor;
        }

        public override void StartChanneling()
        {
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

            playerMotor.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            base.StartChanneling();


            // animation
            if (currentAttack == 1)
            {
                playerMotor.animator.Play("BotAb1a", -1, 0);
                playerMotor.animator.Play("TopAb1a", -1, 0);
            }
            else if (currentAttack == 2)
            {
                playerMotor.animator.Play("BotAb1b");
                playerMotor.animator.Play("TopAb1b");
            }
            else if (currentAttack == 3)
            {
                playerMotor.animator.Play("BotAb1c");
                playerMotor.animator.Play("TopAb1c");
            }
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // Trail effect
            SlashEffect();


            // No more rotation
            playerMotor.abilityMaxRotation = CASTING_ROTATION_SPEED;

            // collider list
            encounteredCols = new Dictionary<Collider, Vector3>();

            if (currentAttack < 3)
            {
                playerMotor.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR_AB;
                CreateTriggerAB();
            }
            else
            {
                playerMotor.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR_C;
            }
        }

        private void SlashEffect()
        {
            if (currentAttack == 1)
            {
                lightMotor.ab1aSlash.Play();
            }
            if (currentAttack == 2)
            {
                lightMotor.ab1bSlash.Play();
            }
        }

        public override void Cast()
        {
            if (currentAttack == 3 && Time.time > castStartTime + LIGHT_TIME && !lightSpawned)
            {
                lightSpawned = true;
                SpawnLight();
            }

            base.Cast();
        }

        private void CreateTriggerAB()
        {
            trigger = GameObject.Instantiate(lightMotor.ab1abTriggerPrefab, playerMotor.characterContainer);
            trigger.transform.localPosition = new Vector3(0f, .1f, 0f);
            trigger.transform.localRotation = Quaternion.identity;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        private void SpawnLight()
        {
            Vector3 pos = new Vector3(lightMotor.sword.transform.position.x, .2f, lightMotor.sword.transform.position.z);

            GameObject lightZone = GameObject.Instantiate(lightMotor.lightZonePrefab, null);
            lightZone.transform.position = pos;

            // Particle effect
            lightMotor.LightSpawnPE(pos);

            // Damage zone (trigger)
            trigger = GameObject.Instantiate(lightMotor.lightSpawnTriggerPrefab, null);
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
                DamageTaker dt = closestCol.GetComponent<DamageTaker>();
                if (dt.bouncing)
                {
                    // Stun character
                    playerMotor.psm.ApplyCrowdControl(
                        new CrowdControl(CrowdControlType.Stun, DamageType.Self, DamageElement.None),
                        STUN_DURATION
                    );

                    // effect
                    dt.TakeDamage(new Damage(DAMAGE_AB, DamageType.Melee, DamageElement.Light), playerMotor, playerMotor.transform.position, id);
                }
                else
                {
                    ApplyDamageAB(closestCol, encounteredCols[closestCol], id);
                }
            }

        }

        private void ApplyDamageAB(Collider col, Vector3 origin, int id)
        {
            Vector3 impactPoint = col.ClosestPoint(origin);

            Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(DAMAGE_AB, DamageType.Melee, DamageElement.Light));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, playerMotor.transform.position, id);

            // Particle effect
            lightMotor.ImpactPE(impactPoint);
        }

        private void ApplyDamageC()
        {
            int id = Random.Range(int.MinValue, int.MaxValue);

            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                if (pair.Key.tag == "Enemy")
                {
                    Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(DAMAGE_C, DamageType.AreaOfEffect, DamageElement.Light));
                    pair.Key.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, pair.Value, id);
                }
            }
        }

        public override void OnColliderEnter(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy") && col.GetComponent<DamageTaker>() != null && !encounteredCols.ContainsKey(col))
            {
                if (currentAttack < 3)
                {
                    encounteredCols.Add(col, playerMotor.transform.position + Vector3.up);
                }
                else
                {
                    encounteredCols.Add(col, act.transform.position);
                }
            }
        }
    }
}