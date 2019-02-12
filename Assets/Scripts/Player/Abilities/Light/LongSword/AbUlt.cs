using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Player.Class;
using UnityEngine;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbUlt : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION = 4f;
        private const float ABILITY_DURATION = 6f / 60f;
        private const float CHANNELING_DURATION = 30f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = .7f;

        private const float STUN_DURATION = .2f;
        private const float DAMAGE = 10f;
        private const float EXTRA_DAMAGE_TAKER_DURATION = 10f;

        private const float SWORD_LOADED_TIME = 16f / 60f;

        // Action time bool
        private bool swordLoaded = false;

        // Trigger
        private GameObject trigger;

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        // Inherited motor
        LightLongSwordMotor lightMotor;

        public AbUlt(LightLongSwordMotor motor) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, motor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            lightMotor = motor;
        }

        public override void StartChanneling()
        {
            base.StartChanneling();
            playerMotor.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            playerMotor.animator.Play("BotUlt");
            playerMotor.animator.Play("TopUlt");

            encounteredCols = new Dictionary<Collider, Vector3>();

            // Indicator
            DisplayIndicator();

            // Loading Sword animation
            ((LightLongSwordMotor)playerMotor).LoadSwordWithSpheres();

            // Action Time bool
            swordLoaded = false;
        }

        private void DisplayIndicator()
        {
            GameObject indicator = GameObject.Instantiate(lightMotor.abOffIndicatorPrefab, playerMotor.characterContainer);
            GameObject.Destroy(indicator, channelDuration);
            indicators.Add(indicator);
        }

        public override void Channel()
        {
            base.Channel();

            if (Time.time > channelStartTime + SWORD_LOADED_TIME && !swordLoaded)
            {
                swordLoaded = true;
                lightMotor.sword.transform.Find("UltLoaded").gameObject.SetActive(true);
            }
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // No more rotation
            playerMotor.abilityMaxRotation = 0f;

            playerMotor.animator.Play("BotAbOffb");
            playerMotor.animator.Play("TopAbOffb");


            // Trail effect
            lightMotor.sword.transform.Find("FxTrail").GetComponent<ParticleSystem>().Play();

            CreateTrigger();

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
            bool spheresConsumed = ApplyAllDamage();

            lightMotor.sword.transform.Find("UltLoaded").gameObject.SetActive(false);

            if (!spheresConsumed)
            {
                ((LightLongSwordMotor)playerMotor).CancelLoadSwordWithSpheres();
            }

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            base.End();
        }

        private bool ApplyAllDamage()
        {
            Collider closestCol = null;
            float minDist = float.PositiveInfinity;
            float dist;

            int id = Random.Range(int.MinValue, int.MaxValue);

            // find the closest collider (and the extraDamage ones)
            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                // Extra damage: ignore
                DamageTaker dt = pair.Key.GetComponent<DamageTaker>();
                if (dt == null || !dt.extraDmg)
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
                DamageTaker dt = closestCol.GetComponent<DamageTaker>();
                if (dt.bouncing)
                {
                    // Stun character
                    playerMotor.psm.ApplyCrowdControl(
                        new CrowdControl(CrowdControlType.Stun, DamageType.Self, DamageElement.None),
                        STUN_DURATION
                    );
                }
                else
                {
                    ApplyDamage(closestCol, encounteredCols[closestCol], id);
                    return true;
                }
            }

            return false;
        }

        private void ApplyDamage(Collider col, Vector3 origin, int id)
        {
            Vector3 impactPoint = col.ClosestPoint(origin);

            Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(DAMAGE, DamageType.Melee, DamageElement.Light));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, origin, id);

            // Spawn Ulti Extra damage taker
            ApplyEffect(col);

            GameObject impactEffect = GameObject.Instantiate(lightMotor.impactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            impactEffect.transform.rotation = Quaternion.LookRotation(playerMotor.transform.position + Vector3.up - impactPoint, Vector3.up);
            GameObject.Destroy(impactEffect, 1f);

            ((LightLongSwordMotor)playerMotor).ConsumeAllSpheres();
        }

        private void ApplyEffect(Collider col)
        {
            // TODO See what happens when monster no capsule-shaped or multi-part
            StatusManager sm = col.GetComponent<DamageTaker>().statusManager;
            Transform target = sm.transform;
            GameObject ultiDTContainer = GameObject.Instantiate(lightMotor.ultiDTprefab);
            Transform ultiDT = ultiDTContainer.transform.Find("DamageTaker");
            ultiDT.localScale = Vector3.one * target.GetComponent<CharacterController>().radius;
            ultiDT.GetComponent<UltDamageTaker>().statusManager = sm;
            GameObject.Destroy(ultiDTContainer, EXTRA_DAMAGE_TAKER_DURATION);
        }

        public override void AbortChanelling()
        {
            base.AbortChanelling();

            ((LightLongSwordMotor)playerMotor).CancelLoadSwordWithSpheres();
            lightMotor.sword.transform.Find("UltLoaded").gameObject.SetActive(false);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            ((LightLongSwordMotor)playerMotor).CancelLoadSwordWithSpheres();
            lightMotor.sword.transform.Find("UltLoaded").gameObject.SetActive(false);
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if (col.tag == "Enemy" && col.GetComponent<DamageTaker>() != null && !encounteredCols.ContainsKey(col))
            {
                encounteredCols.Add(col, playerMotor.transform.position + Vector3.up);
            }
        }
    }
}