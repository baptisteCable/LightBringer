﻿using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Player.Class;
using UnityEngine;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbOff : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;
        private const bool PARALLELIZABLE = false;

        // const
        public const float COOLDOWN_DURATION_A = 10f;
        public const float COOLDOWN_DURATION_B = .1f;
        public const float CHANNELING_DURATION_A = 12f / 60f;
        public const float CHANNELING_DURATION_B = 10f / 60f;
        private const float ABILITY_DURATION_A = 6f / 60f;
        private const float ABILITY_DURATION_B = 6f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = .3f;
        private const float DAMAGE = 6f;

        private const float VANISH_DURATION = 1f;

        private const float STUN_DURATION = .6f;

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        // GameObjects
        private GameObject trigger;

        // Status
        private int currentAttack = 1;
        private bool vanished = false;
        private float forcedFadeInTime;

        // Respawn point relatively to enemy center
        Transform spawnPoint;

        // Inherited motor
        LightLongSwordMotor lightMotor;

        public AbOff(LightLongSwordMotor motor, int id) :
            base(COOLDOWN_DURATION_A, CHANNELING_DURATION_A, ABILITY_DURATION_A, motor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE, PARALLELIZABLE, id)
        {
            lightMotor = motor;
        }

        public override void StartChanneling()
        {
            base.StartChanneling();
            playerMotor.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            encounteredCols = new Dictionary<Collider, Vector3>();

            if (!vanished)
            {
                currentAttack = 1;
                lightMotor.animator.Play("BotAbOffa");
                lightMotor.animator.Play("TopAbOffa");
                channelDuration = CHANNELING_DURATION_A;

                // Indicator
                GameObject indicator = GameObject.Instantiate(lightMotor.abOffIndicatorPrefab, lightMotor.characterContainer);
                GameObject.Destroy(indicator, AbOff.CHANNELING_DURATION_A);
                indicators.Add(indicator);                
            }
            else
            {
                currentAttack = 2;

                // No more rotation
                playerMotor.abilityMaxRotation = 0f;

                lightMotor.animator.Play("BotAbOffb");
                lightMotor.animator.Play("TopAbOffb");
                channelDuration = AbOff.CHANNELING_DURATION_B;

                FadeIn();
            }
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // No more rotation
            playerMotor.abilityMaxRotation = 0f;

            // Trail effect
            SlashEffect();

            CreateTrigger();

        }

        private void SlashEffect()
        {
            if (currentAttack == 1)
            {
                lightMotor.abOffaSlash.Play();
            }
            else
            {
                lightMotor.abOffbSlash.Play();
            }
        }

        private void FadeIn()
        {
            // Positionning character
            playerMotor.SetMovementMode(MovementMode.Player);
            playerMotor.transform.position = spawnPoint.position;
            playerMotor.characterContainer.rotation = spawnPoint.rotation;

            // Effect
            Vector3 pos = playerMotor.transform.position;

            GameObject effect = GameObject.Instantiate(lightMotor.fadeInEffetPrefab);
            effect.transform.position = pos;
            GameObject.Destroy(effect, .3f);
            GameObject lightColumn = GameObject.Instantiate(lightMotor.lightColumnPrefab);
            lightColumn.transform.position = pos;
            GameObject.Destroy(lightColumn, .5f);

            // Long cooldown
            coolDownDuration = AbOff.COOLDOWN_DURATION_A;

            // unlock other abilities
            playerMotor.LockAbilitiesExcept(false, this);

            // Reset state
            playerMotor.psm.isTargetable = true;
            vanished = false;

            // Special cancel
            playerMotor.specialCancelAbility = null;
        }

        private void FadeOut(Collider col)
        {
            // Fade in position and rotation
            Transform enemyTransform = col.GetComponent<DamageTaker>().statusManager.transform;
            spawnPoint = enemyTransform.GetComponent<BackSpawn>().backSpawPoint;

            // effect
            Vector3 pos = playerMotor.transform.position;
            GameObject effect = GameObject.Instantiate(lightMotor.fadeOutEffetPrefab);
            effect.transform.position = pos;
            GameObject.Destroy(effect, .2f);
            GameObject lightColumn = GameObject.Instantiate(lightMotor.lightColumnPrefab);
            lightColumn.transform.position = pos;
            GameObject.Destroy(lightColumn, .5f);

            // Lock other abilities
            playerMotor.LockAbilitiesExcept(true, this);

            // short CD and set fadeIn time
            coolDownDuration = AbOff.COOLDOWN_DURATION_B;

            // Anchor character
            playerMotor.MergeWith(enemyTransform);
            playerMotor.psm.isTargetable = false;
            vanished = true;

            // short CD and set fadeIn time
            forcedFadeInTime = Time.time + VANISH_DURATION;

            // Special cancel
            playerMotor.specialCancelAbility = this;
        }

        private void CreateTrigger()
        {
            trigger = GameObject.Instantiate(lightMotor.abOffTriggerPrefab, playerMotor.characterContainer);
            trigger.transform.localPosition = new Vector3(0f, .1f, 0f);
            trigger.transform.localRotation = Quaternion.identity;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        public override void End()
        {
            if (currentAttack == 1)
            {
                Collider col = ApplyAllDamage();
                if (col)
                {
                    FadeOut(col);
                }
            }
            else
            {
                ApplyAllDamage();
            }

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            base.End();
        }

        private Collider ApplyAllDamage()
        {
            Collider closestCol = null;
            float minDist = float.PositiveInfinity;
            float dist;

            int id = Random.Range(int.MinValue, int.MaxValue);

            // find the closest collider (and the extraDamage ones)
            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                // Extra damage: deal damage
                DamageTaker dt = pair.Key.GetComponent<DamageTaker>();
                if (dt != null && dt.extraDmg)
                {
                    ApplyDamage(pair.Key, pair.Value, id);
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

            if (closestCol != null)
            {
                ApplyDamage(closestCol, encounteredCols[closestCol], id);
                return closestCol;
            }

            return null;
        }

        private void ApplyDamage(Collider col, Vector3 origin, int id)
        {
            Vector3 impactPoint = col.ClosestPoint(origin);

            Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(DAMAGE, DamageType.Melee, DamageElement.Light, impactPoint));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, origin, id);

            // Impact effect
            lightMotor.ImpactPE(impactPoint);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            FadeIn();
        }

        public override void OnColliderEnter(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy") && col.GetComponent<DamageTaker>() != null && !encounteredCols.ContainsKey(col))
            {
                encounteredCols.Add(col, playerMotor.transform.position + Vector3.up);
            }
        }

        public override void ComputeSpecial()
        {
            if (vanished && Time.time > forcedFadeInTime)
            {
                StartChanneling();
            }
        }

        public override void SpecialCancel()
        {
            StartChanneling();
        }

        public override string GetTitle()
        {
            return "Evaporation";
        }

        public override string GetDescription()
        {
            return "Attaque rapide infligeant 6 dégâts et faisant disparaître le personnage.\n\nAu bout d’une seconde ou lors de la réactivation, réapparaît derrière la cible et effectue une seconde attaque de 6 dégâts.";
        }
    }
}