using UnityEngine;

namespace LightBringer
{
    public class Attack1 : Ability
    {
        private const float c_coolDownDuration = .5f;
        private const float c_abilityDuration = 1f;
        private const float c_channelingDuration = .5f;
        private const float c_height = 3f;
        private const float c_maxRange = 1.5f;
        private const bool c_channelingCancellable = true;

        private GameObject abilityTriggerPrefab;
        private GameObject abilityTrigger;
        
        public Attack1(Character character) :
            base(c_coolDownDuration, c_channelingDuration, c_abilityDuration, character, c_channelingCancellable)
        {
            abilityTriggerPrefab = Resources.Load("Abilities/Attack1Trigger") as GameObject;
        }

        
        public override void StartChanneling()
        {
            coolDownRemaining = coolDownDuration;
            channelingTime = 0;
            character.currentChanneling = this;
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
            // No more rotation
            character.canRotate = false;

            // créer le trigger

            abilityTrigger = GameObject.Instantiate(abilityTriggerPrefab);
            abilityTrigger.transform.SetParent(character.gameObject.transform.Find("CharacterContainer"));
            abilityTrigger.transform.localPosition = new Vector3(0f, -.8f, 0f);
            abilityTrigger.transform.localRotation = Quaternion.identity;

            character.currentAbility = this;
            character.currentChanneling = null;
            abilityTime = 0;
        }

        public override void DoAbility()
        {
            abilityTime += Time.deltaTime;
            if (abilityTime > abilityDuration)
            {
                End();
            }
        }

        public override void End()
        {
            // Rotations back
            character.canRotate = true;

            // détruire le trigger
            Object.Destroy(abilityTrigger);

            character.currentAbility = null;
        }

        public override void CancelChanelling()
        {
            character.currentChanneling = null;
        }
    }
}