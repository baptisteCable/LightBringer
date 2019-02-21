using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(KnightMotor))]
    public class KnightController : Controller
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
        private Behaviour currentBehaviour;
        private bool readyForNext = true;
        public bool passive = false;

        // CD
        private float[] remainingCD;
        private bool[] CDUp;

        // Target
        public Transform target;
        public float targetModificationTime;

        // Use this for initialization
        void Start()
        {
            motor = GetComponent<KnightMotor>();
            agent = motor.agent;
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
            if (motor.statusManager.isDead)
            {
                if (currentBehaviour != null && !currentBehaviour.complete)
                {
                    currentBehaviour.Abort();
                }
            }
            else
            {
                RefreshCD();

                if (!readyForNext)
                {
                    currentBehaviour.Run();
                }

                // New behaviour after run to have 1 frame to compute agent path
                if (readyForNext)
                {
                    // Create the list of possible behaviours, depending on the situation
                    Dictionary<Behaviour, float> list = ComputeBehaviourList();

                    // Determine next behaviour from list
                    ActivateNextBehaviourFromDictionary(list);

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

        private Dictionary<Behaviour, float> ComputeBehaviourList()
        {
            Dictionary<Behaviour, float> list = new Dictionary<Behaviour, float>();
            float weight = 0;

            // Passive case
            if (passive)
            {
                list.Add(new WaitBehaviour(motor, 1.5f * Random.value + .5f), 1f);
                return list;
            }

            // Wait behaviour
            weight = 1f;
            if (currentBehaviour.GetType() == typeof(WaitBehaviour))
            {
                weight -= .5f;
            }
            list.Add(new WaitBehaviour(motor, 1.5f * Random.value + .5f), weight);

            // Random move
            weight = 1f;
            if (currentBehaviour.GetType() == typeof(RandomMove))
            {
                weight -= .5f;
            }
            list.Add(new RandomMove(motor, target), weight);

            // If no target, find one
            if (target == null)
            {
                // find a target
                weight = .5f;
                list.Add(new FindTargetBehaviour(motor), weight);

                // no other possible options
                return list;
            }
            else
            {
                // lose the current target
                weight = (Time.time - targetModificationTime) / 10f;
                list.Add(new LoseTargetBehaviour(motor), weight);
            }

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
            list.Add(new GoToTargetBehaviour(motor, 3f, target), weight);

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
            list.Add(new Attack1Behaviour(motor, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO), weight);

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
            list.Add(new Attack3Behaviour(motor, motor.attack3act1GO, motor.attack3act2GO, motor.shieldCollider), weight);

            return list;
        }

        private void ActivateNextBehaviourFromDictionary(Dictionary<Behaviour, float> list)
        {
            currentBehaviour = null;

            list = NormalizedDictionary(list);

            if (list.Count == 0)
            {
                Debug.LogError("Empty list of Knight Behaviour");
                currentBehaviour = new WaitBehaviour(motor, Random.value * 1.5f);
                return;
            }

            float rnd = Random.value;
            float sum = 0;

            Dictionary<Behaviour, float>.Enumerator en = list.GetEnumerator();
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

        private void SetBehaviour(Behaviour behaviour)
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

        private static Dictionary<Behaviour, float> NormalizedDictionary(Dictionary<Behaviour, float> list)
        {
            float sum = 0;

            foreach (KeyValuePair<Behaviour, float> pair in list)
            {
                sum += pair.Value;
            }

            if (sum < .0001f)
            {
                return new Dictionary<Behaviour, float>();
            }

            Dictionary<Behaviour, float> normalized = new Dictionary<Behaviour, float>();

            foreach (KeyValuePair<Behaviour, float> pair in list)
            {
                normalized.Add(pair.Key, pair.Value / sum);
            }

            return normalized;
        }
    }
}
