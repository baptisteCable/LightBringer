using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(KnightMotor))]
    public class KnightController : Controller
    {
        // Component
        [HideInInspector] public KnightMotor km;

        private int waitCounter = 0;

        // Use this for initialization
        void Start()
        {
            BaseStart();

            km = GetComponent<KnightMotor>();

            // last behaviour
            currentBehaviour = new WaitBehaviour(km);
            nextActionBehaviour = null;

            SelectTarget();
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
                // Change mode if needed
                ChangeMode();

                // Find target
                SelectTarget();

                // if no target, do nothing (TODO: improve it and make it passive after several tries)
                if (target == null)
                {
                    nextActionBehaviour = new WaitBehaviour(km);
                    SetBehaviour(new WaitBehaviour(km));
                }

                else
                {
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
        }

        private void ChangeMode()
        {
            if (motor.statusManager.mode == Mode.Fight && motor.statusManager.nextMode == Mode.Rage)
            {
                motor.statusManager.RageStart();
            }
            else if (motor.statusManager.mode == Mode.Rage && motor.statusManager.nextMode == Mode.Exhaustion)
            {
                motor.statusManager.ExhaustionStart();
            }
            else if (motor.statusManager.mode == Mode.Exhaustion && motor.statusManager.nextMode == Mode.Fight)
            {
                motor.statusManager.ExhaustionEnd();
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
                AddWaitBehaviour(dic, 1f);
                return dic;
            }

            // Rage starting
            if (motor.statusManager.rageToBeStarted && motor.statusManager.mode == Mode.Rage)
            {
                dic.Add(new StartRageBehaviour(km), 1f);
                motor.statusManager.rageToBeStarted = false;
                return dic;
            }

            // Exhaustion Starting
            if (motor.statusManager.exhaustionToBeStarted && motor.statusManager.mode == Mode.Exhaustion)
            {
                dic.Add(new WaitBehaviour(km), 1f);
                motor.statusManager.exhaustionToBeStarted = false;
                return dic;
            }

            // Exhaustion Ending
            if (motor.statusManager.exhaustionToBeEnded && motor.statusManager.mode == Mode.Fight)
            {
                dic.Add(new EndExhaustionBehaviour(km), 1f);
                motor.statusManager.exhaustionToBeEnded = false;
                return dic;
            }

            // Attack 1 behaviour
            weight = 0f;
            if (currentBehaviour.GetType() != typeof(Attack1Behaviour))
            {
                float distance = (target.position - motor.transform.position).magnitude;
                if (distance < 20f)
                {
                    weight = 10f;
                }
                else
                {
                    weight = 200f / distance;
                }
            }
            weight = 100000000; // DEBUG
            dic.Add(new Attack1Behaviour(km, target, km.attack1GroundActGOPrefab, km.attack1GroundRendererGOPrefab), weight);

            // Attack 2 behaviour
            weight = 0f;
            if (currentBehaviour.GetType() != typeof(Attack2Behaviour))
            {
                if ((target.position - motor.transform.position).magnitude < 25f)
                {
                    weight = 10f;
                }
                else
                {
                    weight = 15f;
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
                    weight = 15f;
                }
            }
            dic.Add(new Attack4Behaviour(km, target), weight);

            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourList()
        {
            // Passive case
            if (passive || nextActionBehaviour.GetType() == typeof(WaitBehaviour))
            {
                Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
                AddWaitBehaviour(dic, 1f);
                return dic;
            }

            // Rage start case
            if (nextActionBehaviour.GetType() == typeof(StartRageBehaviour))
            {
                Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
                AddWaitBehaviour(dic, 1f);
                return dic;
            }
            // End exhaustion case
            else if (nextActionBehaviour.GetType() == typeof(EndExhaustionBehaviour))
            {
                Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
                AddWaitBehaviour(dic, 1f);
                return dic;
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack1Behaviour))
            {
                return TransistionBehaviourListBeforeAttack1();
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack2Behaviour))
            {
                return TransistionBehaviourListBeforeAttack2();
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack3Behaviour))
            {
                return TransistionBehaviourListBeforeAttack3();
            }
            else if (nextActionBehaviour.GetType() == typeof(Attack4Behaviour))
            {
                return TransistionBehaviourListBeforeAttack4();
            }
            else
            {
                Debug.LogError("Invalid next action behaviour");
                return new Dictionary<EnemyBehaviour, float>();
            }
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListBeforeAttack1()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();

            Vector3 destination;

            // if good way to have a sight line after charge
            if (CanFindTargetPoint(
                    motor.transform, target.position, 5, 25, true, true, true,
                    Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination)
                   )
            {
                AddMovementBehaviour(dic, destination, 1f);

                // if sight line and not too far (before charge), prefer waiting behaviour
                if ((target.position - motor.transform.position).magnitude < 25f && hasSightLine(target.position, motor.transform.position))
                {
                    AddWaitBehaviour(dic, 3f);
                }
            }
            else
            {
                // if sight line and not too far , waiting behaviour
                if ((target.position - motor.transform.position).magnitude < 25f && hasSightLine(target.position, motor.transform.position))
                {
                    AddWaitBehaviour(dic, 3f);
                }
                // else, no way to be in right position, thus random move
                else
                {
                    if (CanFindTargetPoint(
                            motor.transform, target.position, 5, 50, false, true, false,
                            Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination))
                    {
                        AddMovementBehaviour(dic, destination, 3f);
                    }

                    // or wait
                    AddWaitBehaviour(dic, 1f);
                }
            }

            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListBeforeAttack2()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();
            AddWaitBehaviour(dic, 1f);
            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListBeforeAttack3()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();

            Vector3 destination;

            // if good way to have a close position
            if (CanFindTargetPoint(
                    motor.transform, target.position, 6f, false, true,
                    Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination)
                   )
            {
                AddMovementBehaviour(dic, destination, 1f);

                // if not too far (before charge), can also trigger waiting behaviour
                if ((target.position - motor.transform.position).magnitude < 12f)
                {
                    AddWaitBehaviour(dic, 1f);
                }
            }
            else
            {
                // if not too far, waiting behaviour
                if ((target.position - motor.transform.position).magnitude < 15f)
                {
                    AddWaitBehaviour(dic, 1f);
                }
                // try a closer not optimal move
                else if (CanFindTargetPoint(
                    motor.transform, target.position, 3, 15f, false, true, true,
                    Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination)
                   )
                {
                    AddMovementBehaviour(dic, destination, 1f);
                }
                // else, no way to be in right position, thus random move
                else
                {
                    if (CanFindTargetPoint(
                            motor.transform, target.position, 5, 50, false, true, true,
                            Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination))
                    {
                        AddMovementBehaviour(dic, destination, 1f);
                    }

                    // or wait
                    AddWaitBehaviour(dic, 1f);
                }
            }

            return dic;
        }

        private Dictionary<EnemyBehaviour, float> TransistionBehaviourListBeforeAttack4()
        {
            Dictionary<EnemyBehaviour, float> dic = new Dictionary<EnemyBehaviour, float>();

            Vector3 destination;

            // if good way to have a sight line after charge
            if (CanFindTargetPoint(
                    motor.transform, target.position, 5, 25, true, true, true,
                    Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination)
                   )
            {
                AddMovementBehaviour(dic, destination, 1f);

                // if sight line and not too far (before charge), prefer waiting behaviour
                if ((target.position - motor.transform.position).magnitude < 100f && hasSightLine(target.position, motor.transform.position))
                {
                    AddWaitBehaviour(dic, 3f);
                }
            }
            else
            {
                // if sight line, waiting behaviour
                if ((target.position - motor.transform.position).magnitude < 100f && hasSightLine(target.position, motor.transform.position))
                {
                    AddWaitBehaviour(dic, 3f);
                }
                // else, no way to be in right position, thus random move
                else
                {
                    // can find with sight line but farther
                    if (CanFindTargetPoint(
                            motor.transform, target.position, 5, 50, true, true, true,
                            Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination))
                    {
                        AddMovementBehaviour(dic, destination, 3f);
                    }
                    // no sightline but closer
                    else if (CanFindTargetPoint(
                            motor.transform, target.position, 5, 20, false, true, false,
                            Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination))
                    {
                        AddMovementBehaviour(dic, destination, 3f);
                    }
                    // no sightline and not that close
                    else if (CanFindTargetPoint(
                            motor.transform, target.position, 5, 40, false, true, false,
                            Charge1Behaviour.CHARGE_MIN_RANGE, Charge1Behaviour.CHARGE_MAX_RANGE, out destination))
                    {
                        AddMovementBehaviour(dic, destination, 3f);
                    }

                    // or wait
                    AddWaitBehaviour(dic, 1f);
                }
            }

            return dic;
        }

        private void AddWaitBehaviour(Dictionary<EnemyBehaviour, float> dic, float weight)
        {
            // 1 wait over 2 is an exhaustion wait if exhausted
            waitCounter = (waitCounter + 1) % 2;

            if (motor.statusManager.mode == Mode.Exhaustion && waitCounter == 0)
            {
                dic.Add(new WaitExhaustionBehaviour(km), weight);
            }
            else
            {
                dic.Add(new WaitBehaviour(km), weight);
            }
        }

        private void AddMovementBehaviour(Dictionary<EnemyBehaviour, float> dic, Vector3 destination, float weight)
        {
            if (motor.statusManager.mode == Mode.Exhaustion)
            {
                dic.Add(new GoToPointBehaviour(km, destination), weight);
            }
            else
            {
                dic.Add(new Charge1Behaviour(km, destination), weight);
            }
        }

        private EnemyBehaviour SelectBehaviourFromDictionary(Dictionary<EnemyBehaviour, float> dic)
        {
            currentBehaviour = null;

            dic = NormalizedDictionary(dic);

            if (dic.Count == 0)
            {
                Debug.LogError("No behaviour in list");
                return new WaitBehaviour(km);
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

            Debug.LogError("No behaviour selected");
            return new WaitBehaviour(km);
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

        // Interuption 
        public override void Interrupt(Vector3 origin)
        {
            currentBehaviour.Abort();
            nextActionBehaviour = null;
            SetBehaviour(new InterruptionBehaviour(km, origin));
        }
    }
}
