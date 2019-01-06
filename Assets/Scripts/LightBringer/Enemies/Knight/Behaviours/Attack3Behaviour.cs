﻿using UnityEngine;
using LightBringer.Player;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Knight
{
    public class Attack3Behaviour : KnightBehaviour, CollisionAbility
    {
        private const float DURATION = 1.6f;
        private const float SPEAR_DMG = 14f;
        private const float SHIELD_DMG = 10f;
        private const float SHIELD_STUN_DURATION = 1f;

        private const float SPEAR_DMG_START = 25f / 60f;
        private const float SHIELD_DMG_START = 57f / 60f;
        private const float SPEAR_DMG_STOP = 59f / 60f;
        private const float SHIELD_DMG_STOP = 85f / 60f;

        // Colliders GO
        private GameObject act1GO;
        private GameObject act2GO;
        private AbilityColliderTrigger act1;
        private AbilityColliderTrigger act2;
        private GameObject shieldCollider;

        // Init booleans
        private bool part1Initialized = false;
        private bool part2Initialized = false;

        float stopDist;
        Transform target;

        float ellapsedTime = 0f;

        public Attack3Behaviour(KnightMotor enemyMotor, GameObject attack3act1GO, GameObject attack3act2GO, GameObject shieldCollider) : base(enemyMotor)
        {
            act1GO = attack3act1GO;
            act2GO = attack3act2GO;
            this.shieldCollider = shieldCollider;
        }

        public override void Init()
        {
            em.anim.SetBool("castingAttack3", true);
            em.anim.Play("Attack3");
            act1 = act1GO.GetComponent<AbilityColliderTrigger>();
            act2 = act2GO.GetComponent<AbilityColliderTrigger>();
        }

        public override void Run()
        {
            ellapsedTime += Time.deltaTime;

            // DMG 1
            if (ellapsedTime >= SPEAR_DMG_START && ellapsedTime <= SPEAR_DMG_STOP)
            {
                InitPart1();
            }
            if (act1GO.activeSelf && ellapsedTime >= SPEAR_DMG_STOP)
            {
                EndPart1();
            }

            // DMG 2
            if (ellapsedTime >= SHIELD_DMG_START && ellapsedTime <= SHIELD_DMG_STOP)
            {
                InitPart2();
            }
            if (act2GO.activeSelf && ellapsedTime >= SHIELD_DMG_STOP)
            {
                EndPart2();
            }

            if (ellapsedTime > DURATION)
            {
                End();
            }
        }

        public void End()
        {
            em.anim.SetBool("castingAttack3", false);
            complete = true;
            em.SetOverrideAgent(false);
        }

        public void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (abilityColliderTrigger == act1GO.GetComponent<AbilityColliderTrigger>())
                {
                    DamagePart1(col);
                }

                if (abilityColliderTrigger == act2GO.GetComponent<AbilityColliderTrigger>())
                {
                    DamagePart2(col);
                }
            }
        }

        private void DamagePart1(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            psm.TakeDamage(SPEAR_DMG);
        }

        private void DamagePart2(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            psm.TakeDamage(SHIELD_DMG);
            psm.Stun(SHIELD_STUN_DURATION);
        }

        private void InitPart1()
        {
            if (!part1Initialized)
            {
                act1GO.SetActive(true);
                act1.SetAbility(this);
                part1Initialized = true;
            }
        }

        private void InitPart2()
        {
            if (!part2Initialized)
            {
                act2GO.SetActive(true);
                act2.SetAbility(this);
                shieldCollider.SetActive(false);
                part2Initialized = true;
            }
        }

        private void EndPart1()
        {
            act1GO.SetActive(false);
            act1.UnsetAbility();
        }

        private void EndPart2()
        {
            act2GO.SetActive(false);
            act2.UnsetAbility();
            shieldCollider.SetActive(true);
        }
    }
}