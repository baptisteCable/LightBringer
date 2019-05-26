using LightBringer.Abilities;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class StartRageBehaviour : CollisionBehaviour
    {
        private const float DURATION = 1.7f;
        private const float SHIELD_ON_GROUND = 39f / 60f;
        private const float PUSH_AWAY_DURATION = .2f;
        private const float STUN_DURATION = .4f;

        private KnightMotor km;

        public StartRageBehaviour(KnightMotor enemyMotor) : base(enemyMotor)
        {
            km = enemyMotor;
        }

        public override void Init()
        {
            base.Init();

            em.anim.Play("StartRage", -1, 0);

            em.SetOverrideAgent(true);

            actGOs = new GameObject[1];
            actGOs[0] = km.StartRageActGO;
            parts = new Part[1];
            parts[0] = new Part(State.Before, SHIELD_ON_GROUND, PUSH_AWAY_DURATION, -1);

            acts = new AbilityColliderTrigger[1];
            acts[0] = actGOs[0].GetComponent<AbilityColliderTrigger>();
        }

        public override void Run()
        {
            DisplayIndicators();
            StartCollisionParts();
            RunCollisionParts();

            if (Time.time >= startTime + DURATION)
            {
                End();
            }
        }
        protected override void StartCollisionPart(int i)
        {
            base.StartCollisionPart(i);

            if (i == 0)
            {
                km.startRagePs.Play(true);
                km.rage.StartRage();
            }
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

                // Stun the player and push away
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                if (psm.IsAffectedByCC(new CrowdControl(CrowdControlType.ForcedMove, DamageType.AreaOfEffect, DamageElement.Physical)))
                {
                    psm.ApplyCrowdControl(new CrowdControl(CrowdControlType.Stun, DamageType.AreaOfEffect, DamageElement.Physical), STUN_DURATION);
                    PushAway(psm, abilityColliderTrigger, col);
                }
            }
        }

        void PushAway(PlayerStatusManager psm, AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            // Push the player away
            Vector3 actPos = abilityColliderTrigger.transform.position;
            SphereCollider sc = abilityColliderTrigger.GetComponent<SphereCollider>();

            Vector3 position = col.transform.position;
            Vector3 finalPosition = position + (position - actPos).normalized * (sc.radius - (position - actPos).magnitude);

            // x and z
            AnimationCurve xCurve = new AnimationCurve();
            xCurve.AddKey(new Keyframe(0, position.x));
            xCurve.AddKey(new Keyframe(PUSH_AWAY_DURATION, finalPosition.x));

            AnimationCurve zCurve = new AnimationCurve();
            zCurve.AddKey(new Keyframe(0, position.z));
            zCurve.AddKey(new Keyframe(PUSH_AWAY_DURATION, finalPosition.z));

            // y
            AnimationCurve yCurve = new AnimationCurve();
            yCurve.AddKey(new Keyframe(0, position.y));
            yCurve.AddKey(new Keyframe(PUSH_AWAY_DURATION, position.y));

            // Add curve to player movement
            psm.playerMotor.MoveByCurve(PUSH_AWAY_DURATION, xCurve, yCurve, zCurve);
        }
    }
}