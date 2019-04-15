using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack1Behaviour : CollisionBehaviour
    {
        private const float DURATION = 4.3f;
        private const float RAY_DAMAGE = 10f;
        private const float TIME_BETWEEN_RAY_TICKS = .2f;
        private const float GROUND_DAMAGE = 1.5f;
        private const float TIME_BETWEEN_GROUND_TICKS = .05f;

        private const float DMG_START = 2f;
        private const float DMG_DURATION = 2.2f;
        public const float GROUND_DURATION = 10f;
        public const float CONE_ANGLE = 75f;
        public const float CONE_STARTING = -20f;

        private const float TIME_BETWEEN_REFRESH = .02f;

        private Transform attackContainer;
        private GameObject attackRenderer;
        private Transform target;
        private Vector3 targetPosition;

        private GameObject groundActPrefab;
        private GameObject groundRendererPrefab;
        private GameObject groundActGO;
        private AbilityColliderTrigger groundAct;
        private GameObject groundRenderer;
        private ConeMesh groundConeMesh;
        private BurningGround burningGround;

        // Ground collider list
        protected Dictionary<Collider, float> groundCols;

        private float nextConeRefresh;

        public Attack1Behaviour(KnightMotor enemyMotor, Transform target, GameObject attack1act1GO, GameObject attack1Container,
            GameObject groundActPrefab, GameObject groundRendererPrefab) : base(enemyMotor)
        {
            this.target = target;
            actGOs = new GameObject[1];
            actGOs[0] = attack1act1GO;
            parts = new Part[1];
            parts[0] = new Part(State.Before, DMG_START, DMG_DURATION, -1);
            attackContainer = attack1Container.transform;
            attackRenderer = attackContainer.Find("Renderer").gameObject;
            this.groundActPrefab = groundActPrefab;
            this.groundRendererPrefab = groundRendererPrefab;
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("Attack1", -1, 0);

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

                // Activate ground collider trigger
                groundActGO = GameObject.Instantiate(groundActPrefab, em.transform.position,
                    em.transform.rotation * Quaternion.AngleAxis(CONE_STARTING, Vector3.up), null);
                groundConeMesh = groundActGO.GetComponent<ConeMesh>();
                GameObject.Destroy(groundActGO, GROUND_DURATION);
                nextConeRefresh = Time.time + TIME_BETWEEN_REFRESH;

                // Ground col list init
                groundCols = new Dictionary<Collider, float>();
                groundAct = groundActGO.GetComponent<AbilityColliderTrigger>();
                groundAct.SetAbility(this, "ground");

                // Ground renderer
                groundRenderer = GameObject.Instantiate(groundRendererPrefab, em.transform.position,
                    em.transform.rotation, null);
                burningGround = groundRenderer.GetComponent<BurningGround>();
                GameObject.Destroy(groundRenderer, GROUND_DURATION);
            }

            base.StartCollisionPart(part);
        }

        protected override void RunCollisionPart(int part)
        {
            if (part == 0)
            {
                // move Ray container
                float angle = CONE_STARTING + CONE_ANGLE * (Time.time - parts[0].startTime - startTime) / parts[0].duration + 8f;
                //attackContainer.localRotation = Quaternion.AngleAxis(angle, Vector3.up);

                // Ground collider trigger
                if (Time.time > nextConeRefresh)
                {
                    float coneAngleWidth = angle - CONE_STARTING;
                    nextConeRefresh = Time.time + TIME_BETWEEN_REFRESH;
                    groundConeMesh.angle = coneAngleWidth;
                    groundConeMesh.CreateAngularAoEMesh();
                    groundActGO.transform.localRotation = em.transform.rotation * 
                        Quaternion.AngleAxis((angle - CONE_STARTING) / 2f + CONE_STARTING, Vector3.up);
                    burningGround.SetAngle(angle);
                }

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
                if (abilityColliderTrigger == groundAct)
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
            attackRenderer.SetActive(false);

            base.Abort();
            em.SetOverrideAgent(false);
        }
    }
}