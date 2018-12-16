using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Knight
{
    public class KnightMotor : MonoBehaviour
    {
       private const float MAX_MOVE_SPEED_PASSIVE = 4f;
        private const float MAX_MOVE_SPEED_FIGHT = 12f;
        private const float MAX_MOVE_SPEED_RAGE = 20f;

        private const float ACCELERATION_PASSIVE = 4f;
        private const float ACCELERATION_FIGHT = 40f;
        private const float ACCELERATION_RAGE = 80f;

        private const float ROTATION_ACCELERATION_PASSIVE = 750f;
        private const float ROTATION_ACCELERATION_FIGHT = 1500f;
        private const float ROTATION_ACCELERATION_RAGE = 3000f;

        private const float MAX_ROTATION_SPEED_PASSIVE = 180f;
        private const float MAX_ROTATION_SPEED_FIGHT = 520f;
        private const float MAX_ROTATION_SPEED_RAGE = 800f;

        public NavMeshAgent agent;
        private Vector2 smoothDeltaPosition = Vector2.zero;
        private Vector2 velocity = Vector2.zero;

        // Movement
        private float moveSpeed;
        private float acceleration;

        // Rotation
        private float currentRotationSpeed;
        private float rotationAcceleration;
        private float rotationSpeed = 0f;
        
        // Components
        public Animator anim;

        // Colliders GO
        public GameObject attack1act1GO;
        public GameObject attack1act2GO;
        public GameObject attack1act3GO;

        //Animation acceleration smooth
        Vector3 animAcceleration;
        Vector3 newAcceleration;
        
        private void Start()
        {
            // Animator
            anim = transform.Find("EnemyContainer").GetComponent<Animator>();

            // Agent
            agent = GetComponent<NavMeshAgent>();
            agent.updatePosition = false;

            // Colliders
            attack1act1GO = transform.Find("EnemyContainer/Attack1Trigger").gameObject;
            attack1act2GO = transform.Find("EnemyContainer/Armature/BoneControlerShield/ShieldAttackTrigger").gameObject;
            attack1act3GO = transform.Find("EnemyContainer/Armature/BoneControlerSpear/SpearAttackTrigger").gameObject;
            attack1act1GO.SetActive(false);
            attack1act2GO.SetActive(false);
            attack1act3GO.SetActive(false);

            // Initial mode
            SetMode(EnemyMode.Fight);
        }

        private void Update()
        {
            //RotateTowards(characterGO.transform.position - transform.position, EnemyMode.Fight);

            Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
            transform.position = agent.nextPosition;

            // Map 'worldDeltaPosition' to local space
            float dx = Vector3.Dot(transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);


            // Low-pass filter the deltaMove
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if delta time is safe
            //if (Time.deltaTime > 1e-5f)
                velocity = smoothDeltaPosition / Time.deltaTime;

            bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

            // Update animation parameters
            anim.SetBool("isMoving", shouldMove);
            anim.SetFloat("VelX", velocity.x);
            anim.SetFloat("VelY", velocity.y);

            LookAt lookAt = GetComponent<LookAt>();
            if (lookAt)
                lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;
            
        }

        public void SetMode(int mode)
        {
            switch (mode)
            {
                case EnemyMode.Passive:
                    SetMovementParameters(MAX_MOVE_SPEED_PASSIVE, ACCELERATION_PASSIVE);
                    SetRotationParameters(MAX_ROTATION_SPEED_PASSIVE, ROTATION_ACCELERATION_PASSIVE);
                    break;
                case EnemyMode.Fight:
                    SetMovementParameters(MAX_MOVE_SPEED_FIGHT, ACCELERATION_FIGHT);
                    SetRotationParameters(MAX_ROTATION_SPEED_FIGHT, ROTATION_ACCELERATION_FIGHT);
                    break;
                case EnemyMode.Rage:
                    SetMovementParameters(MAX_MOVE_SPEED_RAGE, ACCELERATION_RAGE);
                    SetRotationParameters(MAX_ROTATION_SPEED_RAGE, ROTATION_ACCELERATION_RAGE);
                    break;
                default: throw new System.Exception("Invalid Enemy Mode");
            }
        }

        private void SetMovementParameters(float ms, float acc)
        {
            moveSpeed = ms;
            acceleration = acc;
            agent.speed = ms;
            SetProximityMovement(agent.autoBraking);
        }

        private void SetRotationParameters(float rs, float acc)
        {
            rotationSpeed = rs;
            rotationAcceleration = acc;
            agent.angularSpeed = rs;
        }

        public void SetProximityMovement(bool proximity)
        {
            if (proximity)
            {
                agent.stoppingDistance = .1f;
                agent.autoBraking = true;
            }
            else
            {
                agent.stoppingDistance = 1f / 2f * moveSpeed * moveSpeed / acceleration;
                agent.autoBraking = false;
            }
        }

        public void RotateTowards(Vector3 direction)
        {
            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
            float rotationSpeed;
            if (currentRotationSpeed * angle < 0)
            {
                rotationSpeed = rotationAcceleration * Time.fixedDeltaTime - Mathf.Abs(currentRotationSpeed);
            }
            else
            {
                rotationSpeed = Mathf.Min(
                Mathf.Abs(currentRotationSpeed) + rotationAcceleration * Time.fixedDeltaTime,
                Mathf.Abs(angle) * rotationAcceleration / 40,
                this.rotationSpeed);
            }

            currentRotationSpeed = Mathf.Sign(angle) * rotationSpeed;

            transform.Rotate(Vector3.up, currentRotationSpeed * Time.fixedDeltaTime);
        }

        public void EnableAgentRoation()
        {
            agent.angularSpeed = 0;
        }

        public void DisableAgentRotation()
        {
            agent.angularSpeed = rotationSpeed;
        }

        private void OnGUI()
        {
            GUI.contentColor = Color.black;
            GUILayout.BeginArea(new Rect(20, 20, 250, 120));
            GUILayout.Label("Knight position : " + transform.position);
            GUILayout.EndArea();
        }
    }
}
