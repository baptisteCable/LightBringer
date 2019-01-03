using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace LightBringer.Knight
{
    [RequireComponent(typeof(KnightMotor))]
    public class KnightController : MonoBehaviour
    {
        private const int ATTACK1 = 0;
        private const int ATTACK2 = 1;
        private const int ATTACK3 = 2;
        private const float ATTACK1_CD = 7f;
        private const float ATTACK2_CD = 21f;
        private const float ATTACK3_CD = 13f;

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
            remainingCD[ATTACK1] = ATTACK1_CD;
            remainingCD[ATTACK2] = ATTACK2_CD;
            remainingCD[ATTACK3] = ATTACK3_CD;
        }

        // Update is called once per frame
        void Update()
        {
            if (!motor.isDead)
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
            list.Add(new WaitBehaviour(motor, Random.value + .5f), weight);

            // Wait and rotate behaviour
            weight = 1.5f;
            if (currentBehaviour.GetType() == typeof(WaitAndRotateBehaviour))
            {
                weight -= .5f;
            }
            list.Add(new WaitAndRotateBehaviour(motor, Random.value * 2.5f + .5f, target), weight);

            // Go to player behaviour
            weight = 0;
            if ((target.position - motor.transform.position).magnitude > 4)
            {
                weight = ((target.position - motor.transform.position).magnitude - 4) / 2f;
            }
            list.Add(new GoToPointBehaviour(motor, 3f, target), weight);

            // Go around player
            weight = 1f;
            list.Add(new GoAroundPlayerBehaviour(motor, .5f + Random.value * 2.5f, target, Random.value < .5f), weight);

            // Side steps
            weight = 1.5f;
            list.Add(new SideStepsBehaviour(motor, 1f + Random.value * 4f, target), weight);

            // Attack 1 behaviour
            weight = 0f;
            if (CDUp[ATTACK1] && (target.position - motor.transform.position).magnitude < 8f)
            {
                weight = 6f + .5f * (8f - ((target.position - motor.transform.position).magnitude));
            }
            list.Add(new Attack1Behaviour(motor, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO), weight);

            // Attack 2 behaviour
            weight = 0f;
            if (CDUp[ATTACK2] && (target.position - motor.transform.position).magnitude < 20f)
            {
                weight = 10f;
            }
            list.Add(new Attack2Behaviour(motor, target), weight);

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

        private void OnGUI()
        {
            GUI.contentColor = Color.black;
            GUILayout.BeginArea(new Rect(20, 20, 250, 120));
            GUILayout.Label("Attack 1 up : " + CDUp[ATTACK1]);
            GUILayout.Label("Attack 1 cd : " + remainingCD[ATTACK1]);
            GUILayout.EndArea();
        }
    }
}
