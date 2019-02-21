using System.Collections.Generic;
using LightBringer.Networking;
using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class Motor : DelayedNetworkBehaviour
    {
        public float MaxMoveSpeedPassive;
        public float MaxMoveSpeedFight;
        public float MaxMoveSpeedRage;

        public float AccelerationPassive;
        public float AccelerationFight;
        public float AccelerationRage;

        public float MaxRotationSpeedPassive;
        public float MaxRotationSpeedFight;
        public float MaxRotationSpeedRage;

        [HideInInspector]
        public NavMeshAgent agent;

        // Movement
        protected float moveSpeed;
        protected float acceleration;
        protected Vector3 lastPosition;
        protected bool overrideAgent;

        // Rotation
        protected float currentRotationSpeed;
        protected float maxRotationSpeed = 0f;
        private struct TimePoint
        {
            public float time;
            public Vector3 point;

            public TimePoint(float time, Vector3 point)
            {
                this.time = time;
                this.point = point;
            }
        }
        private List<TimePoint> delayedRotations;

        // Components
        public Animator anim;
        [HideInInspector] public CharacterController cc;
        [HideInInspector] public StatusManager statusManager;
        [HideInInspector] public Controller controller;
        public Head head;

        //Animation acceleration smooth
        protected Vector3 animAcceleration;
        protected Vector3 newAcceleration;
        protected Vector2 smoothDeltaPosition = Vector2.zero;

        // Indicators
        [Header("Indicators")]
        [SerializeField] private GameObject[] indicators;

        protected void BaseStart()
        {
            if (isServer)
            {
                controller = GetComponent<Controller>();
                controller.enabled = true;
                
                // Agent
                agent = GetComponent<NavMeshAgent>();
                agent.enabled = true;
                agent.updatePosition = false;
                overrideAgent = false;

                // Last position
                lastPosition = transform.position;

                // Rotations
                delayedRotations = new List<TimePoint>();

                // Character controller
                cc = GetComponent<CharacterController>();
                cc.enabled = true;
            }

            statusManager = GetComponent<StatusManager>();
        }

        protected void BaseUpdate()
        {
            if (!statusManager.isDead && isServer)
            {
                Vector3 worldDeltaPosition = transform.position - lastPosition;
                lastPosition = transform.position;

                // Map 'worldDeltaPosition' to local space
                float dx = Vector3.Dot(transform.right, worldDeltaPosition);
                float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
                Vector2 deltaPosition = new Vector2(dx, dy);

                // Low-pass filter the deltaMove
                float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
                smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

                // Update velocity if delta time is safe
                Vector2 velocity = smoothDeltaPosition / Time.deltaTime;

                bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

                // Update animation parameters
                anim.SetBool("isMoving", shouldMove);
                anim.SetFloat("VelX", velocity.x);
                anim.SetFloat("VelY", velocity.y);

                if (agent.velocity.magnitude > 0 && !overrideAgent)
                {
                    Move(agent.velocity);
                }

                ApplyDelayedRotations();
            }

        }

        public void EnableAgentRotation()
        {
            agent.angularSpeed = maxRotationSpeed;
        }

        public void DisableAgentRotation()
        {
            agent.angularSpeed = 0;
        }

        public void SetOverrideAgent(bool oa)
        {
            if (oa)
            {
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
                overrideAgent = true;
            }
            else
            {
                agent.nextPosition = transform.position;
                agent.SetDestination(transform.position);
                overrideAgent = false;
            }
        }
        public void MoveInDirection(Vector3 direction)
        {
            Move(direction.normalized * moveSpeed);
        }

        public void Move(Vector3 velocity)
        {
            velocity.y = velocity.y - GameManager.GRAVITY;
            cc.Move(velocity * Time.deltaTime);
            if (overrideAgent)
            {
                agent.nextPosition = transform.position + velocity * Time.deltaTime;
            }
        }

        public void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Passive:
                    SetMovementParameters(MaxMoveSpeedPassive, AccelerationPassive);
                    SetRotationParameters(MaxRotationSpeedPassive);
                    break;
                case Mode.Fight:
                    SetMovementParameters(MaxMoveSpeedFight, AccelerationFight);
                    SetRotationParameters(MaxRotationSpeedFight);
                    break;
                case Mode.Rage:
                    SetMovementParameters(MaxMoveSpeedRage, AccelerationRage);
                    SetRotationParameters(MaxRotationSpeedRage);
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

        private void SetRotationParameters(float rs)
        {
            maxRotationSpeed = rs;
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

        private void ApplyDelayedRotations()
        {
            Vector3 target = Vector3.zero;
            bool rotate = false;

            while (delayedRotations.Count > 0 && Time.time >= delayedRotations[0].time)
            {
                target = delayedRotations[0].point;
                rotate = true;
                delayedRotations.RemoveAt(0);
            }

            if (rotate)
            {
                RotateTowards(target);
            }
        }

        public void DelayedRotateTowards(Vector3 point, float delay)
        {
            TimePoint timeRot = new TimePoint(Time.time + delay, point);
            delayedRotations.Add(timeRot);
        }

        public void RotateTowards(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
            float rotationSpeed;

            rotationSpeed = Mathf.Min(
                        Mathf.Abs(angle) / 10 / Time.deltaTime,
                        maxRotationSpeed
                    );
            currentRotationSpeed = Mathf.Sign(angle) * rotationSpeed;

            if (Mathf.Abs(currentRotationSpeed * Time.deltaTime) < .1f)
            {
                currentRotationSpeed = 0;
            }

            transform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
        }

        public virtual void Die()
        {
            disableColliders(transform);
            agent.enabled = false;
            Destroy(gameObject, 10f);
            anim.Play("Die");
        }

        private void disableColliders(Transform t)
        {
            Component[] colliders = t.GetComponents(typeof(Collider));

            foreach (Collider coll in colliders)
            {
                coll.enabled = false;
            }

            foreach (Transform child in t)
            {
                disableColliders(child);
            }
        }

        protected override bool CallById(int methdodId, int i)
        {
            if (base.CallById(methdodId, i))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_HideIndicator: HideIndicator(i); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_HideIndicator = 1300;
        private void HideIndicator(int id)
        {
            indicators[id].SetActive(false);
        }

        protected override bool CallById(int methdodId, int i, float f)
        {
            if (base.CallById(methdodId, i, f))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_DisplayIndicator: DisplayIndicator(i, f); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_DisplayIndicator = 1400;
        private void DisplayIndicator(int id, float loadingTime)
        {
            indicators[id].SetActive(true);
            indicators[id].GetComponent<IndicatorLoader>().Load(loadingTime);
        }
    }
}
