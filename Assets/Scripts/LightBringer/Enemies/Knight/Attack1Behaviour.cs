using UnityEngine;
using LightBringer.Player;
using System.Collections.Generic;

namespace LightBringer.Knight
{
    public class Attack1Behaviour : KnightBehaviour, CollisionAbility
    {
        private const float DURATION = 2.315f;
        private const float CHARGE_RANGE = 12f;

        private const float POS_CHECKPOINT_1_START = 38f / 60f;
        private const float POS_CHECKPOINT_1_END = 46f / 60f;
        private const float POS_CHECKPOINT_2_START = 60f / 60f;
        private const float POS_CHECKPOINT_2_END = 70f / 60f;

        private const float DMG_CHECKPOINT_1_START = 26f / 60f;
        private const float DMG_CHECKPOINT_1_END = 29f / 60f;
        private const float DMG_CHECKPOINT_2_START = 56f / 60f;
        private const float DMG_CHECKPOINT_2_END = 58f / 60f;
        private const float DMG_CHECKPOINT_3_START = 76f / 60f;
        private const float DMG_CHECKPOINT_3_END = 118f / 60f;

        // Colliders GO
        public GameObject act1GO;
        public GameObject act2GO;
        public GameObject act3GO;
        private AbilityColliderTrigger act1;
        private AbilityColliderTrigger act2;
        private AbilityColliderTrigger act3;

        // Collider list
        private List<Collider> cols;

        // Charge curves
        private AnimationCurve positionCurveX;
        private AnimationCurve positionCurveZ;

        // Init booleans
        private bool part1Initialized = false;
        private bool part2Initialized = false;
        private bool part3Initialized = false;

        float stopDist;
        Transform target;

        float ellapsedTime = 0f;

        public Attack1Behaviour(KnightMotor enemyMotor, Transform target, GameObject attack1act1GO,
            GameObject attack1act2GO, GameObject attack1act3GO) : base(enemyMotor)
        {
            this.target = target;
            act1GO = attack1act1GO;
            act2GO = attack1act2GO;
            act3GO = attack1act3GO;
        }

        public override void Init()
        {
            em.anim.SetBool("castingAttack1", true);
            em.anim.Play("Attack1");
            act1 = act1GO.GetComponent<AbilityColliderTrigger>();
            act2 = act2GO.GetComponent<AbilityColliderTrigger>();
            act3 = act3GO.GetComponent<AbilityColliderTrigger>();
        }

        public override void Run()
        {
            ellapsedTime += Time.deltaTime;

            // DMG 1
            if (ellapsedTime >= DMG_CHECKPOINT_1_START && ellapsedTime <= DMG_CHECKPOINT_1_END)
            {
                InitPart1();
            }
            if (act1GO.activeSelf && ellapsedTime >= DMG_CHECKPOINT_1_END)
            {
                act1GO.SetActive(false);
                act1.UnsetAbility();
            }

            // DMG 2
            if (ellapsedTime >= DMG_CHECKPOINT_2_START && ellapsedTime <= DMG_CHECKPOINT_2_END)
            {
                InitPart2();
            }
            if (act2GO.activeSelf && ellapsedTime >= DMG_CHECKPOINT_2_END)
            {
                act2GO.SetActive(false);
                act2.UnsetAbility();
            }

            // DMG 3
            if (ellapsedTime >= DMG_CHECKPOINT_3_START && ellapsedTime <= DMG_CHECKPOINT_3_END)
            {
                InitPart3();

                // Position
                if (positionCurveX == null)
                {
                    ComputeCharge();
                }
                /*
                em.transform.position = new Vector3(
                        positionCurveX.Evaluate(ellapsedTime - DMG_CHECKPOINT_3_START),
                        em.transform.position.y,
                        positionCurveZ.Evaluate(ellapsedTime - DMG_CHECKPOINT_3_START)
                    );
                */
                em.agent.nextPosition = em.transform.position;
                em.cc.Move(em.transform.forward * 30 * Time.deltaTime);
            }
            if (act3GO.activeSelf && ellapsedTime >= DMG_CHECKPOINT_3_END)
            {
                act3GO.SetActive(false);
                act3.UnsetAbility();
            }

            // POS init, 1 AND 2
            if (ellapsedTime <= DMG_CHECKPOINT_1_START
                || (ellapsedTime >= POS_CHECKPOINT_1_START && ellapsedTime <= POS_CHECKPOINT_1_END)
                || (ellapsedTime >= POS_CHECKPOINT_2_START && ellapsedTime <= POS_CHECKPOINT_2_END))
            {
                em.RotateTowards(this.target.position - em.transform.position);
            }

            if (ellapsedTime > DURATION)
            {
                End();
            }
        }

        public void End()
        {
            em.agent.SetDestination(em.transform.position);
            em.agent.nextPosition = em.transform.position;
            em.anim.SetBool("castingAttack1", false);
            complete = true;
        }

        private void ComputeCharge()
        {
            positionCurveX = new AnimationCurve();
            positionCurveZ = new AnimationCurve();

            positionCurveX.AddKey(new Keyframe(0f, em.transform.position.x, 0f, 0f));
            positionCurveZ.AddKey(new Keyframe(0f, em.transform.position.z, 0f, 0f));

            positionCurveX.AddKey(new Keyframe(.7f, (em.transform.position + em.transform.forward * CHARGE_RANGE).x, 0f, 0f));
            positionCurveZ.AddKey(new Keyframe(.7f, (em.transform.position + em.transform.forward * CHARGE_RANGE).z, 0f, 0f));

            positionCurveX.AddKey(new Keyframe(1f, (em.transform.position + em.transform.forward * CHARGE_RANGE).x, 0f, 0f));
            positionCurveZ.AddKey(new Keyframe(1f, (em.transform.position + em.transform.forward * CHARGE_RANGE).z, 0f, 0f));
        }

        public void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
            if (col.tag == "Player")
            {
                if (abilityColliderTrigger == act1GO.GetComponent<AbilityColliderTrigger>())
                {
                    Part1(col);
                }

                if (abilityColliderTrigger == act2GO.GetComponent<AbilityColliderTrigger>())
                {
                    Part2(col);
                }

                if (abilityColliderTrigger == act3GO.GetComponent<AbilityColliderTrigger>())
                {
                    Part3(col);
                }
            }
        }

        private void Part1(Collider col)
        {
            if (!cols.Contains(col))
            {
                cols.Add(col);
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                psm.TakeDamage(15f);
            }
        }

        private void Part2(Collider col)
        {
            if (!cols.Contains(col))
            {
                cols.Add(col);
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                psm.TakeDamage(5f);
                psm.Stun(1f);
            }
        }

        private void Part3(Collider col)
        {
            if (!cols.Contains(col))
            {
                cols.Add(col);
                PlayerStatusManager psm = col.GetComponent<PlayerStatusManager>();
                psm.TakeDamage(25f);
                psm.Interrupt(1f);
            }
        }

        private void InitPart1()
        {
            if (!part1Initialized)
            {
                act1GO.SetActive(true);
                act1.SetAbility(this);
                cols = new List<Collider>();
                part1Initialized = true;
            }
        }

        private void InitPart2()
        {
            if (!part2Initialized)
            {
                act2GO.SetActive(true);
                act2.SetAbility(this);
                cols = new List<Collider>();
                part2Initialized = true;
            }
        }

        private void InitPart3()
        {
            if (!part3Initialized)
            {
                act3GO.SetActive(true);
                act3.SetAbility(this);
                cols = new List<Collider>();
                part3Initialized = true;
            }
        }
    }
}