using UnityEngine;

namespace LightBringer
{
    public class RaySpell : Ability
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = true;

        // const
        private const float COOLDOWN_DURATION = 4f;
        private const float ABILITY_DURATION = 3f;
        private const float CHANNELING_DURATION = 1.5f;
        private const float HEIGHT = 1.4f;
        private const float MAX_RANGE = 12f;
        private const float DPS = 12f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .4f;
        private const float CASTING_MOVE_MULTIPLICATOR = 0f;
        private const float CASTING_MAX_ROTATION = 45f;

        private GameObject rayDisplayPrefab;
        private GameObject rayDisplay;
        private GameObject rayBallDisplayPrefab;
        private GameObject rayBallDisplay;

        private Transform charContainer;

        public float range;

        public RaySpell(Character character) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            rayDisplayPrefab = Resources.Load("Abilities/RayDisplay") as GameObject;
            rayBallDisplayPrefab = Resources.Load("Abilities/RayBallDisplay") as GameObject;
        }

        
        public override void StartChanneling()
        {
            channelingTime = 0;
            character.currentChanneling = this;
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelRaySpell");
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
            character.animator.SetBool("startRaySpell", true);

            // Movement restrictions
            character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR;
            character.abilityMaxRotation = CASTING_MAX_ROTATION;

            // Display
            rayDisplay = GameObject.Instantiate(rayDisplayPrefab);
            charContainer = character.gameObject.transform.Find("CharacterContainer");
            rayDisplay.transform.SetParent(charContainer);
            rayDisplay.transform.localPosition = new Vector3(0f, HEIGHT, .7f);
            rayDisplay.transform.localRotation = Quaternion.identity;
            rayBallDisplay = GameObject.Instantiate(rayBallDisplayPrefab);


            character.currentAbility = this;
            character.currentChanneling = null;
            castingTime = 0;
        }

        public override void DoAbility()
        {
            castingTime += Time.deltaTime;

            // raycast
            RaycastHit rch;
            Vector3 start = new Vector3(character.transform.position.x, HEIGHT, character.transform.position.z);
            Vector3 direction = charContainer.forward;
            range = MAX_RANGE;
            int mask = ~(1 << LayerMask.NameToLayer("Player"));
            rayBallDisplay.SetActive(false);
            if (Physics.Raycast(start, direction, out rch, MAX_RANGE + .7f, mask))
            {
                range = rch.distance - .7f;
                if (rch.transform.tag == "Enemy")
                {
                    DamageController dc = rch.transform.GetComponent<DamageController>();
                    dc.TakeDamage(DPS * Time.deltaTime);
                    rayBallDisplay.transform.position = rch.point;
                    rayBallDisplay.SetActive(true);
                }
            }

            // display length
            rayDisplay.transform.localScale = new Vector3(rayDisplay.transform.localScale.x, rayDisplay.transform.localScale.y, range);
            
            
            if (castingTime > castingDuration)
            {
                End();
            }
        }

        public override void End()
        {
            // movement back
            character.canRotate = true;
            character.abilityMoveMultiplicator = 1f;
            character.abilityMaxRotation = -1f;

            // destroy display
            Object.Destroy(rayDisplay);
            Object.Destroy(rayBallDisplay);

            character.currentAbility = null;
            coolDownRemaining = coolDownDuration;

            // animation
            character.animator.SetBool("startRaySpell", false);
        }

        public override void CancelChanelling()
        {
            character.currentChanneling = null;

            // animation
            character.animator.Play("NoAction");
        }
    }
}