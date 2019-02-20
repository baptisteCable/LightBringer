using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

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
        private GameObject[] bullets;

        // Collider list
        private List<Collider> cols;

        private Transform target;

        public Attack2Behaviour(KnightMotor enemyMotor, Transform target) : base(enemyMotor)
        {
            this.target = target;
            km = enemyMotor;
        }

        public override void Init()
        {
            base.Init();

            em.anim.SetBool("castingAttack2", true);
            em.anim.Play("Attack2");
            em.SetOverrideAgent(true);
            bullets = new GameObject[3];

            parts = new Part[3];
            parts[0] = new Part(State.IndicatorDisplayed, LOAD_1, FIRE_1 - LOAD_1, null);
            parts[1] = new Part(State.IndicatorDisplayed, LOAD_2, FIRE_2 - LOAD_2, null);
            parts[2] = new Part(State.IndicatorDisplayed, LOAD_3, FIRE_3 - LOAD_3, null);
        }

        public override void Run()
        {
            StartParts();
            RunParts();

            if (Time.time > startTime + DURATION)
            {
                End();
            }
        }

        protected override void StartPart(int i)
        {
            bullets[i] = InitBullet();
            base.StartPart(i);
        }

        protected override void RunPart(int part)
        {
            bullets[part].transform.localScale = Vector3.one * (Time.time - (startTime + parts[part].startTime)) / parts[part].duration;
            base.RunPart(part);
        }

        protected override void EndPart(int i)
        {
            FireBullet(bullets[i]);
            base.EndPart(i);
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