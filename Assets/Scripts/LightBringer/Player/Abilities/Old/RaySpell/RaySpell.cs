using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public class RaySpell : Ability
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = true;

        // const
        private const float COOLDOWN_DURATION = 4f;
        private const float ABILITY_DURATION = 3f;
        private const float CHANNELING_DURATION = .6f;
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
            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelRaySpell");
        }

        public override void StartAbility()
        {
            base.StartAbility();

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
        }

        public override void Cast()
        {
            base.Cast();
            
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
                    StatusController dc = rch.transform.GetComponent<StatusController>();
                    Damage dmg = new Damage(DPS * Time.deltaTime, DamageType.RangeInstant, DamageElement.Energy);
                    dc.TakeDamage(dmg, character);
                    rayBallDisplay.transform.position = rch.point;
                    rayBallDisplay.SetActive(true);
                }
            }

            // display length
            rayDisplay.transform.localScale = new Vector3(rayDisplay.transform.localScale.x, rayDisplay.transform.localScale.y, range);
        }

        public override void End()
        {
            base.End();
            DestroyDisplay();

            // animation
            character.animator.SetBool("startRaySpell", false);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();
            DestroyDisplay();
        }

        private void DestroyDisplay()
        {
            Object.Destroy(rayDisplay);
            Object.Destroy(rayBallDisplay);
        }
    }
}