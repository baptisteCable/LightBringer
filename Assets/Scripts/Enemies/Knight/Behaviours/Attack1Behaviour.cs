using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack1Behaviour : CollisionBehaviour
    {
        private const float DURATION = 3.1f;
        private const float RAY_DAMAGE = 15f;

        private const float DMG_START = 60f / 60f;
        private const float DMG_DURATION = 120f / 60f;
        private const float CONE_ANGLE = 60f;
        private const float TIME_BETWEEN_TICKS = .2f;
        
        private Vector3 targetPosition;

        private Transform attackContainer;
        private GameObject attackRenderer;

        public Attack1Behaviour(KnightMotor enemyMotor, Transform target, GameObject attack1act1GO, GameObject attack1Container) : base(enemyMotor)
        {
            targetPosition = target.position;
            actGOs = new GameObject[1];
            actGOs[0] = attack1act1GO;
            parts = new Part[1];
            parts[0] = new Part(State.Before, DMG_START, DMG_DURATION, 0);
            attackContainer = attack1Container.transform;
            attackRenderer = attackContainer.Find("Renderer").gameObject;
        }

        public override void Init()
        {
            base.Init();

            // em.anim.Play("Attack1", -1, 0);

            acts = new AbilityColliderTrigger[1];
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

            // Rotate at the beginning
            if (Time.time <= startTime + DMG_START)
            {
                em.RotateTowards(targetPosition);
            }

            if (Time.time > startTime + DURATION)
            {
                End();
            }
        }

        protected override void StartCollisionPart(int part)
        {
            if (part == 0)
            {
                // Activate ray
                attackRenderer.SetActive(true);
            }

            base.StartCollisionPart(part);
        }

        protected override void RunCollisionPart(int part)
        {
            if (part == 0)
            {
                // move Ray container
                float angle = -CONE_ANGLE / 2 + CONE_ANGLE * (Time.time - parts[0].startTime - startTime) / parts[0].duration;
                attackContainer.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
            }

            base.RunCollisionPart(part);
        }

        protected override void EndPart(int part)
        {
            if (part == 0)
            {
                // Disactivate ray
                attackRenderer.SetActive(false);
            }

            base.EndPart(part);
        }

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);
        }

        public override void OnColliderEnter(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            OnCollision(abilityColliderTrigger, col);
        }

        public override void OnColliderStay(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            OnCollision(abilityColliderTrigger, col);
        }

        private void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (cols.ContainsKey(col))
                {
                    if (cols[col] + TIME_BETWEEN_TICKS < Time.time)
                    {
                        cols[col] = Time.time;

                        // Damage
                        if (abilityColliderTrigger == actGOs[0].GetComponent<AbilityColliderTrigger>())
                        {
                            ApplyPart0Damage(col);
                        }
                    }
                }
                else
                {
                    cols.Add(col, Time.time);

                    // Damage
                    if (abilityColliderTrigger == actGOs[0].GetComponent<AbilityColliderTrigger>())
                    {
                        ApplyPart0Damage(col);
                    }
                }
            }
        }

        private void ApplyPart0Damage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(RAY_DAMAGE, DamageType.Melee, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
            }
        }

        public override void Abort()
        {
            attackRenderer.SetActive(false);

            base.Abort();
        }
    }
}