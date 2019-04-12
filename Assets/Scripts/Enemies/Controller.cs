using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Enemies
{
    public class Controller : MonoBehaviour
    {

        // Components
        public Motor motor;
        protected NavMeshAgent agent;

        // Behaviours
        protected EnemyBehaviour currentBehaviour;
        protected EnemyBehaviour nextActionBehaviour;
        public bool passive = false;

        protected void BaseStart()
        {
            motor = GetComponent<Motor>();
            agent = motor.agent;
            agent.destination = transform.position;
        }
    }
}