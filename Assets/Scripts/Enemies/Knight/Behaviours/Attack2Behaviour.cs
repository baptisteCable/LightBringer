using UnityEngine;
using LightBringer.Player;
using System.Collections.Generic;
using LightBringer.Abilities;

namespace LightBringer.Enemies.Knight
{
    public class Attack2Behaviour : Behaviour, CollisionAbility
    {
        private const float DURATION = 4.133f;
        private const float RANGE = 30f;
        private const float DAMAGE = 10f;
        private const float ENEMY_RAIN_RANGE = 15f;
        private const float TARGET_RAIN_RANGE = 8f;
        private const float ENEMY_RAIN_RADIUS = 1.5f;
        private const float TARGET_RAIN_RADIUS = 1f;

        private KnightMotor km;

        private const float LOAD_1 = 59f / 60f;
        private const float LOAD_2 = 91f / 60f;
        private const float LOAD_3 = 123f / 60f;

        private const float FIRE_1 = 75f / 60f;
        private const float FIRE_2 = 107f / 60f;
        private const float FIRE_3 = 139f / 60f;

        // Colliders GO
        private GameObject bullet1, bullet2, bullet3, previousBullet;

        // Collider list
        private List<Collider> cols;

        // Init booleans
        private bool bullet1Loaded = false;
        private bool bullet2Loaded = false;
        private bool bullet3Loaded = false;
        private bool bullet1Fired = false;
        private bool bullet2Fired = false;
        private bool bullet3Fired = false;

        private Transform target;

        float ellapsedTime = 0f;

        public Attack2Behaviour(KnightMotor enemyMotor, Transform target) : base(enemyMotor)
        {
            this.target = target;
            km = enemyMotor;
        }

        public override void Init()
        {
            em.anim.SetBool("castingAttack2", true);
            em.anim.Play("Attack2");
            em.SetOverrideAgent(true);
        }

        public override void Run()
        {
            ellapsedTime += Time.deltaTime;

            // DMG 1
            if (ellapsedTime >= LOAD_1 && !bullet1Loaded)
            {
                bullet1Loaded = true;
                bullet1 = InitBullet();
            }

            if (ellapsedTime >= LOAD_1 && ellapsedTime < FIRE_1)
            {
                bullet1.transform.localScale = Vector3.one * (ellapsedTime - LOAD_1) / (FIRE_1 - LOAD_1);
            }

            if (ellapsedTime >= FIRE_1 && !bullet1Fired)
            {
                bullet1Fired = true;
                FireBullet(bullet1);
            }

            // DMG 2
            if (ellapsedTime >= LOAD_2 && !bullet2Loaded)
            {
                bullet2Loaded = true;
                bullet2 = InitBullet();
            }

            if (ellapsedTime >= LOAD_2 && ellapsedTime < FIRE_2)
            {
                bullet2.transform.localScale = Vector3.one * (ellapsedTime - LOAD_2) / (FIRE_1 - LOAD_2);
            }

            if (ellapsedTime >= FIRE_2 && !bullet2Fired)
            {
                bullet2Fired = true;
                FireBullet(bullet2);
            }

            // DMG 3
            if (ellapsedTime >= LOAD_3 && !bullet3Loaded)
            {
                bullet3Loaded = true;
                bullet3 = InitBullet();
            }

            if (ellapsedTime >= LOAD_3 && ellapsedTime < FIRE_3)
            {
                bullet3.transform.localScale = Vector3.one * (ellapsedTime - LOAD_3) / (FIRE_3 - LOAD_3);
            }

            if (ellapsedTime >= FIRE_3 && !bullet3Fired)
            {
                bullet3Fired = true;
                FireBullet(bullet3);
            }

            if (ellapsedTime > DURATION)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            em.anim.SetBool("castingAttack2", false);
            em.SetOverrideAgent(false);
        }

        private GameObject InitBullet()
        {
            GameObject bullet = GameObject.Instantiate(km.bulletPrefab, em.transform, false);
            bullet.transform.localPosition = new Vector3(1.68f, 14.4f, .34f);
            bullet.transform.SetParent(null, true);
            return bullet;
        }

        private void FireBullet(GameObject bullet)
        {
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 30f, ForceMode.Impulse);
            GameObject.Destroy(bullet, .5f);
            
            InstanciateCaster(em.transform, ENEMY_RAIN_RANGE, ENEMY_RAIN_RADIUS);
            InstanciateCaster(target.transform, TARGET_RAIN_RANGE, TARGET_RAIN_RADIUS);
        }

        private void InstanciateCaster(Transform parentTransform, float range, float radius)
        {
            GameObject caster = GameObject.Instantiate(km.casterPrefab, parentTransform);
            caster.transform.localPosition = Vector3.zero;
            Attack2Caster a2c = caster.GetComponent<Attack2Caster>();
            a2c.ability = this;
            a2c.radius = radius;
            a2c.range = range;
        }

        public void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                Damage dmg = new Damage(10f, DamageType.AreaOfEffect, DamageElement.Energy);
                if (psm.IsAffectedBy(dmg, em))
                {
                    psm.TakeDamage(dmg, em);
                }
            }
        }
    }
}