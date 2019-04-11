﻿using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class GoAroundPlayerBehaviour : EnemyBehaviour
    {
        private Transform target;
        private float duration;
        private bool left;

        public GoAroundPlayerBehaviour(KnightMotor enemyMotor, float duration, Transform target, bool left) : base(enemyMotor)
        {
            this.duration = duration;
            this.target = target;
            this.left = left;
        }

        public override void Init()
        {
            em.SetOverrideAgent(true);
        }

        public override void Run()
        {
            Vector3 mainDir = target.position - em.transform.position;
            Vector3 leftAroundDir = Vector3.Cross(mainDir, Vector3.up);
            if (!left)
            {
                leftAroundDir = -leftAroundDir;
            }
            em.MoveInDirection(leftAroundDir);
            em.RotateTowards(target.position - em.transform.position);

            duration -= Time.deltaTime;

            if (duration < 0)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();
            em.SetOverrideAgent(false);
        }
    }
}