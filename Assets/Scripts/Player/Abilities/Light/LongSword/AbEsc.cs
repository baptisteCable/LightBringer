using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Player.Class;
using LightBringer.Tools;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbEsc : CollisionPlayerAbility
    {
        private const string NO_COLLISION_LAYER = "NoCollision";
        private const string PLAYER_LAYER = "Player";

        // cancelling const
        private const bool CHANNELING_CANCELLABLE = false;
        private const bool CASTING_CANCELLABLE = false;

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
        private GameObject landingIndicator;

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
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, playerMotor, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE, id)
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

            lightMotor.CallForAll(LightLongSwordMotor.M_PlayAbEsc);

            LayerTools.recSetLayer(playerMotor.gameObject, PLAYER_LAYER, NO_COLLISION_LAYER);

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

            if ((playerMotor.pc.pointedWorldPoint - playerMotor.transform.position).magnitude < MAX_RANGE)
            {
                destination = playerMotor.pc.pointedWorldPoint;
            }
            else
            {
                destination = playerMotor.transform.position + (playerMotor.pc.pointedWorldPoint - playerMotor.transform.position).normalized * MAX_RANGE;
            }

            landingIndicator.transform.position = new Vector3(destination.x, .2f, destination.z);
        }

        private void DisplayIndicator()
        {
            landingIndicator = GameObject.Instantiate(lightMotor.abEscLandingIndicatorPrefab, playerMotor.characterContainer);
            GameObject rangeIndicator = GameObject.Instantiate(lightMotor.abEscRangeIndicatorPrefab, playerMotor.characterContainer);
            GameObject.Destroy(rangeIndicator, channelDuration);
        }

        public override void StartAbility()
        {
            base.StartAbility();

            landingTime = Time.time + LANDING_TIME;
            damageTime = Time.time + DAMAGE_TIME;

            GameObject.Destroy(landingIndicator);

            lightMotor.CallForAll(LightLongSwordMotor.M_EscTrails);

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
            pos.y = .2f;

            GameObject lightZone = GameObject.Instantiate(lightMotor.lightZonePrefab, null);
            lightZone.transform.position = pos;
            NetworkServer.Spawn(lightZone);

            // Particle effect
            lightMotor.CallForAll(LightLongSwordMotor.M_LightSpawnPE, pos);

            // Damage zone (trigger)
            trigger = GameObject.Instantiate(lightMotor.lightSpawnTriggerPrefab, null);
            trigger.transform.position = pos;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        public override void End()
        {
            base.End();

            LayerTools.recSetLayer(playerMotor.gameObject, NO_COLLISION_LAYER, PLAYER_LAYER);

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

            LayerTools.recSetLayer(playerMotor.gameObject, NO_COLLISION_LAYER, PLAYER_LAYER);

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            playerMotor.SetMovementMode(MovementMode.Player);
        }

        private void ApplyDamage()
        {
            int id = Random.Range(int.MinValue, int.MaxValue);

            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                Damage dmg = playerMotor.psm.AlterDealtDamage(new Damage(DAMAGE, DamageType.AreaOfEffect, DamageElement.Light));
                pair.Key.GetComponent<DamageTaker>().TakeDamage(dmg, playerMotor, pair.Value, id);
            }
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy") && col.GetComponent<DamageTaker>() != null && !encounteredCols.ContainsKey(col))
            {
                encounteredCols.Add(col, act.transform.position);
            }
        }
    }
}