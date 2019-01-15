using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;
using LightBringer.Tools;

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
        private const float COOLDOWN_DURATION = 1f; // TODO 12f
        private const float CHANNELING_DURATION = 6f / 60f;
        private const float ABILITY_DURATION = 42f / 60f;
        private const float LANDING_TIME = 35f / 60f;
        private const float DAMAGE_TIME = 39f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = 0f;
        private const float DAMAGE = 8f;

        private const float MAX_RANGE = 15f;

        // Prefabs
        private GameObject lightSpawnEffetPrefab;
        private GameObject lightZonePrefab;
        private GameObject triggerPrefab;

        // GameObjects
        private GameObject trigger;
        private Transform characterContainer;
        GameObject landingIndicator;

        // Indicators
        private GameObject landingIndicatorPrefab;
        private GameObject rangeIndicatorPrefab;

        // Move data
        private float speed;
        private Vector3 destination;
        float landingTime;
        float damageTime;
        private bool landed;
        private bool lightSpawned;

        // Colliders
        private List<Collider> encounteredCols;

        public AbEsc(Character character) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            lightZonePrefab = Resources.Load("Player/Light/LightZone/LightZone") as GameObject;
            lightSpawnEffetPrefab = Resources.Load("Player/Light/LongSword/Ab1/LightSpawnEffect") as GameObject;
            landingIndicatorPrefab = Resources.Load("Player/Light/LongSword/AbEsc/AbEscLandingIndicator") as GameObject;
            rangeIndicatorPrefab = Resources.Load("Player/Light/LongSword/AbEsc/AbEscRangeIndicator") as GameObject;
            triggerPrefab = Resources.Load("Player/Light/LongSword/Ab1/Ab1c") as GameObject;

            characterContainer = character.gameObject.transform.Find("CharacterContainer");
        }

        public override void StartChanneling()
        {
            if (!JumpIntialisationValid())
            {
                return;
            }
            Debug.Log("Channeling");

            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;

            character.animator.Play("BotAbEsc");
            character.animator.Play("TopAbEsc");

            LayerTools.recSetLayer(character.gameObject, PLAYER_LAYER, NO_COLLISION_LAYER);

            // Init
            landed = false;
            lightSpawned = false;
            encounteredCols = new List<Collider>();

            // Indicator
            DisplayIndicator();
        }

        public override void Channel()
        {
            base.Channel();

            if ((GameManager.gm.lookedPoint - character.transform.position).magnitude < MAX_RANGE)
            {
                destination = GameManager.gm.lookedPoint;
            }
            else
            {
                destination = character.transform.position + (GameManager.gm.lookedPoint - character.transform.position).normalized * MAX_RANGE;
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

            // No movement
            character.abilityMoveMultiplicator = 0f;
            character.abilityMaxRotation = 0f;

            character.SetMovementMode(MovementMode.Ability);

            ComputeDestination();

            speed = ((character.transform.position - destination).magnitude - 1) / LANDING_TIME;
        }

        private void ComputeDestination()
        {
            // TODO environment collision detection.
        }

        public override void Cast()
        {
            base.Cast();

            if (Time.time < landingTime)
            {
                // movement
                character.rb.velocity = characterContainer.forward * speed;
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
            foreach (Collider col in encounteredCols)
            {
                Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE, DamageType.AreaOfEffect, DamageElement.Light));
                col.GetComponent<StatusController>().TakeDamage(dmg, character);
            }
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy") && !encounteredCols.Contains(col))
            {
                encounteredCols.Add(col);
            }
        }
    }
}