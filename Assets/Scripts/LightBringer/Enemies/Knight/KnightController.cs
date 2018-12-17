using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace LightBringer.Knight
{
    [RequireComponent(typeof(KnightMotor))]
    public class KnightController : MonoBehaviour
    {
        // Components
        KnightMotor motor;
        private NavMeshAgent agent;

        // Behaviours
        private KnightBehaviour currentBehaviour;
        private KnightBehaviour lastBehaviour;
        private bool readyForNext = false;

        // Environment
        public Transform target;

        // Use this for initialization
        void Start()
        {
            motor = GetComponent<KnightMotor>();
            agent = GetComponent<NavMeshAgent>();
            agent.destination = transform.position;
            currentBehaviour = new GoToPointBehaviour(motor, 3f, target);
            lastBehaviour = new WaitBehaviour(motor, 2f);
        }

        // Update is called once per frame
        void Update()
        {
            if (!readyForNext)
            {
                currentBehaviour.Run();
            }

            // New behaviour after run to have 1 frame to compute agent path
            if (readyForNext)
            {
                ComputeNextBehaviour();
                currentBehaviour.Init();
                readyForNext = false;
            }

            // let one frame for animator before next behaviour
            if (currentBehaviour.complete)
            {
                readyForNext = true;
            }
        }

        private void ComputeNextBehaviour()
        {
            Dictionary<KnightBehaviour, float> list = new Dictionary<KnightBehaviour, float>();

            if (currentBehaviour.GetType() == typeof(WaitBehaviour))
            {
                list.Add(new WaitAndRotateBehaviour(motor, Random.value * 1.5f, target), 1f);

                if (lastBehaviour.GetType() != typeof(WaitBehaviour))
                {
                    list.Add(new WaitBehaviour(motor, Random.value + .5f), .5f);
                }
                
                if ((target.position - motor.transform.position).magnitude > 4)
                {
                    list.Add(new GoToPointBehaviour(motor, 3f, target), (target.position - motor.transform.position).magnitude / 2f);
                }
                
                if ((target.position - motor.transform.position).magnitude < 8)
                {
                    list.Add(new Attack1Behaviour(motor, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO), 4f);
                }
            }

            else if (currentBehaviour.GetType() == typeof(WaitAndRotateBehaviour))
            {
                list.Add(new WaitAndRotateBehaviour(motor, Random.value * 1.5f, target), .5f);

                if (lastBehaviour.GetType() != typeof(WaitBehaviour))
                {
                    list.Add(new WaitBehaviour(motor, Random.value + .5f), .5f);
                }

                if ((target.position - motor.transform.position).magnitude > 4)
                {
                    list.Add(new GoToPointBehaviour(motor, 3f, target), (target.position - motor.transform.position).magnitude / 2f);
                }
                
                if ((target.position - motor.transform.position).magnitude < 8)
                {
                    list.Add(new Attack1Behaviour(motor, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO), 4f);
                }
            }

            else if (currentBehaviour.GetType() == typeof(GoToPointBehaviour))
            {
                list.Add(new WaitAndRotateBehaviour(motor, Random.value * 2f, target), 1f);

                list.Add(new WaitBehaviour(motor, Random.value * 2f), .5f);
                
                if ((target.position - motor.transform.position).magnitude > 4)
                {
                    list.Add(new GoToPointBehaviour(motor, 3f, target), (target.position - motor.transform.position).magnitude / 2f);
                }
                
                if ((target.position - motor.transform.position).magnitude < 8)
                {
                    list.Add(new Attack1Behaviour(motor, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO), 8f);
                }
            }
            
            else if (currentBehaviour.GetType() == typeof(Attack1Behaviour))
            {
                list.Add(new WaitAndRotateBehaviour(motor, Random.value * 1.2f + .3f, target), 1f);

                if (lastBehaviour.GetType() != typeof(WaitBehaviour))
                {
                    list.Add(new WaitBehaviour(motor, Random.value * .8f + .5f), 4f);
                }

                if ((target.position - motor.transform.position).magnitude > 4)
                {
                    list.Add(new GoToPointBehaviour(motor, 3f, target), (target.position - motor.transform.position).magnitude / 3f);
                }
            }

            else
            {
                Debug.Log("No current Behaviour for Knight...");
            }
            
            ActivateNextBehaviourFromDictionary(list);
        }

        private void ActivateNextBehaviourFromDictionary(Dictionary<KnightBehaviour, float> list)
        {
            lastBehaviour = currentBehaviour;
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
                    currentBehaviour = en.Current.Key;
                }
                else
                {
                    en.MoveNext();
                }
            }

            Debug.Log(currentBehaviour);
        }

        private Dictionary<KnightBehaviour, float> NormalizedDictionary(Dictionary<KnightBehaviour, float> list)
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
