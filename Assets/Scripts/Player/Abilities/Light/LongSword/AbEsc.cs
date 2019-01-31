using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Tools;
using UnityEngine;

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
        private const float ABILITY_DURATION = 42f / 60f;
        private const float LANDING_TIME = 35f / 60f;
        private const float DAMAGE_TIME = 39f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = 0f;
        private const float DAMAGE = 8f;

        private const float MAX_RANGE = 15f;
        private const float HEIGHT = 5f;

        // Prefabs
        private GameObject lightSpawnEffetPrefab;
        private GameObject lightZonePrefab;
        private GameObject triggerPrefab;
        private GameObject trailEffectPrefab;

        // GameObjects
        private GameObject trigger;
        private Transform characterContainer;
        GameObject landingIndicator;
        private LightSword sword;

        // Indicators
        private GameObject landingIndicatorPrefab;
        private GameObject rangeIndicatorPrefab;

        // Move data
        private Vector3 destination, origin;
        float landingTime;
        float damageTime;
        private bool landed;
        private bool lightSpawned;

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        public AbEsc(Character character, LightSword sword) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            lightZonePrefab = Resources.Load("Player/Light/LightZone/LightZone") as GameObject;
            lightSpawnEffetPrefab = Resources.Load("Player/Light/LongSword/Ab1/LightSpawnEffect") as GameObject;
            landingIndicatorPrefab = Resources.Load("Player/Light/LongSword/AbEsc/AbEscLandingIndicator") as GameObject;
            rangeIndicatorPrefab = Resources.Load("Player/Light/LongSword/AbEsc/AbEscRangeIndicator") as GameObject;
            triggerPrefab = Resources.Load("Player/Light/LongSword/Ab1/Ab1c") as GameObject;
            trailEffectPrefab = Resources.Load("Player/Light/LongSword/Sword/JumpTrail") as GameObject;

            characterContainer = character.gameObject.transform.Find("CharacterContainer");
        }

        public override void StartChanneling()
        {
            if (!JumpIntialisationValid())
            {
                return;
            }

            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            character.animator.Play("BotAbEsc");
            character.animator.Play("TopAbEsc");

            LayerTools.recSetLayer(character.gameObject, PLAYER_LAYER, NO_COLLISION_LAYER);

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

            if ((GameManager.gm.worldMousePoint - character.transform.position).magnitude < MAX_RANGE)
            {
                destination = GameManager.gm.worldMousePoint;
            }
            else
            {
                destination = character.transform.position + (GameManager.gm.worldMousePoint - character.transform.position).normalized * MAX_RANGE;
            }

            landingIndicator.transform.position = new Vector3(destination.x, .2f, destination.z);
        }

        private void DisplayIndicator()
        {
            landingIndicator = GameObject.Instantiate(landingIndicatorPrefab, characterContainer);
            GameObject rangeIndicator = GameObject.Instantiate(rangeIndicatorPrefab, characterContainer);
            GameObject.Destroy(rangeIndicator, channelDuration);
        }

        public override void StartAbility()
        {
            base.StartAbility();

            landingTime = Time.time + LANDING_TIME;
            damageTime = Time.time + DAMAGE_TIME;

            GameObject.Destroy(landingIndicator);

            GameObject trailEffect1 = GameObject.Instantiate(trailEffectPrefab, sword.transform);
            trailEffect1.transform.localPosition = new Vector3(-0.473f, 0.089f, 0f);
            GameObject.Destroy(trailEffect1, ABILITY_DURATION);
            GameObject trailEffect2 = GameObject.Instantiate(trailEffectPrefab, sword.transform);
            trailEffect1.transform.localPosition = new Vector3(0.177f, 0.094f, 0f);
            GameObject.Destroy(trailEffect2, ABILITY_DURATION);

            // No movement
            character.abilityMoveMultiplicator = 0f;
            character.abilityMaxRotation = 0f;

            character.SetMovementMode(MovementMode.Ability);

            ComputeOriginAndDestination();
        }

        private void ComputeOriginAndDestination()
        {
            origin = character.transform.position;

            destination = destination - characterContainer.forward;
            // TODO environment collision detection.
        }

        public override void Cast()
        {
            if (Time.time < landingTime)
            {
                // movement
                character.transform.position = PositionOverTime(Time.time - castStartTime);
            }
            else if (!landed)
            {
                character.SetMovementMode(MovementMode.Player);
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
            return t * (1 - t) * 4 * HEIGHT;
        }

        private void SpawnLight()
        {
            Vector3 pos = character.transform.position + characterContainer.forward;
            pos.y = .2f;

            GameObject lightZone = GameObject.Instantiate(lightZonePrefab, null);
            lightZone.transform.position = pos;

            // Particle effect
            GameObject lightSpawn = GameObject.Instantiate(lightSpawnEffetPrefab, null);
            lightSpawn.transform.position = pos;
            GameObject.Destroy(lightSpawn, 1f);

            // Damage zone (trigger)
            trigger = GameObject.Instantiate(triggerPrefab, null);
            trigger.transform.position = pos;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        public override void End()
        {
            base.End();

            LayerTools.recSetLayer(character.gameObject, NO_COLLISION_LAYER, PLAYER_LAYER);

            ApplyDamage();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            character.SetMovementMode(MovementMode.Player);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            LayerTools.recSetLayer(character.gameObject, NO_COLLISION_LAYER, PLAYER_LAYER);

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            character.SetMovementMode(MovementMode.Player);
        }

        private void ApplyDamage()
        {
            int id = Random.Range(int.MinValue, int.MaxValue);

            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE, DamageType.AreaOfEffect, DamageElement.Light));
                pair.Key.GetComponent<DamageTaker>().TakeDamage(dmg, character, pair.Value, id);
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