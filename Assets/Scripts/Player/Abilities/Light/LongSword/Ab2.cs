﻿using System.Collections.Generic;
using System.Linq;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Player.Class;
using UnityEngine;

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

        private const float STUN_DURATION = .2f;

        // Colliders
        private List<Collider> encounteredCols;
        private Dictionary<Collider, float> newCols;

        // GameObjects
        private GameObject trigger;

        // Misc
        private bool sphereAdded = false;

        // Inherited motor
        LightLongSwordMotor lightMotor;


        public Ab2(LightLongSwordMotor motor, int id) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, motor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE, id)
        {
            lightMotor = motor;
        }

        public override void StartChanneling()
        {
            base.StartChanneling();
            playerMotor.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            lightMotor.CallForAll(LightLongSwordMotor.M_PlayAnimAb2);

            LoadLight();

            encounteredCols = new List<Collider>();

            // Indicator
            lightMotor.CallForAll(LightLongSwordMotor.M_Ab2DisplayIndicators);

            sphereAdded = false;
        }

        private void LoadLight()
        {
            if (!lightMotor.sword.isLoaded)
            {
                Collider[] colliders = Physics.OverlapSphere(playerMotor.transform.position, .5f);
                LightZone closestZone = null;
                float shortestDistance = 10000f;

                foreach (Collider col in colliders)
                {
                    LightZone zone = col.GetComponent<LightZone>();
                    if (zone != null && zone.canBeAbsorbed)
                    {
                        float distance = (playerMotor.transform.position - zone.transform.position).magnitude;
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            closestZone = zone;
                        }
                    }
                }

                if (closestZone != null)
                {
                    closestZone.CallForAll(LightZone.M_Absorb);
                    lightMotor.CallForAll(LightLongSwordMotor.M_LoadSword);
                }
            }
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // No more rotation
            playerMotor.abilityMaxRotation = 0f;

            CreateTrigger();

            playerMotor.SetMovementMode(MovementMode.Ability);

            newCols = new Dictionary<Collider, float>();

            // Trail effect
            lightMotor.CallForAll(LightLongSwordMotor.M_TrailEffect);
        }

        public override void Cast()
        {

            // movement
            playerMotor.AbilityMove(playerMotor.characterContainer.forward * DASH_DISTANCE / ABILITY_DURATION);

            ApplyEffectToNew();

            base.Cast();
        }

        private void CreateTrigger()
        {
            trigger = GameObject.Instantiate(lightMotor.ab2TriggerPrefab, playerMotor.characterContainer);
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

            if (lightMotor.sword.isLoaded)
            {
                lightMotor.CallForAll(LightLongSwordMotor.M_UnloadSword);
            }

            playerMotor.SetMovementMode(MovementMode.Player);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            playerMotor.SetMovementMode(MovementMode.Player);
        }

        // Every frame, apply dmg to colliders from closest to farthest
        private void ApplyEffectToNew()
        {
            int id = Random.Range(int.MinValue, int.MaxValue);

            while (newCols.Count > 0 && !playerMotor.psm.isStunned)
            {
                Collider col = newCols.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
                ApplyEffect(col, id);
                newCols.Remove(col);
            }
        }

        private void ApplyEffect(Collider col, int id)
        {
            DamageTaker dt = col.GetComponent<DamageTaker>();
            if (dt.bouncing)
            {
                // Stun character
                playerMotor.psm.ApplyCrowdControl(
                    new CrowdControl(CrowdControlType.Stun, DamageType.Self, DamageElement.None),
                    STUN_DURATION
                );

                // effect
                dt.TakeDamage(new Damage(DAMAGE_UNLOADED, DamageType.Melee, DamageElement.Light), playerMotor, playerMotor.transform.position, id);

                if (lightMotor.sword.isLoaded)
                {
                    lightMotor.CallForAll(LightLongSwordMotor.M_UnloadSword);
                }
            }
            else
            {
                if (dt.extraDmg)
                {
                    ApplyDamageToExtra(col, id);
                }
                else
                {
                    ApplyDamage(col, id);
                }
            }
        }

        private void ApplyDamageToExtra(Collider col, int id)
        {
            // base damage
            float damageAmount = DAMAGE_UNLOADED;
            if (lightMotor.sword.isLoaded)
            {
                // damage update
                damageAmount = DAMAGE_LOADED;
            }
            Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(damageAmount, DamageType.Melee, DamageElement.Light));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, playerMotor.transform.position, id);
        }

        private void ApplyDamage(Collider col, int id)
        {
            Vector3 impactPoint = col.ClosestPoint(playerMotor.transform.position + Vector3.up);

            // base damage
            float damageAmount = DAMAGE_UNLOADED;
            if (lightMotor.sword.isLoaded)
            {
                // damage update
                damageAmount = DAMAGE_LOADED;

                // Effect
                lightMotor.CallForAll(LightLongSwordMotor.M_LoadedImpactPE, impactPoint);

                // Load Ulti
                LoadUlti();
            }

            // Apply damage
            Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(damageAmount, DamageType.Melee, DamageElement.Light));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, playerMotor.transform.position, id);

            // Effect
            lightMotor.CallForAll(LightLongSwordMotor.M_ImpactPE, impactPoint);
        }

        private void LoadUlti()
        {
            if (!sphereAdded)
            {
                sphereAdded = true;
                lightMotor.CallForAll(LightLongSwordMotor.M_AddUltiSphere);
            }
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy") && col.GetComponent<DamageTaker>() != null && !encounteredCols.Contains(col))
            {
                encounteredCols.Add(col);
                float distance = (col.ClosestPoint(playerMotor.transform.position) - playerMotor.transform.position).magnitude;
                newCols.Add(col, distance);
            }
        }
    }
}