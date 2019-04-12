using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack2Behaviour : EnemyBehaviour, CollisionAbility
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

        // Collider list
        private List<Collider> cols;

        private Transform target;

        private GameObject bullet;

        public Attack2Behaviour(KnightMotor enemyMotor, Transform target) : base(enemyMotor)
        {
            this.target = target;
            km = enemyMotor;
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("Attack2", -1, 0);

            em.SetOverrideAgent(true);

            parts = new Part[3];
            parts[0] = new Part(State.IndicatorDisplayed, LOAD_1, FIRE_1 - LOAD_1, -1);
            parts[1] = new Part(State.IndicatorDisplayed, LOAD_2, FIRE_2 - LOAD_2, -1);
            parts[2] = new Part(State.IndicatorDisplayed, LOAD_3, FIRE_3 - LOAD_3, -1);
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
            bullet = GameObject.Instantiate(km.bulletPrefab, em.transform, false);
            bullet.transform.localPosition = new Vector3(1.68f, 14.4f, .34f);
            bullet.transform.SetParent(null, true);
            base.StartPart(i);
        }

        protected override void EndPart(int i)
        {
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 30f, ForceMode.Impulse);
            GameObject.Destroy(bullet, .5f);

            InstanciateCaster(em.transform, ENEMY_RAIN_RANGE, ENEMY_RAIN_RADIUS);
            InstanciateCaster(target.transform, TARGET_RAIN_RANGE, TARGET_RAIN_RADIUS);

            base.EndPart(i);
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

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);
        }

        public void OnColliderEnter(AbilityColliderTrigger abilityColliderTrigger, Collider col)
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

        public void OnColliderStay(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
        }
    }
}