using UnityEngine;
using System.Collections.Generic;

namespace LightBringer
{
    public class MeleeAoE1 : Ability
    {
        private const float COOLDOWN_DURATION = 3f;
        private const float ABILITY_DURATION = 3f;
        private const float CHANNELING_DURATION = .8f;
        private const float HEIGHT = 3f;
        private const float MAX_RANGE = 1.5f;
        private const bool CHANNELING_CANCELLABLE = true;
        private const float DPS = 8f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float DOING_MOVE_MULTIPLICATOR = .3f;
        private const float DOING_ROTATION_MULTIPLICATOR = 0;

        private GameObject abilityTriggerPrefab;
        private GameObject abilityTrigger;
        private GameObject abilityDisplayPrefab;
        private GameObject abilityDisplay;

        private List<DamageController> dcs;

        public MeleeAoE1(Character character) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE)
        {
            abilityTriggerPrefab = Resources.Load("Abilities/MeleeAoE1Trigger") as GameObject;
            abilityDisplayPrefab = Resources.Load("Abilities/MeleeAoE1Display") as GameObject;
        }

        
        public override void StartChanneling()
        {
            channelingTime = 0;
            character.currentChanneling = this;
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelMeleeAoE1");
        }

        public override void Channel()
        {
            channelingTime += Time.deltaTime;

            if (channelingTime > channelingDuration)
            {
                StartAbility();
            }
        }

        public override void StartAbility()
        {
            // animation
            character.animator.SetBool("startMeleeAoE1", true);

            // créer le trigger
            abilityTrigger = GameObject.Instantiate(abilityTriggerPrefab);
            abilityTrigger.transform.SetParent(character.gameObject.transform.Find("CharacterContainer"));
            abilityTrigger.transform.localPosition = new Vector3(0f, -.8f, 0f);
            abilityTrigger.transform.localRotation = Quaternion.identity;
            abilityTrigger.GetComponent<MeleeAoE1Trigger>().caller = this;

            // Display
            abilityDisplay = GameObject.Instantiate(abilityDisplayPrefab);
            abilityDisplay.transform.SetParent(character.gameObject.transform.Find("CharacterContainer"));
            abilityDisplay.transform.localPosition = new Vector3(0f, -.79f, 0f);
            abilityDisplay.transform.localRotation = Quaternion.identity;

            // Movement restrictions
            character.abilityMoveMultiplicator = DOING_MOVE_MULTIPLICATOR;
            character.abilityRotationMultiplicator = DOING_ROTATION_MULTIPLICATOR;

            character.currentAbility = this;
            character.currentChanneling = null;
            abilityTime = 0;

            // Enemy list
            dcs = new List<DamageController>();
        }

        public override void DoAbility()
        {
            abilityTime += Time.deltaTime;

            foreach(DamageController dc in dcs)
            {
                dc.TakeDamage(DPS * Time.deltaTime);
            }
            
            if (abilityTime > abilityDuration)
            {
                End();
            }
        }

        public override void End()
        {
            // Movement restrictions
            character.abilityMoveMultiplicator = 1f;
            character.abilityRotationMultiplicator = 1f;

            // détruire le trigger
            Object.Destroy(abilityTrigger);
            Object.Destroy(abilityDisplay);

            character.currentAbility = null;
            coolDownRemaining = coolDownDuration;

            // animation
            character.animator.SetBool("startMeleeAoE1", false);
        }

        public override void CancelChanelling()
        {
            // Movement restrictions
            character.abilityMoveMultiplicator = 1f;

            character.currentChanneling = null;

            // animation
            character.animator.Play("NoAction");
        }

        public void AddEnemyDamageController(DamageController enemyDC)
        {
            dcs.Add(enemyDC);
        }

        public void RemoveEnemyDamageController(DamageController enemyDC)
        {
            dcs.Remove(enemyDC);
        }
    }
}