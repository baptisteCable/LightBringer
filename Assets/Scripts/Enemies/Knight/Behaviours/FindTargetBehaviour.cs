using System.Collections.Generic;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class FindTargetBehaviour : Behaviour
    {
        private const float MAX_CYCLE_DURATION = 4f;
        private const float MIN_CYCLE_DURATION = .5f;
        private const int MAX_CYCLE_NUMBER = 6;
        private const float DETECTION_RANGE = 30f;

        private int cycleCounter;
        private float cycleStartTime;
        private float cycleEndTime;

        private Collider target;

        public FindTargetBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
        }

        public override void Init()
        {
            base.Init();
            ((KnightController)em.controller).target = null;
            cycleCounter = 0;
            StartCycle();
        }

        private void StartCycle()
        {
            cycleCounter += 1;
            cycleStartTime = Time.time;
            float duration = (MAX_CYCLE_NUMBER - cycleCounter) / (MAX_CYCLE_NUMBER - 1f) * MAX_CYCLE_DURATION
                           + (cycleCounter - 1f) / (MAX_CYCLE_NUMBER - 1f) * MIN_CYCLE_DURATION;
            cycleEndTime = cycleStartTime + duration;

            // find all players within the range
            Collider[] colliders = Physics.OverlapSphere(em.transform.position, DETECTION_RANGE);
            List<Collider> players = new List<Collider>();
            foreach (Collider col in colliders)
            {
                PlayerMotor pm = col.GetComponent<PlayerMotor>();
                if (pm != null)
                {
                    players.Add(col);
                }
            }

            // find all visible players (closer than 5m or direct sightline)
            List<Collider> visiblePlayers = new List<Collider>();
            foreach (Collider col in players)
            {
                if (IsVisible(col))
                {
                    visiblePlayers.Add(col);
                }
            }

            // Take a random player in visible players
            if (visiblePlayers.Count == 0)
            {
                target = null;
                em.head.NoTarget();
            }
            else
            {
                int index = (int) (Random.value * visiblePlayers.Count);
                target = visiblePlayers[index];
                em.head.LookAroundTarget(target.transform, 1f);
            }

        }

        /// <summary>
        /// Checks if the player is closer than 5m or has a direct sightline with the monster
        /// </summary>
        /// <param name="col">The player's collider</param>
        /// <returns></returns>
        private bool IsVisible(Collider col)
        {
            float distance = Vector3.Distance(col.transform.position, em.transform.position);
            if (distance <= 5)
            {
                return true;
            }

            Vector3 direction = (col.transform.position - em.transform.position).normalized;
            Vector3 origin = em.transform.position + direction * 5f + Vector3.up;
            int mask = LayerMask.NameToLayer("Environment");
            return !Physics.Raycast(origin, direction, distance, mask);
        }

        public override void Run()
        {
            if (Time.time > cycleEndTime)
            {
                EndCycle();

                if (((KnightController)em.controller).target != null || cycleCounter >= MAX_CYCLE_NUMBER)
                {
                    End();
                    return;
                }

                StartCycle();
            }

            RunCycle();
        }

        private void RunCycle()
        {
            if (target == null)
            {
                return;
            }

            em.head.lookAroundError = (cycleEndTime - Time.time) / (cycleEndTime - cycleStartTime);
            em.DelayedRotateTowards(target.transform.position, .3f);
        }

        private void EndCycle()
        {
            if (target == null)
            {
                return;
            }

            if (IsVisible(target))
            {
                ((KnightController)em.controller).target = target.transform;
                ((KnightController)em.controller).targetModificationTime = Time.time;
                em.head.LookAtTarget(target.transform);
            }
        }

        public override void End()
        {
            base.End();
        }
    }
}