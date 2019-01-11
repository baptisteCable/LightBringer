using UnityEngine;
using System.Collections.Generic;

namespace LightBringer.Player.Abilities
{
    public class MeleeAoE1 : Ability
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = true;

        // const
        private const float COOLDOWN_DURATION = 3f;
        private const float ABILITY_DURATION = 3f;
        private const float CHANNELING_DURATION = .3f;
        private const float HEIGHT = 3f;
        private const float MAX_RANGE = 1.5f;
        private const float DPS = 8f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = .3f;
        private const float CASTING_MAX_ROTATION = 0;

        private GameObject abilityTriggerPrefab;
        private GameObject abilityTrigger;
        private GameObject abilityDisplayPrefab;
        private GameObject abilityDisplay;

        private List<StatusController> dcs;

        public MeleeAoE1(Character character) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            abilityTriggerPrefab = Resources.Load("Abilities/MeleeAoE1Trigger") as GameObject;
            abilityDisplayPrefab = Resources.Load("Abilities/MeleeAoE1Display") as GameObject;
        }

        
        public override void StartChanneling()
        {
            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelMeleeAoE1");
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // animation
            character.animator.SetBool("startMeleeAoE1", true);

            // créer le trigger
            abilityTrigger = GameObject.Instantiate(abilityTriggerPrefab);
            abilityTrigger.transform.SetParent(character.gameObject.transform.Find("CharacterContainer"));
            abilityTrigger.transform.localPosition = new Vector3(0f, .1f, 0f);
            abilityTrigger.transform.localRotation = Quaternion.identity;
            abilityTrigger.GetComponent<MeleeAoE1Trigger>().caller = this;

            // Display
            abilityDisplay = GameObject.Instantiate(abilityDisplayPrefab);
            abilityDisplay.transform.SetParent(character.gameObject.transform.Find("CharacterContainer"));
            abilityDisplay.transform.localPosition = new Vector3(0f, -.79f, 0f);
            abilityDisplay.transform.localRotation = Quaternion.identity;

            // Movement restrictions
            character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR;
            character.abilityMaxRotation = CASTING_MAX_ROTATION;

            // Enemy list
            dcs = new List<StatusController>();
        }

        public override void Cast()
        {
            base.Cast();

            foreach(StatusController dc in dcs)
            {
                Damage dmg = new Damage(DPS * Time.deltaTime, DamageType.AreaOfEffect, DamageElement.Energy);
                dc.TakeDamage(dmg, character);
            }
        }

        public override void End()
        {
            base.End();
            DestroyTrigger();

            // animation
            character.animator.SetBool("startMeleeAoE1", false);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();
            DestroyTrigger();
        }

        public void AddEnemyDamageController(StatusController enemyDC)
        {
            dcs.Add(enemyDC);
        }

        public void RemoveEnemyDamageController(StatusController enemyDC)
        {
            dcs.Remove(enemyDC);
        }

        private void DestroyTrigger()
        {
            Object.Destroy(abilityTrigger);
            Object.Destroy(abilityDisplay);
        }
    }
}