using System.Collections.Generic;
using UnityEngine;

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
        private const float TRANSITION_DURATION = 2f;

        // Component
        [HideInInspector] public KnightMotor km;

        // Use this for initialization
        void Start()
        {
            BaseStart();

            km = GetComponent<KnightMotor>();

            // last behaviour
            currentBehaviour = new WaitBehaviour(km, 2f);
            nextActionBehaviour = null;

            SelectTarget();

            motor.head.LookAtTarget(target.gameObject);
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
                if (!currentBehaviour.complete)
                {
                    currentBehaviour.Run();
                }

                if (currentBehaviour.complete)
                {
                    ComputeNextActionAndTransitionBehaviours();
                }
            }
        }

        private void ComputeNextActionAndTransitionBehaviours()
        {
            // If current behaviour is a transition behaviour, start action behaviour
            if (currentBehaviour != nextActionBehaviour && nextActionBehaviour != null)
            {
                SetBehaviour(nextActionBehaviour);
            }

            // Else compute the next action behaviour
            else
            {
                // Find target
                SelectTarget();

                // Create the list of possible behaviours, depending on the situation
                Dictionary<EnemyBehaviour, float> dic = ActionBehaviourList();

                // Determine next action behaviour from list
                nextActionBehaviour = SelectBehaviourFromDictionary(dic);

                // Create the list of possible behaviours, depending on the situation
                dic = TransistionBehaviourList();

                // Determine next transition behaviour from list and start it
                EnemyBehaviour transitionBehaviour = SelectBehaviourFromDictionary(dic);
                SetBehaviour(transitionBehaviour);

            }
        }

        private void SetBehaviour(EnemyBehaviour behaviour)
        {
            currentBehaviour = behaviour;
            currentBehaviour.Init();
        }

        private Dictionary<EnemyBehaviour, float> ActionBehaviourList()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
            float weight;

            // Passive case
            if (passive)
            {
                dic.Add(new WaitBehaviour(km, TRANSITION_DURATION), 1f);
                return dic;
            }

            // Attack 1 behaviour
            weight = 0f;
            if (currentBehaviour.GetType() != typeof(Attack1Behaviour))
            {
                float distance = (target.position - motor.transform.position).magnitude;
                if (distance < 5f)
                {
                    weight = 8f;
                }
                else if (distance < 20f)
                {
                    weight = 5f;
                }
                else
                {
                    weight = 100f / distance;
                }
            }
            dic.Add(new Attack1Behaviour(km, target, km.attack1actGO, km.attack1Container, km.attack1GroundActGOPrefab,
                km.attack1GroundRendererGOPrefab), weight);

            // Attack 2 behaviour
            weight = 0f;
            if (currentBehaviour.GetType() != typeof(Attack2Behaviour))
            {
                if ((target.position - motor.transform.position).magnitude < 25f)
                {
                    weight = 10f;
                }
            }
            dic.Add(new Attack2Behaviour(km, target), weight);

            // Attack 3 behaviour
            weight = 0f;
            if (currentBehaviour.GetType() != typeof(Attack3Behaviour))
            {
                float distance = (target.position - motor.transform.position).magnitude;
                if (distance < 5f)
                {
                    weight = 10f;
                }
                else if (distance < 20f)
                {
                    weight = 5f;
                }
                else
                {
                    weight = 100f / distance;
                }
            }
            dic.Add(new Attack3Behaviour(km, km.attack3act1GO, km.attack3act2GO, km.shieldCollider), weight);

            // Attack 4 behaviour
            weight = 0f;
            if (currentBehaviour.GetType() != typeof(Attack4Behaviour))
            {
                float distance = (target.position - motor.transform.position).magnitude;
                if (distance < 5f)
                {
                    weight = 2f;
                }
                else if (distance < 30f)
                {
                    weight = 12f;
                }
                else
                {
                    weight = 5f;
                }
            }
            weight = 10000; // Debug
            dic.Add(new Attack4Behaviour(km, target), weight);

            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourList()
        {
            // Passive case
            if (passive || nextActionBehaviour.GetType() == typeof(WaitBehaviour))
            {
                Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
                dic.Add(new WaitBehaviour(km, TRANSITION_DURATION), 1f);
                return dic;
            }

            if (nextActionBehaviour.GetType() == typeof(Attack1Behaviour))
            {
                return TransistionBehaviourListAfterAttack1();
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack2Behaviour))
            {
                return TransistionBehaviourListAfterAttack2();
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack3Behaviour))
            {
                return TransistionBehaviourListAfterAttack3();
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack4Behaviour))
            {
                return TransistionBehaviourListAfterAttack4();
            }
            else
            {
                Debug.Log("Invalid next action behaviour");
                return new Dictionary<EnemyBehaviour, float>();
            }
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListAfterAttack1()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
            float weight;

            // Wait behaviour
            if ((target.position - motor.transform.position).magnitude < 15f)
            {
                weight = 1f;
            }
            else
            {
                weight = .1f;
            }
            dic.Add(new WaitBehaviour(km, TRANSITION_DURATION), weight);

            // Charge to position
            weight = 0;
            if ((target.position - motor.transform.position).magnitude > 13)
            {
                weight = 1f;
            }
            else
            {
                weight = .2f;
            }
            dic.Add(new GoToPointBehaviour(km, target.position), weight);

            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListAfterAttack2()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
            dic.Add(new WaitBehaviour(km, TRANSITION_DURATION), 1f);
            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListAfterAttack3()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
            float weight;

            // Wait behaviour
            if ((target.position - motor.transform.position).magnitude < 10)
            {
                weight = 1f;
            }
            else
            {
                weight = .2f;
            }
            dic.Add(new WaitBehaviour(km, TRANSITION_DURATION), weight);

            // Go to position
            weight = 0;
            if ((target.position - motor.transform.position).magnitude > 13)
            {
                weight = 1f;
            }
            else
            {
                weight = .2f;
            }
            dic.Add(new GoToPointBehaviour(km, target.position), weight);

            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListAfterAttack4()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
            float weight;

            // Wait behaviour
            if ((target.position - motor.transform.position).magnitude < 15f)
            {
                weight = 1f;
            }
            else
            {
                weight = .1f;
            }
            weight = 10000; // Debug
            dic.Add(new WaitBehaviour(km, TRANSITION_DURATION), weight);

            // Go to position
            weight = 0;
            if ((target.position - motor.transform.position).magnitude > 13)
            {
                weight = 1f;
            }
            else
            {
                weight = .2f;
            }
            dic.Add(new GoToPointBehaviour(km, target.position), weight);

            return dic;
        }


        private EnemyBehaviour SelectBehaviourFromDictionary(Dictionary<EnemyBehaviour, float> dic)
        {
            currentBehaviour = null;

            dic = NormalizedDictionary(dic);

            if (dic.Count == 0)
            {
                Debug.LogError("No behaviour in list");
                return new WaitBehaviour(km, TRANSITION_DURATION);
            }

            float rnd = Random.value;
            float sum = 0;

            foreach (KeyValuePair<EnemyBehaviour, float> pair in dic)
            {
                sum += pair.Value;
                if (sum > rnd)
                {
                    return pair.Key;
                }
            }

            Debug.Log("No behaviour selected");
            return new WaitBehaviour(km, TRANSITION_DURATION);
        }

        private static Dictionary<EnemyBehaviour, float> NormalizedDictionary(Dictionary<EnemyBehaviour, float> dic)
        {
            float sum = 0;

            foreach (KeyValuePair<EnemyBehaviour, float> pair in dic)
            {
                sum += pair.Value;
            }

            if (sum < .0001f)
            {
                return new Dictionary<EnemyBehaviour, float>();
            }

            Dictionary<EnemyBehaviour, float> normalized = new Dictionary<EnemyBehaviour, float>();

            foreach (KeyValuePair<EnemyBehaviour, float> pair in dic)
            {
                normalized.Add(pair.Key, pair.Value / sum);
            }

            return normalized;
        }
    }
}
