using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack1Behaviour : CollisionBehaviour
    {
        private const float DURATION = 4.83f;
        private const float DURATION_RAGE = 3.58f;
        private const float RAY_DAMAGE = 10f;
        private const float TIME_BETWEEN_RAY_TICKS = .2f;
        private const float GROUND_DAMAGE = 1.5f;
        private const float TIME_BETWEEN_GROUND_TICKS = .05f;

        private const float DMG_START = 123f / 60f;
        private const float DMG_START_RAGE = 63f / 60f;
        private const float DMG_DURATION = 136f / 60f;
        public const float GROUND_DURATION = 10f;
        public const float CONE_ANGLE = 70.8f;
        public const float CONE_STARTING = -18.8f;

        private const float RAYCAST_HEIGHT = 2f;
        public const float MAX_DISTANCE = 29f;
        private const float DIST_FROM_CENTER_COLLIDER = 2f;

        private const float ANGLE_SPACING = 3f;
        private const float SAFETY_OVERLAP = .5f;

        private Transform target;
        private Vector3 targetPosition;

        private GameObject groundActPrefab;
        private GameObject groundRendererPrefab;
        private GameObject groundRenderer;
        private ConeMesh groundConeMesh;
        private BurningGround burningGround;

        private KnightMotor km;
        private float dmgStart;
        private float duration;

        private bool missed = true;

        // Ground collider list
        protected Dictionary<Collider, float> groundCols;

        private float nextAngle;

        public Attack1Behaviour(KnightMotor enemyMotor, Transform target, GameObject groundActPrefab, GameObject groundRendererPrefab) : base(enemyMotor)
        {
            km = enemyMotor;
            this.target = target;
            this.groundActPrefab = groundActPrefab;
            this.groundRendererPrefab = groundRendererPrefab;
        }

        public override void Init()
        {
            base.Init();

            if (em.statusManager.mode == Mode.Rage)
            {
                duration = DURATION_RAGE;
                dmgStart = DMG_START_RAGE;
                em.anim.Play("Attack1Rage", -1, 0);
                km.attack1ChannelingEffectRage.Play();
            }
            else
            {
                duration = DURATION;
                dmgStart = DMG_START;
                km.attack1ChannelingEffect.Play();

                if (em.statusManager.mode == Mode.Exhaustion)
                {
                    em.anim.Play("Attack1Exhaustion", -1, 0);
                }
                else
                {
                    em.anim.Play("Attack1", -1, 0);
                }
            }

            actGOs = new GameObject[1];
            actGOs[0] = km.attack1actGO;
            parts = new Part[1];
            parts[0] = new Part(State.Before, dmgStart, DMG_DURATION, -1);

            acts = new AbilityColliderTrigger[1];
            acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();

            // Rotate to face player
            targetPosition = target.position;
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

        protected override void StartCollisionPart(int part)
        {
            if (part == 0)
            {
                // Activate ray
                km.attack1RayRenderer.SetupAndStart(DMG_DURATION, MAX_DISTANCE);

                // Ground col list init
                groundCols = new Dictionary<Collider, float>();

                // Ground renderer
                groundRenderer = GameObject.Instantiate(groundRendererPrefab, em.transform.position + Vector3.up * .1f + em.transform.right * 2f,
                    em.transform.rotation, null);
                burningGround = groundRenderer.GetComponent<BurningGround>();
                GameObject.Destroy(groundRenderer, GROUND_DURATION);

                // First angle
                nextAngle = CONE_STARTING + (ANGLE_SPACING - SAFETY_OVERLAP) / 2f;
            }

            base.StartCollisionPart(part);
        }

        protected override void RunCollisionPart(int part)
        {
            if (part == 0)
            {
                // move Ray container
                float angle = CONE_STARTING + CONE_ANGLE * (Time.time - parts[0].startTime - startTime) / parts[0].duration;

                // compute length
                float length = MAX_DISTANCE;
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Environment");

                // If environment contact, shorter ray and explosion
                if (Physics.Raycast(groundRenderer.transform.position + km.attack1Container.transform.forward * DIST_FROM_CENTER_COLLIDER,
                    Quaternion.Euler(0, angle, 0) * km.transform.forward, out hit, MAX_DISTANCE, mask))
                {
                    // ray length (collider)
                    length = hit.distance;
                }

                km.attack1actContainer.transform.localScale = new Vector3(1, 1, length);
                km.attack1RayRenderer.SetLength(length - 1);

                // Ground angle
                burningGround.SetAngle(angle);

                // Next sector
                if (angle > nextAngle)
                {
                    nextAngle = angle + ANGLE_SPACING - SAFETY_OVERLAP;

                    // Ground renderer
                    burningGround.addAngle3d(angle, length + DIST_FROM_CENTER_COLLIDER);

                    // new ground collider trigger for this sector
                    GameObject groundActGO = GameObject.Instantiate(groundActPrefab, groundRenderer.transform.position,
                                    em.transform.rotation * Quaternion.AngleAxis(angle, Vector3.up), null);
                    groundActGO.transform.localScale = new Vector3(length + DIST_FROM_CENTER_COLLIDER, 1, length + DIST_FROM_CENTER_COLLIDER);
                    GameObject.Destroy(groundActGO, GROUND_DURATION - (Time.time - startTime - DMG_START));

                    // Ability coolider trigger
                    AbilityColliderTrigger groundAct = groundActGO.GetComponent<AbilityColliderTrigger>();
                    groundAct.SetAbility(this, "ground");
                }

            }

            base.RunCollisionPart(part);
        }

        protected override void EndPart(int part)
        {
            base.EndPart(part);
            burningGround.EndRotation();
        }

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);

            if (missed)
            {
                em.statusManager.IncreaseRageMissedAttack();
            }
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
                if (abilityColliderTrigger == acts[0])
                {
                    if (cols.ContainsKey(col))
                    {
                        if (cols[col] + TIME_BETWEEN_RAY_TICKS < Time.time)
                        {
                            cols[col] = Time.time;
                            ApplyRayDamage(col);
                        }
                    }
                    else
                    {
                        cols.Add(col, Time.time);
                        ApplyRayDamage(col);
                    }
                }
                if (abilityColliderTrigger.abTag == "ground")
                {
                    if (groundCols.ContainsKey(col))
                    {
                        if (groundCols[col] + TIME_BETWEEN_GROUND_TICKS < Time.time)
                        {
                            groundCols[col] = Time.time;
                            ApplyGroundDamage(col);
                        }
                    }
                    else
                    {
                        groundCols.Add(col, Time.time);
                        ApplyGroundDamage(col);
                    }
                }
            }
        }

        private void ApplyRayDamage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(RAY_DAMAGE, DamageType.AreaOfEffect, DamageElement.Energy);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
                missed = false;
            }
        }

        private void ApplyGroundDamage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(GROUND_DAMAGE, DamageType.AreaOfEffect, DamageElement.Energy);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
            }
        }

        public override void Abort()
        {
            base.Abort();
            em.SetOverrideAgent(false);
        }
    }
}