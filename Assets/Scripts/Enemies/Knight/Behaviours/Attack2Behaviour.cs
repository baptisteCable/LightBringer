using LightBringer.Abilities;
using LightBringer.Player;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack2Behaviour : EnemyBehaviour, CollisionAbility
    {
        private const float DURATION = 4.2f;
        private const float DURATION_RAGE = 3.3f;
        private const float RANGE = 30f;
        private const float DAMAGE = 10f;
        private const float ENEMY_RAIN_RANGE = 15f;
        private const float TARGET_RAIN_RANGE = 8f;
        private const float ENEMY_RAIN_RADIUS = 1.5f;
        private const float TARGET_RAIN_RADIUS = 1f;

        private KnightMotor km;

        private const float LOAD_1 = 59f / 60f;
        private const float LOAD_1_RAGE = 22f / 60f;
        private const float TIME_BETWEEN_LOAD = 32f / 60f;
        private const float FIRE_AFTER_LOADING = 16f / 60f;
        private const float CHANNELING_START_BEFORE_LOAD = 20f / 60f;

        // Collider list
        private List<Collider> cols;

        private Transform target;

        private GameObject bullet;

        private float duration;
        private float load1;

        public Attack2Behaviour (KnightMotor enemyMotor, Transform target) : base (enemyMotor)
        {
            this.target = target;
            km = enemyMotor;
        }

        public override void Init ()
        {
            base.Init ();

            if (em.statusManager.mode == Mode.Rage)
            {
                duration = DURATION_RAGE;
                load1 = LOAD_1_RAGE;
                em.anim.Play ("Attack2Rage", -1, 0);
            }
            else
            {
                duration = DURATION;
                load1 = LOAD_1;

                if (em.statusManager.mode == Mode.Exhaustion)
                {
                    em.anim.Play ("Attack2Exhaustion", -1, 0);
                }
                else
                {
                    em.anim.Play ("Attack2", -1, 0);
                }
            }

            em.SetOverrideAgent (true);

            parts = new Part[6];
            parts[0] = new Part (State.IndicatorDisplayed, load1, FIRE_AFTER_LOADING, -1);
            parts[1] = new Part (State.IndicatorDisplayed, load1 + TIME_BETWEEN_LOAD, FIRE_AFTER_LOADING, -1);
            parts[2] = new Part (State.IndicatorDisplayed, load1 + 2 * TIME_BETWEEN_LOAD, FIRE_AFTER_LOADING, -1);

            // channeling effects
            parts[3] = new Part (State.IndicatorDisplayed, load1 - CHANNELING_START_BEFORE_LOAD, FIRE_AFTER_LOADING, -1);
            parts[4] = new Part (State.IndicatorDisplayed, load1 + TIME_BETWEEN_LOAD - CHANNELING_START_BEFORE_LOAD, FIRE_AFTER_LOADING, -1);
            parts[5] = new Part (State.IndicatorDisplayed, load1 + 2 * TIME_BETWEEN_LOAD - CHANNELING_START_BEFORE_LOAD, FIRE_AFTER_LOADING, -1);
        }

        public override void Run ()
        {
            StartParts ();
            RunParts ();

            if (Time.time > startTime + duration)
            {
                End ();
            }
        }

        protected override void StartPart (int i)
        {
            if (i < 3)
            {
                bullet = GameObject.Instantiate (km.bulletPrefab, em.transform, false);
                bullet.transform.localPosition = new Vector3 (1.68f, 14.4f, .34f);
                bullet.transform.SetParent (null, true);
            }
            else
            {
                km.attack2ChannelingEffect.Play ();
            }

            base.StartPart (i);
        }

        protected override void EndPart (int i)
        {
            if (i < 3)
            {
                Rigidbody rb = bullet.GetComponent<Rigidbody> ();
                rb.AddForce (Vector3.up * 30f, ForceMode.Impulse);
                GameObject.Destroy (bullet, .5f);

                InstanciateCaster (em.transform, ENEMY_RAIN_RANGE, ENEMY_RAIN_RADIUS);
                InstanciateCaster (target.transform, TARGET_RAIN_RANGE, TARGET_RAIN_RADIUS);
            }

            base.EndPart (i);
        }

        private void InstanciateCaster (Transform parentTransform, float range, float radius)
        {
            GameObject caster = GameObject.Instantiate (km.casterPrefab, parentTransform);
            caster.transform.localPosition = Vector3.zero;
            Attack2Caster a2c = caster.GetComponent<Attack2Caster> ();
            a2c.ability = this;
            a2c.radius = radius;
            a2c.range = range;
        }

        public override void End ()
        {
            base.End ();
            em.SetOverrideAgent (false);
        }

        public void OnColliderEnter (AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager> ();
                Damage dmg = new Damage (10f, DamageType.AreaOfEffect, DamageElement.Energy, abilityColliderTrigger.transform.position);
                if (psm.IsAffectedBy (dmg, em))
                {
                    psm.TakeDamage (dmg, em);
                }
            }
        }

        public void OnColliderStay (AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
        }

        public override void Abort ()
        {
            base.Abort ();

            if (bullet != null)
            {
                GameObject.Destroy (bullet);
            }

            em.SetOverrideAgent (false);
        }
    }
}