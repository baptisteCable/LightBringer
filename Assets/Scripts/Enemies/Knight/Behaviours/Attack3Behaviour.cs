using UnityEngine;
using LightBringer.Player;
using LightBringer.Abilities;

namespace LightBringer.Enemies.Knight
{
    public class Attack3Behaviour : CollisionBehaviour
    {
        private const float DURATION = 3.2f;
        private const float DURATION_RAGE = 2.2f;
        private const float SPEAR_DMG = 14f;
        private const float SHIELD_DMG = 10f;

        private const float SPEAR_DMG_START = 121f / 60f;
        private const float SPEAR_DMG_START_RAGE = 61f / 60f;
        private const float SPEAR_DMG_DURATION = 25f / 60f;
        private const float SHIELD_DMG_START_AFTER_SPEAR = 36f / 60f;
        private const float SHIELD_DMG_DURATION = 20f / 60f;

        // Motor
        KnightMotor km;

        private bool missed = true;

        // Shield collider to disable
        private GameObject shieldCollider;

        private float duration;
        private float spearDmgStart;

        public Attack3Behaviour(KnightMotor enemyMotor, GameObject attack3act1GO, GameObject attack3act2GO, GameObject shieldCollider) : base(enemyMotor)
        {
            km = enemyMotor;
            actGOs = new GameObject[2];
            actGOs[0] = attack3act1GO;
            actGOs[1] = attack3act2GO;
            this.shieldCollider = shieldCollider;
        }

        public override void Init()
        {
            base.Init();

            if (em.statusManager.mode == Mode.Rage)
            {
                duration = DURATION_RAGE;
                spearDmgStart = SPEAR_DMG_START_RAGE;
                em.anim.Play("Attack3Rage", -1, 0);
                km.attack3ChannelingEffectRage.Play();
            }
            else
            {
                duration = DURATION;
                spearDmgStart = SPEAR_DMG_START;
                em.anim.Play("Attack3", -1, 0);
                km.attack3ChannelingEffect.Play();
            }

            parts = new Part[2];
            parts[0] = new Part(State.Before, spearDmgStart, SPEAR_DMG_DURATION, -1);
            parts[1] = new Part(State.Before, spearDmgStart + SHIELD_DMG_START_AFTER_SPEAR, SHIELD_DMG_DURATION, -1);

            acts = new AbilityColliderTrigger[2];
            acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();
            acts[1] = actGOs[1].GetComponent<AbilityColliderTrigger>();
        }

        public override void Run()
        {
            DisplayIndicators();
            StartCollisionParts();
            RunCollisionParts();

            if (Time.time > startTime + duration)
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
            em.SetOverrideAgent(false);

            if (missed)
            {
                em.statusManager.IncreaseRageMissedAttack();
            }
        }

        public override void OnColliderEnter(AbilityColliderTrigger abilityColliderTrigger, Collider col)
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
                missed = false;
            }
        }

        private void DamagePart1(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(SHIELD_DMG, DamageType.AreaOfEffect, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em))
            {
                psm.TakeDamage(dmg, em);
                missed = false;
            }
        }

        public override void Abort()
        {
            shieldCollider.SetActive(true);
            base.Abort();
            em.SetOverrideAgent(false);
        }
    }
}