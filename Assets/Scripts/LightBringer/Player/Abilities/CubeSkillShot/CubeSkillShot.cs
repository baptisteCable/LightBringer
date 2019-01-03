using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Player.Abilities
{
    public class CubeSkillShot : Ability, CollisionAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = true;

        // const
        private const float COOLDOWN_DURATION = 1f;
        private const float ABILITY_DURATION = .1f;
        private const float CHANNELING_DURATION = .3f;
        private const float HEIGHT = 1.4f;
        private const float MAX_RANGE = 25f;
        private const float DAMAGE = 8f;
        private const float PROJECTILE_SPEED = 20f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = 0;

        private GameObject cubePrefab;
        private GameObject cube;

        private List<DamageController> dcs;

        public CubeSkillShot(Character character) :
            base(COOLDOWN_DURATION, CHANNELING_DURATION, ABILITY_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            cubePrefab = Resources.Load("Abilities/CubeSkillShot") as GameObject;
        }

        public override void StartChanneling()
        {
            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            character.animator.Play("ChannelCubeSkillShot");
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // animation
            character.animator.SetBool("startCubeSkillShot", true);

            // cube trigger
            cube = GameObject.Instantiate(cubePrefab);
            Vector3 direction = character.transform.Find("CharacterContainer").transform.forward;
            cube.transform.position = character.transform.position + .7f * direction + new Vector3(0, HEIGHT, 0);
            cube.GetComponent<Rigidbody>().AddForce(PROJECTILE_SPEED * direction, ForceMode.Impulse);
            cube.GetComponent<Rigidbody>().AddTorque(35f * character.transform.Find("CharacterContainer").transform.right, ForceMode.Impulse);
            Object.Destroy(cube, MAX_RANGE / PROJECTILE_SPEED);
            cube.GetComponent<AbilityColliderTrigger>().ForcedStart();
            cube.GetComponent<AbilityColliderTrigger>().SetAbility(this);

            // Movement restrictions
            character.abilityMoveMultiplicator = CASTING_MOVE_MULTIPLICATOR;
        }

        public override void End()
        {
            base.End();

            // animation
            character.animator.SetBool("startCubeSkillShot", false);
        }

        public void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.name == "Character")
            {
                return;
            }

            if (col.tag == "Enemy")
            {
                col.GetComponent<DamageController>().TakeDamage(DAMAGE);
            }
            Object.Destroy(cube);
        }
    }
}