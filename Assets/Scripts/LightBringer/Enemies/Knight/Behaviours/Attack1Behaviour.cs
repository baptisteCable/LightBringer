using UnityEngine;
using LightBringer.Player;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Enemies.Knight
{
    public class Attack1Behaviour : KnightBehaviour, CollisionAbility
    {
        private const float DURATION = 2.315f;
        private const float CHARGE_RANGE = 20f;

        private const float POS_CHECKPOINT_1_START = 38f / 60f;
        private const float POS_CHECKPOINT_1_END = 46f / 60f;
        private const float POS_CHECKPOINT_2_START = 60f / 60f;
        private const float POS_CHECKPOINT_2_END = 70f / 60f;

        private const float DMG_CHECKPOINT_1_START = 26f / 60f;
        private const float DMG_CHECKPOINT_1_END = 29f / 60f;
        private const float DMG_CHECKPOINT_2_START = 56f / 60f;
        private const float DMG_CHECKPOINT_2_END = 58f / 60f;
        private const float DMG_CHECKPOINT_3_START = 76f / 60f;
        private const float DMG_CHECKPOINT_3_END = 118f / 60f;

        // Colliders GO
        public GameObject act1GO;
        public GameObject act2GO;
        public GameObject act3GO;
        private AbilityColliderTrigger act1;
        private AbilityColliderTrigger act2;
        private AbilityColliderTrigger act3;

        // Indicators
        private GameObject indicator1, indicator2, indicator3;

        // Collider list
        private List<Collider> cols;

        // Init booleans
        private bool part1Initialized = false;
        private bool part2Initialized = false;
        private bool part3Initialized = false;

        float stopDist;
        Transform target;

        float ellapsedTime = 0f;

        public Attack1Behaviour(KnightMotor enemyMotor, Transform target, GameObject attack1act1GO,
            GameObject attack1act2GO, GameObject attack1act3GO,
            GameObject indicator1, GameObject indicator2, GameObject indicator3) : base(enemyMotor)
        {
            this.target = target;
            act1GO = attack1act1GO;
            act2GO = attack1act2GO;
            act3GO = attack1act3GO;
            this.indicator1 = indicator1;
            this.indicator2 = indicator2;
            this.indicator3 = indicator3;
        }

        public override void Init()
        {
            em.anim.SetBool("castingAttack1", true);
            em.anim.Play("Attack1");
            act1 = act1GO.GetComponent<AbilityColliderTrigger>();
            act2 = act2GO.GetComponent<AbilityColliderTrigger>();
            act3 = act3GO.GetComponent<AbilityColliderTrigger>();

            // Indicator 1
            indicator1.SetActive(true);
        }

        public override void Run()
        {
            ellapsedTime += Time.deltaTime;

            // DMG 1
            if (ellapsedTime >= DMG_CHECKPOINT_1_START && ellapsedTime <= DMG_CHECKPOINT_1_END)
            {
                InitPart1();
            }
            if (act1GO.activeSelf && ellapsedTime >= DMG_CHECKPOINT_1_END)
            {
                act1GO.SetActive(false);
                act1.UnsetAbility();

                // Indicator 2
                indicator2.SetActive(true);
            }

            // DMG 2
            if (ellapsedTime >= DMG_CHECKPOINT_2_START && ellapsedTime <= DMG_CHECKPOINT_2_END)
            {
                InitPart2();
            }
            if (act2GO.activeSelf && ellapsedTime >= DMG_CHECKPOINT_2_END)
            {
                act2GO.SetActive(false);
                act2.UnsetAbility();

                // Indicator 3
                indicator3.SetActive(true);
            }

            // DMG 3
            if (ellapsedTime >= DMG_CHECKPOINT_3_START && ellapsedTime <= DMG_CHECKPOINT_3_END)
            {
                InitPart3();

                em.Move(em.transform.forward * CHARGE_RANGE / (DMG_CHECKPOINT_3_END - DMG_CHECKPOINT_3_START));
            }
            if (act3GO.activeSelf && ellapsedTime >= DMG_CHECKPOINT_3_END)
            {
                act3GO.SetActive(false);
                act3.UnsetAbility();
            }

            // POS init, 1 AND 2
            if (ellapsedTime <= DMG_CHECKPOINT_1_START || (ellapsedTime >= POS_CHECKPOINT_1_START && ellapsedTime <= POS_CHECKPOINT_1_END))
            {
                em.RotateTowards(target.position - em.transform.position);
            }

            if (ellapsedTime > DURATION)
            {
                End();
            }
        }

        public void End()
        {
            em.anim.SetBool("castingAttack1", false);
            complete = true;
            em.SetOverrideAgent(false);
        }

        public void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (abilityColliderTrigger == act1GO.GetComponent<AbilityColliderTrigger>())
                {
                    ApplyPart1Damage(col);
                }

                if (abilityColliderTrigger == act2GO.GetComponent<AbilityColliderTrigger>())
                {
                    ApplyPart2Damage(col);
                }

                if (abilityColliderTrigger == act3GO.GetComponent<AbilityColliderTrigger>())
                {
                    ApplyPart3Damage(col);
                }
            }
        }

        private void ApplyPart1Damage(Collider col)
        {
            if (!cols.Contains(col))
            {
                cols.Add(col);
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                Damage dmg = new Damage(15f, DamageType.Melee, DamageElement.Physical);
                if (psm.IsAffectedBy(dmg, em, em.transform.position))
                {
                    psm.TakeDamage(dmg, em, em.transform.position);
                }
            }
        }

        private void ApplyPart2Damage(Collider col)
        {
            if (!cols.Contains(col))
            {
                cols.Add(col);
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                Damage dmg = new Damage(5f, DamageType.AreaOfEffect, DamageElement.Physical);
                if (psm.IsAffectedBy(dmg, em))
                {
                    psm.TakeDamage(dmg, em);
                    psm.Stun(1f);
                }
            }
        }

        private void ApplyPart3Damage(Collider col)
        {
            if (!cols.Contains(col))
            {
                cols.Add(col);
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                Damage dmg = new Damage(25f, DamageType.Melee, DamageElement.Physical);
                if (psm.IsAffectedBy(dmg, em))
                {
                    psm.TakeDamage(dmg, em);
                    psm.Interrupt(1f);
                }
            }
        }

        private void InitPart1()
        {
            if (!part1Initialized)
            {
                // Indicator 1
                indicator1.SetActive(false);

                act1GO.SetActive(true);
                act1.SetAbility(this);
                cols = new List<Collider>();
                part1Initialized = true;
            }
        }

        private void InitPart2()
        {
            if (!part2Initialized)
            {
                // Indicator 2
                indicator2.SetActive(false);

                act2GO.SetActive(true);
                act2.SetAbility(this);
                cols = new List<Collider>();
                part2Initialized = true;
            }
        }

        private void InitPart3()
        {
            if (!part3Initialized)
            {
                // Indicator 3
                indicator3.SetActive(false);

                em.SetOverrideAgent(true);
                act3GO.SetActive(true);
                act3.SetAbility(this);
                cols = new List<Collider>();
                part3Initialized = true;
            }
        }
    }
}