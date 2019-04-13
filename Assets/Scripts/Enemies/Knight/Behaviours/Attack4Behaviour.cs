using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;
/*
namespace LightBringer.Enemies.Knight
{
    public class Attack4Behaviour : CollisionBehaviour
    {
        private const float DURATION = 3.3f;
        private const float RAY_DAMAGE = 30f;

        private const float DMG_START = 2f;
        private const float DMG_DURATION = 1f;

        private GameObject attackContainerPrefab;
        private GameObject attackContainer;
        private GameObject attackRendererPrefab;
        private GameObject attackRenderer;

        private Transform target;
        private Vector3 targetPosition;

        public Attack4Behaviour(KnightMotor enemyMotor, Transform target, GameObject attackContainerPrefab,
            GameObject attackRendererPrefab) : base(enemyMotor)
        {
            this.target = target;
            actGOs = new GameObject[1];
            parts = new Part[1];
            parts[0] = new Part(State.Before, DMG_START, DMG_DURATION, -1);
            this.attackContainerPrefab = attackContainerPrefab;
            this.attackRendererPrefab = attackRendererPrefab;
        }

        public override void Init()
        {
            base.Init();

            // em.anim.Play("Attack1", -1, 0);

            acts = new AbilityColliderTrigger[1];

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
                // Create ray
                // Raycast
                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    Debug.Log("Did Hit");
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                    Debug.Log("Did not Hit");
                }

                // Selon distance, calculer la taille du rayon

                // instancier le rayon

                // Init Collision behaviour data
                actGOs[0] = attackContainer;
                acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();

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
                float angle = CONE_STARTING + CONE_ANGLE * (Time.time - parts[0].startTime - startTime) / parts[0].duration;
                attackContainer.localRotation = Quaternion.AngleAxis(angle, Vector3.up);

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
            //em.SetOverrideAgent(false);
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
        }
    }
}*/