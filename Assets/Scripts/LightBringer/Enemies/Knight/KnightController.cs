using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Knight
{
    [RequireComponent(typeof(KnightMotor))]
    public class KnightController : MonoBehaviour
    {
        // Components
        KnightMotor motor;
        private NavMeshAgent agent;

        // Behaviours
        public KnightBehaviour currentBehaviour = null;
        float timeToNextBehaviour = 2f;
        bool waitForNextBehaviour = true;

        // Environment
        public Transform target;

        // Use this for initialization
        void Start()
        {
            motor = GetComponent<KnightMotor>();
            agent = GetComponent<NavMeshAgent>();
            agent.destination = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (!waitForNextBehaviour && currentBehaviour == null)
            {
                waitForNextBehaviour = true;
            }

            if (waitForNextBehaviour)
            {
                timeToNextBehaviour -= Time.deltaTime;
            }

            if (currentBehaviour != null)
            {
                currentBehaviour.Run();
                if (currentBehaviour.complete)
                {
                    currentBehaviour = null;
                }
            }

            // New behaviour after run to have 1 frame to compute agent path
            if (waitForNextBehaviour && currentBehaviour == null && timeToNextBehaviour <= 0f)
            {
                agent.isStopped = false;
                agent.destination = transform.position;
                currentBehaviour = new Attack1Behaviour(motor, 3f, target, motor.attack1act1GO, motor.attack1act2GO, motor.attack1act3GO);
                //currentBehaviour = new GoToPointBehaviour(motor, 3f, target);
                timeToNextBehaviour = 2f;
                waitForNextBehaviour = false;
            }
        }
    }
}
