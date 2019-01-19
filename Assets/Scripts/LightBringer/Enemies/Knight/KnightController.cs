using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(KnightMotor))]
    public class KnightController : MonoBehaviour
    {
        private const int ATTACK1 = 0;
        private const int ATTACK2 = 1;
        private const int ATTACK3 = 2;
        private const float ATTACK1_CD = 6f;
        private const float ATTACK2_CD = 15f;
        private const float ATTACK3_CD = 11f;

        // Components
        KnightMotor motor;
        private NavMeshAgent agent;

        // Behaviours
        private KnightBehaviour currentBehaviour;
        private bool readyForNext = true;

        // CD
        private float[] remainingCD;
        private bool[] CDUp;

        // Environment
        public Transform target;

        // Use this for initialization
        void Start()
        {
            motor = GetComponent<KnightMotor>();
            agent = GetComponent<NavMeshAgent>();
            agent.destination = transform.position;

            // last behaviour
            currentBehaviour = new WaitBehaviour(motor, 2f);

            // CD
            remainingCD = new float[3];
            CDUp = new bool[3];
            CDUp[ATTACK1] = false;
            CDUp[ATTACK2] = false;
            CDUp[ATTACK3] = false;
            remainingCD[ATTACK1] = 0f;
            remainingCD[ATTACK2] = ATTACK2_CD;
            remainingCD[ATTACK3] = ATTACK3_CD;
        }

        // Update is called once per frame
        void Update()
        {
            if (!motor.statusManager.isDead)
            {
                RefreshCD();

                if (!readyForNext)
                {
                    currentBehaviour.Run();
                }

                // New behaviour after run to have 1 frame to compute agent path
                if (readyForNext)
                {
                    ComputeNextBehaviour();
                    readyForNext = false;
                }

                // let one frame for animator before next behaviour
                if (currentBehaviour.complete)
                {
                    readyForNext = true;
                }
            }
        }

        private void RefreshCD()
        {
            for (int i = 0; i < CDUp.Length; i++)
            {
                if (!CDUp[i])
                {
                    remainingCD[i] -= Time.deltaTime;
                    if (remainingCD[i] < 0)
                    {
                        CDUp[i] = true;
                    }
                }
            }
        }

        private void ComputeNextBehaviour()
        {
            Dictionary<KnightBehaviour, float> list = new Dictionary<KnightBehaviour, float>();
            float weight = 0;

            // Wait behaviour
            weight = 1f;
            if (currentBehaviour.GetType() == typeof(WaitBehaviour))
            {
                weight -= .5f;
            }
            //weight = 1000000f; // Debug
            list.Add(new WaitBehaviour(motor, .5f * Random.value + .5f), weight);

            // Wait and rotate behaviour
            weight = 1.5f;
            if (currentBehaviour.GetType() == typeof(WaitAndRotateBehaviour))
            {
                weight -= .5f;
            }
            list.Add(new WaitAndRotateBehaviour(motor, Random.value * .8f + .2f, target), weight);

            // Go to player behaviour
            weight = 0;
            if ((target.position - motor.transform.position).magnitude > 4)
            {
                weight = ((target.position - motor.transform.position).magnitude - 4) / 2f;
            }
            list.Add(new GoToPointBehaviour(motor, 3f, target), weight);

            // Go around player
            weight = 1f;
            list.Add(new GoAroundPlayerBehaviour(motor, .5f + Random.value * 1.2f, target, Random.value < .5f), weight);

            // Side steps
            weight = 1.5f;
            list.Add(new SideStepsBehaviour(motor, .5f + Random.value * 1.2f, target), weight);

            // Attack 1 behaviour
            weight = 0f;
            if (CDUp[ATTACK1])
            {
                if ((target.position - motor.transform.position).magnitude < 5f)
                {
                    weight = 8f;
                }
                else if ((target.position - motor.transform.position).magnitude < 10f)
                {
                    weight = 8f * (10f - (target.position - motor.transform.position).magnitude) / (10f - 5f);
                }
            }
            weight = 1000000f; // Debug
            list.Add(new Attack1Behaviour(motor, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO,
                motor.Attack1Indicator1, motor.Attack1Indicator2, motor.Attack1Indicator3), weight);

            // Attack 2 behaviour
            weight = 0f;
            if (CDUp[ATTACK2] && (target.position - motor.transform.position).magnitude < 22f)
            {
                weight = 10f;
            }
            list.Add(new Attack2Behaviour(motor, target), weight);

            // Attack 3 behaviour
            weight = 0f;
            if (CDUp[ATTACK3])
            {
                if ((target.position - motor.transform.position).magnitude < 6f)
                {
                    weight = 10f;
                }
                else if ((target.position - motor.transform.position).magnitude < 20f)
                {
                    weight = 10f * (20f - (target.position - motor.transform.position).magnitude) / (20f - 6f);
                }
            }
            list.Add(new Attack3Behaviour(motor, motor.attack3act1GO, motor.attack3act2GO, motor.shieldCollider,
                motor.Attack3Indicator1, motor.Attack3Indicator2), weight);

            // Determine next behaviour from list
            ActivateNextBehaviourFromDictionary(list);
        }

        private void ActivateNextBehaviourFromDictionary(Dictionary<KnightBehaviour, float> list)
        {
            currentBehaviour = null;

            if (list.Count == 0)
            {
                Debug.LogError("Empty list of Knight Behaviour");
                currentBehaviour = new WaitAndRotateBehaviour(motor, Random.value * 1.5f, target);
                return;
            }

            list = NormalizedDictionary(list);

            float rnd = Random.value;
            float sum = 0;

            Dictionary<KnightBehaviour, float>.Enumerator en = list.GetEnumerator();
            while (currentBehaviour == null)
            {
                sum += en.Current.Value;
                if (sum > rnd)
                {
                    SetBehaviour(en.Current.Key);
                }
                else
                {
                    en.MoveNext();
                }
            }
        }

        private void SetBehaviour(KnightBehaviour behaviour)
        {
            if (behaviour.GetType() == typeof(Attack1Behaviour))
            {
                CDUp[ATTACK1] = false;
                remainingCD[ATTACK1] = ATTACK1_CD;
            }
            else if (behaviour.GetType() == typeof(Attack2Behaviour))
            {
                CDUp[ATTACK2] = false;
                remainingCD[ATTACK2] = ATTACK2_CD;
            }
            else if (behaviour.GetType() == typeof(Attack3Behaviour))
            {
                CDUp[ATTACK3] = false;
                remainingCD[ATTACK3] = ATTACK3_CD;
            }
            currentBehaviour = behaviour;
            currentBehaviour.Init();
        }

        private static Dictionary<KnightBehaviour, float> NormalizedDictionary(Dictionary<KnightBehaviour, float> list)
        {
            float sum = 0;

            foreach (KeyValuePair<KnightBehaviour, float> pair in list)
            {
                sum += pair.Value;
            }

            Dictionary<KnightBehaviour, float> normalized = new Dictionary<KnightBehaviour, float>();

            foreach (KeyValuePair<KnightBehaviour, float> pair in list)
            {
                normalized.Add(pair.Key, pair.Value / sum);
            }

            return normalized;
        }
    }
}
