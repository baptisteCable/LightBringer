using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Enemies
{
    public abstract class Controller : MonoBehaviour
    {
        protected const float TARGET_DETECTION_DISTANCE = 100f;

        public const float ACCESSIBILITY_RAYCAST_ALTITUDE = .1f;

        // Components
        public Motor motor;
        protected NavMeshAgent agent;

        // Behaviours
        protected EnemyBehaviour currentBehaviour;
        protected EnemyBehaviour nextActionBehaviour;
        public bool passive = false;

        // Target
        public Transform target;

        protected void BaseStart ()
        {
            motor = GetComponent<Motor> ();
            agent = motor.agent;
            agent.destination = transform.position;
        }

        protected void SelectTarget ()
        {
            LayerMask mask = LayerMask.GetMask ("Player", "Immaterial", "NoCollision");
            Collider[] cols = Physics.OverlapSphere (transform.position, TARGET_DETECTION_DISTANCE, mask);
            List<Collider> colList = new List<Collider> (cols);

            // Random choice
            int index;
            target = null;
            while (colList.Count > 0 && (target == null || target.tag != "Player"))
            {
                index = (int)(Random.value * cols.Length);
                target = colList[index].transform;
                colList.RemoveAt (index);
            }

            if (target == null || target.tag != "Player")
            {
                target = null;
                // Debug.Log("No target found");
            }
        }

        // Try 10 points at the right distance from the target point
        public static bool GetAccessiblePointInStraightLineAroundPlayer (Transform enemy, Vector3 playerPos, float distance, bool sightLineRequired,
            bool canGoBehindPlayer, float minDist, float maxDist, out Vector3 point)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = Random.value * 180 - 90;
                if (canGoBehindPlayer)
                {
                    angle *= 2;
                }

                Vector3 targetPoint = playerPos + Quaternion.AngleAxis (angle, Vector3.up) * ((enemy.position - playerPos).normalized * distance);

                if (Controller.isPointAccessibleInStraightLine (enemy, targetPoint, minDist, maxDist)
                    && (!sightLineRequired || Controller.hasSightLine (playerPos, targetPoint)))
                {
                    point = targetPoint;
                    return true;
                }
            }

            point = Vector3.zero;
            return false;
        }

        // Try 100 points
        public static bool GetAccessiblePointInStraightLineWithSightLine (Transform enemy, Vector3 playerPos, float minPlayerDistance,
            float maxPlayerDistance, bool sightLineRequired, bool canGoBehindPlayer, bool canGoBackWard, float minDist, float maxDist, out Vector3 point)
        {
            for (int i = 0; i < 100; i++)
            {
                float angle = Random.value * 180 - 90;
                if (canGoBackWard)
                {
                    angle *= 2;
                }

                float distance = Random.value * (maxDist - minDist) + minDist;

                Vector3 targetPoint = enemy.position + Quaternion.AngleAxis (angle, Vector3.up) * ((playerPos - enemy.position).normalized * distance);

                if (Vector3.Distance (targetPoint, playerPos) >= minPlayerDistance && Vector3.Distance (targetPoint, playerPos) <= maxPlayerDistance
                    && Controller.isPointAccessibleInStraightLine (enemy, targetPoint, minDist, maxDist)
                    && (!sightLineRequired || Controller.hasSightLine (playerPos, targetPoint))
                    && (canGoBehindPlayer || !Controller.isBehindPlayer (enemy, playerPos, targetPoint)))
                {
                    point = targetPoint;
                    return true;
                }
            }

            point = Vector3.zero;
            return false;
        }

        public static bool hasSightLine (Vector3 origin, Vector3 destination)
        {
            LayerMask mask = LayerMask.GetMask ("Environment");
            return !Physics.Raycast (origin + ACCESSIBILITY_RAYCAST_ALTITUDE * Vector3.up,
                destination - origin, Vector3.Distance (destination, origin), mask);
        }

        public static bool isBehindPlayer (Transform knight, Vector3 playerPos, Vector3 point)
        {
            return Vector3.Dot (playerPos - knight.position, playerPos - point) < 0;
        }

        // Test if a charge can go to this point within range and with no obstacle
        public static bool isPointAccessibleInStraightLine (Transform enemy, Vector3 point, float minDist, float maxDist)
        {
            float dist = Vector3.Distance (enemy.position, point);
            if (dist < minDist || dist > maxDist)
            {
                return false;
            }

            // Knight radius
            float knightRadius = enemy.GetComponent<CharacterController> ().radius;

            // Create mask
            LayerMask mask = LayerMask.GetMask ("Environment");

            // Cast 5 rays to check accessibility
            for (int i = -2; i <= 2; i++)
            {
                if (Physics.Raycast (enemy.position + Controller.ACCESSIBILITY_RAYCAST_ALTITUDE * Vector3.up + enemy.right * knightRadius * i,
                    point - enemy.position, dist + knightRadius, mask))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanFindTargetPoint (Transform enemy, Vector3 playerPos, float playerDistance, bool sightLineRequired,
            bool canGoBehindPlayer, float minDist, float maxDist, out Vector3 targetPosition)
        {
            // fixed distance to the player
            return Controller.GetAccessiblePointInStraightLineAroundPlayer (enemy, playerPos, playerDistance, sightLineRequired,
                canGoBehindPlayer, minDist, maxDist, out targetPosition);
        }

        public static bool CanFindTargetPoint (Transform enemy, Vector3 playerPos, float minPlayerDistance, float maxPlayerDistance, bool sightLineRequired,
            bool canGoBehindPlayer, bool canGoBackWard, float minDist, float maxDist, out Vector3 targetPosition)
        {
            return Controller.GetAccessiblePointInStraightLineWithSightLine (enemy, playerPos, minPlayerDistance, maxPlayerDistance, sightLineRequired,
                canGoBehindPlayer, canGoBackWard, minDist, maxDist, out targetPosition);
        }

        public abstract void Interrupt (Vector3 origin);
    }
}