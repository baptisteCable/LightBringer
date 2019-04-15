using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Enemies
{
    public class Controller : MonoBehaviour
    {
        protected const float TARGET_DETECTION_DISTANCE = 100f;

        // Components
        public Motor motor;
        protected NavMeshAgent agent;

        // Behaviours
        protected EnemyBehaviour currentBehaviour;
        protected EnemyBehaviour nextActionBehaviour;
        public bool passive = false;

        // Target
        public Transform target;

        protected void BaseStart()
        {
            motor = GetComponent<Motor>();
            agent = motor.agent;
            agent.destination = transform.position;
        }

        protected void SelectTarget()
        {
            LayerMask mask = LayerMask.GetMask("Player");
            Collider[] cols = Physics.OverlapSphere(transform.position, TARGET_DETECTION_DISTANCE, mask);

            // Random choice
            int index = (int)(Random.value * cols.Length);
            target = cols[index].transform;
        }
    }
}