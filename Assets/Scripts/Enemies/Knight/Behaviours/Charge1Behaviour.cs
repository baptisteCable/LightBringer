using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Charge1Behaviour : CollisionBehaviour
    {
        private const float DURATION = 2.9f;
        public const float CHARGE_MAX_RANGE = 40f;
        public const float CHARGE_MIN_RANGE = 5f;

        private const float STUN_DURATION = .5f;

        private const float DMG_START = 52f / 60f;
        private const float DMG_DURATION = 32f / 60f;

        private const float ACCESSIBILITY_RAYCAST_ALTITUDE = .1f;

        private Vector3 targetPosition;

        private float range;
        private KnightMotor km;

        public Charge1Behaviour(KnightMotor enemyMotor, Vector3 targetPoint, GameObject charge1actGO) : base(enemyMotor)
        {
            targetPosition = targetPoint;
            actGOs = new GameObject[1];
            actGOs[0] = charge1actGO;
            parts = new Part[1];
            parts[0] = new Part(State.Before, DMG_START, DMG_DURATION, 0);
            km = enemyMotor;
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("Charge1", -1, 0);

            acts = new AbilityColliderTrigger[actGOs.Length];
            for (int i = 0; i < actGOs.Length; i++)
            {
                acts[i] = actGOs[i].GetComponent<AbilityColliderTrigger>();
            }

            range = Vector3.Distance(em.transform.position, targetPosition);
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

        protected override void StartCollisionPart(int i)
        {
            if (i == 0)
            {
                // Effect
                km.chargeEffect.GetComponent<ParticleSystem>().Play();
            }

            base.StartCollisionPart(i);
        }

        protected override void RunCollisionPart(int part)
        {
            if (part == 0)
            {
                em.Move(em.transform.forward * range / DMG_DURATION);
            }

            base.RunCollisionPart(part);
        }

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);
        }

        public override void OnColliderEnter(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player" && !cols.ContainsKey(col))
            {
                cols.Add(col, Time.time);

                if (abilityColliderTrigger == actGOs[0].GetComponent<AbilityColliderTrigger>())
                {
                    ApplyDamage(col);
                }
            }
        }

        private void ApplyDamage(Collider col)
        {
            PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
            Damage dmg = new Damage(25f, DamageType.Melee, DamageElement.Physical);
            if (psm.IsAffectedBy(dmg, em, em.transform.position))
            {
                psm.TakeDamage(dmg, em);
                psm.ApplyCrowdControl(new CrowdControl(CrowdControlType.Stun, DamageType.Melee, DamageElement.Physical), STUN_DURATION);
            }
        }

        public static bool ComputeTargetPoint(Transform knight, Vector3 playerPos, float playerDistance, bool sightLineRequired,
            bool canGoBehindPlayer, bool canGoBackWard, out Vector3 targetPosition)
        {
            // fixed distance to the player
            if (playerDistance > 0)
            {
                return GetAccessiblePointAroundPlayer(knight, playerPos, playerDistance, sightLineRequired, canGoBehindPlayer, out targetPosition);
            }
            // random dist (accessible) with sight line
            else
            {
                return GetAccessiblePointWithSightLine(knight, playerPos, sightLineRequired, canGoBehindPlayer, canGoBackWard, out targetPosition);
            }
        }

        // Try 10 points at the right distance from the target point
        private static bool GetAccessiblePointAroundPlayer(Transform knight, Vector3 playerPos, float distance, bool sightLineRequired,
            bool canGoBehindPlayer, out Vector3 point)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = Random.value * 180 - 90;
                if (canGoBehindPlayer)
                {
                    angle *= 2;
                }

                Vector3 targetPoint = playerPos + Quaternion.AngleAxis(angle, Vector3.up) * ((knight.position - playerPos).normalized * distance);

                if (isPointAccessible(knight, targetPoint) && (!sightLineRequired || hasSightLine(playerPos, targetPoint)))
                {
                    point = targetPoint;
                    return true;
                }
            }

            point = Vector3.zero;
            return false;
        }

        // Try 100 points
        private static bool GetAccessiblePointWithSightLine(Transform knight, Vector3 playerPos, bool sightLineRequired,
            bool canGoBehindPlayer, bool canGoBackWard, out Vector3 point)
        {
            for (int i = 0; i < 100; i++)
            {
                float angle = Random.value * 180 - 90;
                if (canGoBackWard)
                {
                    angle *= 2;
                }

                float distance = Random.value * (CHARGE_MAX_RANGE - CHARGE_MIN_RANGE) + CHARGE_MIN_RANGE;

                Vector3 targetPoint = knight.position + Quaternion.AngleAxis(angle, Vector3.up) * ((knight.forward).normalized * distance);

                if (isPointAccessible(knight, targetPoint)
                    && (!sightLineRequired || hasSightLine(playerPos, targetPoint))
                    && (canGoBehindPlayer || !isBehindPlayer(knight, playerPos, targetPoint)))
                {
                    point = targetPoint;
                    return true;
                }
            }

            point = Vector3.zero;
            return false;
        }

        private static bool hasSightLine(Vector3 origin, Vector3 destination)
        {
            LayerMask mask = LayerMask.GetMask("Environment");
            return !Physics.Raycast(origin + ACCESSIBILITY_RAYCAST_ALTITUDE * Vector3.up,
                destination - origin, Vector3.Distance(destination, origin), mask);
        }

        private static bool isBehindPlayer(Transform knight, Vector3 playerPos, Vector3 point)
        {
            return Vector3.Dot(playerPos - knight.position, playerPos - point) < 0;
        }

        // Test if a charge can go to this point within range and with no obstacle
        private static bool isPointAccessible(Transform knight, Vector3 point)
        {
            float dist = Vector3.Distance(knight.position, point);
            if (dist < CHARGE_MIN_RANGE || dist > CHARGE_MAX_RANGE)
            {
                return false;
            }

            // Knight radius
            float knightRadius = knight.GetComponent<CharacterController>().radius;

            // Create mask
            LayerMask mask = LayerMask.GetMask("Environment");

            // Cast 5 rays to check accessibility
            for (int i = -2; i <= 2; i++)
            {
                if (Physics.Raycast(knight.position + ACCESSIBILITY_RAYCAST_ALTITUDE * Vector3.up + knight.right * knightRadius * i,
                    point - knight.position, dist + knightRadius, mask))
                {
                    return false;
                }
            }

            return true;
        }
    }
}