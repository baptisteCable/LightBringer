﻿using LightBringer.Abilities;
using LightBringer.Player;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack4Behaviour : CollisionBehaviour
    {
        private const float DURATION = 3.3f;
        private const float RAY_DAMAGE = 30f;
        private const float EXPLOSION_DAMAGE = 30f;

        private const float DMG_START = 1.8f;
        private const float DMG_DURATION = 1.2f;

        private const float RAYCAST_HEIGHT = 2f;
        private const float MAX_DISTANCE = 100f;
        private const float DIST_FROM_CENTER = 2f;

        private const float EXPLOSION_RADIUS = 8f;
        private const float EXPLOSION_RENDER_DURATION = 4f;

        private GameObject rayColliderContainer;
        private GameObject rayRenderer;

        private Transform target;
        private Vector3 targetPosition;

        private GameObject explRenderer;
        private GameObject explActGO;
        private AbilityColliderTrigger explAct;

        KnightMotor km;

        // Explosion collider list
        protected Dictionary<Collider, float> explCols;

        public Attack4Behaviour(KnightMotor enemyMotor, Transform target) : base(enemyMotor)
        {
            km = enemyMotor;
            this.target = target;
            actGOs = new GameObject[1];
            parts = new Part[1];
            parts[0] = new Part(State.Before, DMG_START, DMG_DURATION, -1);
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
                float length = MAX_DISTANCE;

                // Create ray
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Environment");

                // If environment contact, shorter ray and explosion
                if (Physics.Raycast(em.transform.position + Vector3.up * RAYCAST_HEIGHT + em.transform.forward * DIST_FROM_CENTER,
                    em.transform.forward, out hit, MAX_DISTANCE, mask))
                {
                    // ray length
                    length = hit.distance;

                    // Activate explosion collider trigger
                    explActGO = GameObject.Instantiate(km.attack4ExplColliderPrefab, hit.point,
                        Quaternion.LookRotation(hit.normal, Vector3.up), null);
                    explActGO.transform.localScale = Vector3.one * EXPLOSION_RADIUS;
                    GameObject.Destroy(explActGO, DMG_DURATION);

                    // Explosion zone collider
                    explCols = new Dictionary<Collider, float>();
                    explAct = explActGO.GetComponent<AbilityColliderTrigger>();
                    explAct.SetAbility(this, "explosion");

                    // Activate explosion renderer
                    explRenderer = GameObject.Instantiate(km.attack4ExplRendererPrefab, hit.point,
                        Quaternion.LookRotation(hit.normal, Vector3.up), null);
                    explRenderer.transform.localScale = Vector3.one * EXPLOSION_RADIUS;
                    GameObject.Destroy(explRenderer, EXPLOSION_RENDER_DURATION);
                }

                // Instanciate ray collider
                rayColliderContainer = GameObject.Instantiate(km.attack4RayColliderPrefab, em.transform);
                rayColliderContainer.transform.localPosition = new Vector3(0f, RAYCAST_HEIGHT, DIST_FROM_CENTER);
                rayColliderContainer.transform.localScale = new Vector3(1, 1, length);
                GameObject.Destroy(rayColliderContainer, DMG_DURATION + .05f);

                // Init Collision behaviour data
                actGOs[0] = rayColliderContainer.transform.Find("Cylinder").gameObject;
                acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();

                // Instanciate ray renderer
                rayRenderer = GameObject.Instantiate(km.attack4RayRendererPrefab, em.transform);
                rayRenderer.transform.localPosition = new Vector3(0f, RAYCAST_HEIGHT, DIST_FROM_CENTER);
                rayRenderer.GetComponent<RayRenderer>().SetupAndStart(DMG_DURATION, length + 1);
            }

            base.StartCollisionPart(part);
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

        private void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (abilityColliderTrigger == acts[0] && !cols.ContainsKey(col))
                {
                    cols.Add(col, Time.time);
                    ApplyRayDamage(col);
                    Debug.Log("Ray");

                }
                if (abilityColliderTrigger == explAct && !explCols.ContainsKey(col))
                {
                    // if not behind obstacle
                    if (Vector3.Dot(explAct.transform.forward, col.transform.position - explAct.transform.position) >= 0f)
                    {
                        explCols.Add(col, Time.time);
                        ApplyExplosionDamage(col);
                        Debug.Log("Explosion");
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

        private void ApplyExplosionDamage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(EXPLOSION_DAMAGE, DamageType.AreaOfEffect, DamageElement.Energy);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
            }
        }

        public override void Abort()
        {
            rayRenderer.SetActive(false);

            base.Abort();
        }
    }
}