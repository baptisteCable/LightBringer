﻿using UnityEngine;
using UnityEngine.AI;

namespace LightBringer
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class EnemyMotor : MonoBehaviour
    {
        public float MaxMoveSpeedPassive;
        public float MaxMoveSpeedFight;
        public float MaxMoveSpeedRage;

        public float AccelerationPassive;
        public float AccelerationFight;
        public float AccelerationRage;

        public float RotationAccelerationPassive;
        public float RotationAccelerationFight;
        public float RotationAccelerationRage;

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
        protected float rotationAcceleration;
        protected float rotationSpeed = 0f;

        // Components
        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public CharacterController cc;

        //Animation acceleration smooth
        protected Vector3 animAcceleration;
        protected Vector3 newAcceleration;
        protected Vector2 smoothDeltaPosition = Vector2.zero;

        // dead or not
        public bool isDead = false;

        protected void StartProcedure()
        {
            // Animator
            anim = transform.Find("EnemyContainer").GetComponent<Animator>();

            // Character controller
            cc = GetComponent<CharacterController>();

            // Agent
            agent = GetComponent<NavMeshAgent>();
            agent.updatePosition = false;
            overrideAgent = false;

            // Last position
            lastPosition = transform.position;
        }

        protected void UpdateProcedure()
        {
            if (!isDead)
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
                //if (Time.deltaTime > 1e-5f)
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

                /*
                LookAt lookAt = GetComponent<LookAt>();
                if (lookAt)
                    lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;
                    */
            }

        }

        public void EnableAgentRoation()
        {
            agent.angularSpeed = 0;
        }

        public void DisableAgentRotation()
        {
            agent.angularSpeed = rotationSpeed;
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

        public void SetMode(int mode)
        {
            switch (mode)
            {
                case EnemyMode.Passive:
                    SetMovementParameters(MaxMoveSpeedPassive, AccelerationPassive);
                    SetRotationParameters(MaxRotationSpeedPassive, RotationAccelerationPassive);
                    break;
                case EnemyMode.Fight:
                    SetMovementParameters(MaxMoveSpeedFight, AccelerationFight);
                    SetRotationParameters(MaxRotationSpeedFight, RotationAccelerationFight);
                    break;
                case EnemyMode.Rage:
                    SetMovementParameters(MaxMoveSpeedRage, AccelerationRage);
                    SetRotationParameters(MaxRotationSpeedRage, RotationAccelerationRage);
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
            
            // rotate if not to close to avoid shivering
            if (Mathf.Abs(angle) >= 2f || (transform.position - direction).magnitude > 5)
            {
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
        }

        public virtual void Die()
        {
            isDead = true;
            GetComponent<CharacterController>().enabled = false;
            agent.enabled = false;
            Destroy(gameObject, 10f);
            anim.Play("Die");
        }
    }
}