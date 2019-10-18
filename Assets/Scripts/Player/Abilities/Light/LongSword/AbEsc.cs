using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Player.Class;
using UnityEngine;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbEsc : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = false;
        private const bool CASTING_CANCELLABLE = false;
        private const bool PARALLELIZABLE = false;

        // const
        private const float COOLDOWN_DURATION = 12f;
        private const float CHANNELING_DURATION = 6f / 60f;
        public const float ABILITY_DURATION = 42f / 60f;
        private const float LANDING_TIME = 35f / 60f;
        private const float DAMAGE_TIME = 39f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = 0f;
        private const float DAMAGE = 8f;

        private const float MAX_RANGE = 15f;
        private const float HEIGHT = 5f;

        // GameObjects
        private GameObject trigger;
        public GameObject landingIndicator;

        // Move data
        private Vector3 destination, origin;
        float landingTime;
        float damageTime;
        private bool landed;
        private bool lightSpawned;

        // Inherited motor
        LightLongSwordMotor lightMotor;

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        public AbEsc(LightLongSwordMotor playerMotor, int id) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, playerMotor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE, PARALLELIZABLE, id)
        {
            lightMotor = playerMotor;
        }

        public override bool CanStart()
        {
            return CanStartEsc();
        }

        public override void StartChanneling()
        {
            base.StartChanneling();
            playerMotor.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            lightMotor.animator.Play("BotAbEsc");
            lightMotor.animator.Play("TopAbEsc");

            playerMotor.layerManager.CallLayer(LayerManager.PlayerLayer.NoCollision, this);

            // Init
            landed = false;
            lightSpawned = false;
            encounteredCols = new Dictionary<Collider, Vector3>();

            // Indicator
            DisplayIndicator();
        }

        public override void Channel()
        {
            base.Channel();
            GetDestination();
            Vector3 pos = new Vector3(destination.x, .2f, destination.z);

            if (landingIndicator != null)
            {
                landingIndicator.transform.position = pos;
            }
        }

        private void GetDestination()
        {
            if ((playerMotor.pc.pointedWorldPoint - playerMotor.transform.position).magnitude < MAX_RANGE)
            {
                destination = playerMotor.pc.pointedWorldPoint;
            }
            else
            {
                destination = playerMotor.transform.position + (playerMotor.pc.pointedWorldPoint - playerMotor.transform.position).normalized * MAX_RANGE;
            }
        }

        private void DisplayIndicator()
        {
            GetDestination();

            Vector3 pos = new Vector3(destination.x, .2f, destination.z);

            landingIndicator = GameObject.Instantiate(lightMotor.abEscLandingIndicatorPrefab);
            landingIndicator.transform.position = pos;
            GameObject.Destroy(landingIndicator, channelDuration);

            GameObject indicator = GameObject.Instantiate(lightMotor.abEscRangeIndicatorPrefab, playerMotor.characterContainer);
            GameObject.Destroy(indicator, channelDuration);
        }

        public override void StartAbility()
        {
            base.StartAbility();

            landingTime = Time.time + LANDING_TIME;
            damageTime = Time.time + DAMAGE_TIME;

            lightMotor.jumpTrails.Play(true);

            // No movement
            playerMotor.abilityMoveMultiplicator = 0f;
            playerMotor.abilityMaxRotation = 0f;

            playerMotor.SetMovementMode(MovementMode.Ability);

            ComputeOriginAndDestination();
        }

        private void ComputeOriginAndDestination()
        {
            origin = playerMotor.transform.position;

            destination = destination - playerMotor.characterContainer.forward;
            // TODO environment collision detection.
        }

        public override void Cast()
        {
            if (Time.time < landingTime)
            {
                // movement
                playerMotor.transform.position = PositionOverTime(Time.time - castStartTime);
            }
            else if (!landed)
            {
                playerMotor.SetMovementMode(MovementMode.Player);
                playerMotor.layerManager.DiscardLayer(this);
                landed = true;
            }
            else if (Time.time >= damageTime && !lightSpawned)
            {
                SpawnLight();
                lightSpawned = true;
            }

            base.Cast();
        }

        private Vector3 PositionOverTime(float t)
        {
            float x = Mathf.Lerp(origin.x, destination.x, t / LANDING_TIME);
            float z = Mathf.Lerp(origin.z, destination.z, t / LANDING_TIME);
            float y = yFunction(t / LANDING_TIME);
            return new Vector3(x, y, z);
        }

        private float yFunction(float t)
        {
            float y = t * (1 - t) * 4 * HEIGHT;
            if (y < 0)
            {
                return 0;
            }
            else
            {
                return y;
            }
        }

        private void SpawnLight()
        {
            Vector3 pos = playerMotor.transform.position + playerMotor.characterContainer.forward;
            pos.y = .02f;

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

            playerMotor.layerManager.DiscardLayer(this);

            ApplyDamage();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
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

            playerMotor.layerManager.DiscardLayer(this);
            playerMotor.SetMovementMode(MovementMode.Player);
        }

        private void ApplyDamage()
        {
            int id = Random.Range(int.MinValue, int.MaxValue);

            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                Damage dmg = playerMotor.psm.AlterDealtDamage(
                    new Damage(DAMAGE, DamageType.AreaOfEffect, DamageElement.Light, playerMotor.transform.position));
                pair.Key.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, pair.Value, id);
            }
        }

        public override void OnColliderEnter(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy") && col.GetComponent<DamageTaker>() != null && !encounteredCols.ContainsKey(col))
            {
                encounteredCols.Add(col, act.transform.position);
            }
        }

        public override string GetTitle()
        {
            return "Saut iridescent";
        }

        public override string GetDescription()
        {
            return "Saut en avant, infligeant 8 dégâts et créant une zone de lumière à l’atterrissage.";
        }
    }
}