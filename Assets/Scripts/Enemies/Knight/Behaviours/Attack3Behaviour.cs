using UnityEngine;
using LightBringer.Player;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Enemies.Knight
{
    public class Attack3Behaviour : CollisionBehaviour
    {
        private const float DURATION = 1.817f;
        private const float SPEAR_DMG = 14f;
        private const float SHIELD_DMG = 10f;
        private const float SHIELD_STUN_DURATION = 1f;

        private const float SPEAR_DMG_START = 41f / 60f;
        private const float SPEAR_DMG_STOP = 61f / 60f;
        private const float SHIELD_DMG_START = 76f / 60f;
        private const float SHIELD_DMG_STOP = 96f / 60f;

        // Shield collider to disable
        private GameObject shieldCollider;

        public Attack3Behaviour(KnightMotor enemyMotor, GameObject attack3act1GO, GameObject attack3act2GO, GameObject shieldCollider,
            GameObject indicatora, GameObject indicatorb) : base(enemyMotor)
        {
            actGOs = new GameObject[2];
            actGOs[0] = attack3act1GO;
            actGOs[1] = attack3act2GO;
            this.shieldCollider = shieldCollider;
            parts = new Part[2];
            parts[0] = new Part(State.Before, SPEAR_DMG_START, SPEAR_DMG_STOP - SPEAR_DMG_START, indicatora);
            parts[1] = new Part(State.Before, SHIELD_DMG_START, SHIELD_DMG_STOP - SHIELD_DMG_START, indicatorb);
        }

        public override void Init()
        {
            base.Init();

            em.anim.SetBool("castingAttack3", true);
            em.anim.Play("Attack3");
            
            acts = new AbilityColliderTrigger[2];
            acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();
            acts[1] = actGOs[1].GetComponent<AbilityColliderTrigger>();
        }

        public override void Run()
        {
            DisplayIndicators();
            StartCollisionParts();
            RunCollisionParts();

            if (Time.time > startTime + DURATION)
            {
                End();
            }
        }

        protected override void StartCollisionPart(int i)
        {
            if (i == 0)
            {
                shieldCollider.SetActive(false);
            }

            base.StartCollisionPart(i);
        }

        public override void End()
        {
            base.End();
            shieldCollider.SetActive(true);
            em.anim.SetBool("castingAttack3", false);
            em.SetOverrideAgent(false);
        }

        public override void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (abilityColliderTrigger == acts[0].GetComponent<AbilityColliderTrigger>())
                {
                    DamagePart0(col);
                }

                if (abilityColliderTrigger == acts[1].GetComponent<AbilityColliderTrigger>())
                {
                    DamagePart1(col);
                }
            }
        }

        private void DamagePart0(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(SPEAR_DMG, DamageType.AreaOfEffect, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em))
            {
                psm.TakeDamage(dmg, em);
            }
        }

        private void DamagePart1(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(SHIELD_DMG, DamageType.AreaOfEffect, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em))
            {
                psm.TakeDamage(dmg, em);
            }
        }

        public override void Abort()
        {
            shieldCollider.SetActive(true);
            base.Abort();
        }
    }
}