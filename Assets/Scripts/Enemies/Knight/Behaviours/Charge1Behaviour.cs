using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Charge1Behaviour : CollisionBehaviour
    {
        private const float DURATION = 1.85f;
        private const float DURATION_RAGE = 1.4f;
        public const float CHARGE_MAX_RANGE = 40f;
        public const float CHARGE_MIN_RANGE = 10f;

        private const float STUN_DURATION = .5f;

        private const float DMG_START = 52f / 60f;
        private const float DMG_START_RAGE = 52f / 60f;
        private const float DMG_DURATION = 32f / 60f;
        private const float DMG_DURATION_RAGE = 32f / 60f;

        private Vector3 targetPosition;

        private float range;
        private float duration;
        private float dmgStart;
        private float dmgDuration;

        private KnightMotor km;

        public Charge1Behaviour(KnightMotor enemyMotor, Vector3 targetPoint) : base(enemyMotor)
        {
            km = enemyMotor;
            targetPosition = targetPoint;
        }

        public override void Init()
        {
            base.Init();

            if (em.statusManager.mode == Mode.Rage)
            {
                duration = DURATION_RAGE;
                dmgStart = DMG_START_RAGE;
                dmgDuration = DMG_DURATION_RAGE;
                em.anim.Play("Charge1Rage", -1, 0);
            }
            else
            {
                duration = DURATION;
                dmgStart = DMG_START;
                dmgDuration = DMG_DURATION;
                em.anim.Play("Charge1", -1, 0);
            }

            actGOs = new GameObject[1];
            actGOs[0] = km.charge1actGO;
            parts = new Part[1];
            parts[0] = new Part(State.Before, dmgStart, dmgDuration, 0);

            acts = new AbilityColliderTrigger[actGOs.Length];
            for (int i = 0; i < actGOs.Length; i++)
            {
                acts[i] = actGOs[i].GetComponent<AbilityColliderTrigger>();
            }

            range = Mathf.Min(Mathf.Max(Vector3.Distance(em.transform.position, targetPosition), CHARGE_MIN_RANGE), CHARGE_MAX_RANGE);
        }

        public override void Run()
        {
            DisplayIndicators();
            StartCollisionParts();
            RunCollisionParts();

            // Rotate at the beginning
            if (Time.time <= startTime + dmgStart)
            {
                em.RotateTowards(targetPosition);
            }

            if (Time.time > startTime + duration)
            {
                End();
            }
        }

        // Resize indicator depending on charge range
        protected override void DisplayIndicator(int part, float loadingTime)
        {
            base.DisplayIndicator(part, loadingTime);

            if (part == 0)
            {
                em.indicators[parts[part].indicator].transform.localScale = new Vector3(1, 1, range);
            }
        }

        protected override void StartCollisionPart(int i)
        {
            base.StartCollisionPart(i);

            if (i == 0)
            {
                // Effect
                km.chargeEffect.GetComponent<ParticleSystem>().Play();

                // Movement collisions
                em.SetMovementCollisonActive(true);
            }
        }

        protected override void RunCollisionPart(int part)
        {
            if (part == 0)
            {
                em.Move(em.transform.forward * range / dmgDuration);
            }

            base.RunCollisionPart(part);
        }

        protected override void EndPart(int part)
        {
            base.EndPart(part);

            if (part == 0)
            {
                // Effect
                km.chargeEffect.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);

                // Movement collisions
                em.SetMovementCollisonActive(false);
            }

        }

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);
        }

        public override void OnColliderEnter(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player" && !cols.ContainsKey(col))
            {
                cols.Add(col, Time.time);

                if (abilityColliderTrigger == actGOs[0].GetComponent<AbilityColliderTrigger>())
                {
                    ApplyDamage(col);
                }
            }
        }

        private void ApplyDamage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(25f, DamageType.Melee, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em);
                psm.ApplyCrowdControl(new CrowdControl(CrowdControlType.Stun, DamageType.Melee, DamageElement.Physical), STUN_DURATION);
            }
        }

        public override void Abort()
        {
            base.Abort();

            // Movement collisions
            em.SetMovementCollisonActive(false);
        }
    }
}