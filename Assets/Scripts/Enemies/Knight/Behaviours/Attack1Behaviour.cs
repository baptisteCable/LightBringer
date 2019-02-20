using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack1Behaviour : CollisionBehaviour
    {
        private const float DURATION = 2.9f;
        private const float CHARGE_RANGE = 20f;

        private const float DMG_CHECKPOINT_1_START = 60f / 60f;
        private const float DMG_CHECKPOINT_1_END = 63f / 60f;
        private const float POS_CHECKPOINT_1_START = 72f / 60f;
        private const float POS_CHECKPOINT_1_END = 80f / 60f;
        private const float DMG_CHECKPOINT_2_START = 90f / 60f;
        private const float DMG_CHECKPOINT_2_END = 92f / 60f;
        private const float DMG_CHECKPOINT_3_START = 110f / 60f;
        private const float DMG_CHECKPOINT_3_END = 152f / 60f;

        float stopDist;
        Transform target;

        public Attack1Behaviour(KnightMotor enemyMotor, Transform target, GameObject attack1act1GO,
            GameObject attack1act2GO, GameObject attack1act3GO,
            GameObject indicator1, GameObject indicator2, GameObject indicator3) : base(enemyMotor)
        {
            this.target = target;
            actGOs = new GameObject[3];
            actGOs[0] = attack1act1GO;
            actGOs[1] = attack1act2GO;
            actGOs[2] = attack1act3GO;
            parts = new Part[3];
            parts[0] = new Part(State.Before, DMG_CHECKPOINT_1_START, DMG_CHECKPOINT_1_END - DMG_CHECKPOINT_1_START, indicator1);
            parts[1] = new Part(State.Before, DMG_CHECKPOINT_2_START, DMG_CHECKPOINT_2_END - DMG_CHECKPOINT_2_START, indicator2);
            parts[2] = new Part(State.Before, DMG_CHECKPOINT_3_START, DMG_CHECKPOINT_3_END - DMG_CHECKPOINT_3_START, indicator3);

        }

        public override void Init()
        {
            base.Init();

            em.anim.SetBool("castingAttack1", true);
            em.anim.Play("Attack1");
            acts = new AbilityColliderTrigger[3];
            for (int i = 0; i < actGOs.Length; i++)
            {
                acts[i] = actGOs[i].GetComponent<AbilityColliderTrigger>();
            }
        }

        public override void Run()
        {
            DisplayIndicators();
            StartCollisionParts();
            RunCollisionParts();

            // Rotate at some times
            if (Time.time <= startTime + DMG_CHECKPOINT_1_START ||
                (Time.time >= startTime + POS_CHECKPOINT_1_START && Time.time <= startTime + POS_CHECKPOINT_1_END))
            {
                em.RotateTowards(target.position);
            }

            if (Time.time > startTime + DURATION)
            {
                End();
            }
        }

        protected override void StartCollisionPart(int i)
        {
            if (i == 2)
            {
                // Effect
                actGOs[2].transform.parent.Find("ChargeEffect").GetComponent<ParticleSystem>().Play();
            }

            base.StartCollisionPart(i);
        }

        protected override void RunCollisionPart(int part)
        {
            if (part == 2)
            {
                em.Move(em.transform.forward * CHARGE_RANGE / (DMG_CHECKPOINT_3_END - DMG_CHECKPOINT_3_START));
            }

            base.RunCollisionPart(part);
        }

        public override void End()
        {
            base.End();
            em.anim.SetBool("castingAttack1", false);
            em.SetOverrideAgent(false);
        }

        public override void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player" && !cols.Contains(col))
            {
                cols.Add(col);

                if (abilityColliderTrigger == actGOs[0].GetComponent<AbilityColliderTrigger>())
                {
                    ApplyPart0Damage(col);
                }

                if (abilityColliderTrigger == actGOs[1].GetComponent<AbilityColliderTrigger>())
                {
                    ApplyPart1Damage(col);
                }

                if (abilityColliderTrigger == actGOs[2].GetComponent<AbilityColliderTrigger>())
                {
                    ApplyPart2Damage(col);
                }
            }
        }

        private void ApplyPart0Damage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(15f, DamageType.Melee, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
            }

        }

        private void ApplyPart1Damage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(5f, DamageType.AreaOfEffect, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em);
                psm.ApplyCrowdControl(new CrowdControl(CrowdControlType.Stun, DamageType.Melee, DamageElement.Physical), 1f);
            }
        }

        private void ApplyPart2Damage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(25f, DamageType.Melee, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em);
                psm.ApplyCrowdControl(new CrowdControl(CrowdControlType.Stun, DamageType.Melee, DamageElement.Physical), 1f);
            }
        }

        public override void Abort()
        {
            foreach (Part part in parts)
            {
                part.indicator.SetActive(false);
            }

            base.Abort();
        }
    }
}