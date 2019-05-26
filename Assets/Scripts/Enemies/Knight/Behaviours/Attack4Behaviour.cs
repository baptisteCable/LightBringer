using LightBringer.Abilities;
using LightBringer.Player;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack4Behaviour : CollisionBehaviour
    {
        private const float DURATION = 2.0f;
        private const float DURATION_RAGE = 1.3f;
        private const float RAY_DAMAGE = 30f;
        private const float EXPLOSION_DAMAGE = 30f;

        private const float CHANNELING_EFFECT_START = 22f / 60f;
        private const float CHANNELING_EFFECT_START_RAGE = 0;
        private const float DMG_START = 82f / 60f;
        private const float DMG_START_RAGE = 42f / 60f;
        private const float DMG_DURATION = .3f;

        private const float RAYCAST_HEIGHT = 2f;
        private const float MAX_DISTANCE = 100f;
        private const float DIST_FROM_CENTER_COLLIDER = 2f;
        private const float DIST_FROM_CENTER_RENDERER = 4f;

        private const float EXPLOSION_RADIUS = 8f;
        private const float EXPLOSION_RENDER_DURATION = 2f;

        private GameObject rayColliderContainer;
        private GameObject rayRenderer;

        private Transform target;
        private Vector3 targetPosition;

        private GameObject explRenderer;
        private GameObject explActGO;
        private AbilityColliderTrigger explAct;

        private bool effectStarted;

        private KnightMotor km;

        private bool missed = true;

        private float dmgStart;
        private float duration;
        private float channelingEffectStart;

        // Explosion collider list
        protected Dictionary<Collider, float> explCols;

        public Attack4Behaviour(KnightMotor enemyMotor, Transform target) : base(enemyMotor)
        {
            km = enemyMotor;
            this.target = target;
        }

        public override void Init()
        {
            base.Init();

            if (em.statusManager.mode == Mode.Rage)
            {
                duration = DURATION_RAGE;
                dmgStart = DMG_START_RAGE;
                channelingEffectStart = CHANNELING_EFFECT_START_RAGE;
                em.anim.Play("Attack4Rage", -1, 0);
                km.attack4ChannelingEffectRage.Play();
            }
            else
            {
                duration = DURATION;
                dmgStart = DMG_START;
                channelingEffectStart = CHANNELING_EFFECT_START;
                km.attack4ChannelingEffect.Play();

                if (em.statusManager.mode == Mode.Exhaustion)
                {
                    em.anim.Play("Attack4Exhaustion", -1, 0);
                }
                else
                {
                    em.anim.Play("Attack4", -1, 0);
                }
            }

            em.SetOverrideAgent(true);

            actGOs = new GameObject[1];
            parts = new Part[1];
            parts[0] = new Part(State.Before, dmgStart, DMG_DURATION, -1);

            acts = new AbilityColliderTrigger[1];
            actGOs[0] = km.attack4actGO;
            acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();

            // Rotate to face player
            targetPosition = target.position;

            effectStarted = false;
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

            // Channeling effect
            if (!effectStarted && Time.time > startTime + channelingEffectStart)
            {
                effectStarted = true;
                km.attack4ChannelingEffect.Play(true);
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
                float length = MAX_DISTANCE;

                // Create ray
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Environment");

                // If environment contact, shorter ray and explosion
                if (Physics.Raycast(km.attack4Container.transform.position + km.attack4Container.transform.forward * DIST_FROM_CENTER_COLLIDER,
                    km.attack4Container.transform.forward, out hit, MAX_DISTANCE, mask))
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

                // Ray collider length
                km.attack4actContainer.transform.localScale = new Vector3(1, 1, length);

                // Instanciate ray renderer
                km.attack4RayRenderer.SetupAndStart(DMG_DURATION, length - (DIST_FROM_CENTER_RENDERER - DIST_FROM_CENTER_COLLIDER));
            }

            base.StartCollisionPart(part);
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

        private void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (abilityColliderTrigger == acts[0] && !cols.ContainsKey(col))
                {
                    cols.Add(col, Time.time);
                    ApplyRayDamage(col);

                }
                if (abilityColliderTrigger == explAct && !explCols.ContainsKey(col))
                {
                    // if not behind obstacle
                    if (Vector3.Dot(explAct.transform.forward, col.transform.position - explAct.transform.position) >= 0f)
                    {
                        explCols.Add(col, Time.time);
                        ApplyExplosionDamage(abilityColliderTrigger, col);
                    }
                }
            }
        }

        private void ApplyRayDamage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(RAY_DAMAGE, DamageType.RangeInstant, DamageElement.Energy, em.transform.position);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
                missed = false;
            }
        }

        private void ApplyExplosionDamage(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(EXPLOSION_DAMAGE, DamageType.AreaOfEffect, DamageElement.Energy,
                abilityColliderTrigger.transform.position);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em, em.transform.position);
                missed = false;
            }
        }

        public override void Abort()
        {
            base.Abort();
            km.attack4RayRenderer.Abort();
            km.attack4ChannelingEffectRage.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            km.attack4ChannelingEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            em.SetOverrideAgent(false);
        }
    }
}