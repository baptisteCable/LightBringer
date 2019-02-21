using UnityEngine;

namespace LightBringer.Enemies
{
    public class Head : MonoBehaviour
    {
        private const float TARGET_LERP_RATE = 10f;
        private const float LOOK_AROUND_LERP_RATE = 1f;
        private const float TIME_BETWEEN_2_RAND = 2f;
        private const float LOOK_AROUND_TURN_PER_SEC = 2f;
        private const float LOOK_AROUND_X_ERROR = 10f;
        private const float LOOK_AROUND_Y_ERROR = 20f;

        [Header("Transforms")]
        [SerializeField] Transform head;
        [SerializeField] Transform sight;

        [Header("Detection")]
        [SerializeField] Detection detectionSystem;

        [Header("Rotation bounds")]
        [SerializeField] private float headYAngleBound = 80f;
        [SerializeField] private float headXAngleBound = 20f;
        [SerializeField] private float sightXAngleBound = 50f;

        private Transform target;
        private float theoYRot;
        private float theoXRot;
        private float targetHeadYRot;
        private float targetHeadXRot;
        private float targetSightXRot;

        private float nextRandomTime;

        private Quaternion lastHeadRotation, lastSightRotation;

        private Behaviour behaviour;

        private enum Behaviour
        {
            NoTarget,
            LookAtTarget,
            LookForTarget
        }

        private void Start()
        {
            NoTarget();
        }

        // Do after animations
        void LateUpdate()
        {
            ComputeTargetRotation();
            ComputeHeadAndSightRotation();
            RotateHeadAndSight();

            lastHeadRotation = head.transform.localRotation;
            lastSightRotation = sight.transform.localRotation;
        }

        public void ComputeTargetRotation()
        {
            if (behaviour == Behaviour.NoTarget)
            {
                if (Time.time > nextRandomTime)
                {
                    RandomRotation();
                }
            }
            else if (behaviour == Behaviour.LookAtTarget || behaviour == Behaviour.LookForTarget)
            {
                Vector3 targetDirection = target.transform.position - head.transform.position;
                targetDirection.y = 0;
                theoYRot = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

                float b = head.transform.position.y - target.transform.position.y - 1.8f;
                float c = Vector3.Distance(head.transform.position, target.transform.position + 1.8f * Vector3.up);
                theoXRot = 180 / 3.141592654f * Mathf.Asin(b / c);
            }
        }

        private void ComputeHeadAndSightRotation()
        {
            if (theoYRot < -headYAngleBound)
            {
                targetHeadYRot = -headYAngleBound;
            }
            else if (theoYRot > headYAngleBound)
            {
                targetHeadYRot = headYAngleBound;
            }
            else
            {
                targetHeadYRot = theoYRot;
            }

            if (theoXRot / 2 > headXAngleBound)
            {
                targetHeadXRot = headXAngleBound;
            }
            else
            {
                targetHeadXRot = theoXRot / 2;
            }

            if (theoXRot - targetHeadXRot > sightXAngleBound)
            {
                targetSightXRot = sightXAngleBound;
            }
            else
            {
                targetSightXRot = theoXRot - targetHeadXRot;
            }
        }

        private void RotateHeadAndSight()
        {
            float lerpRate = LOOK_AROUND_LERP_RATE;
            if (behaviour == Behaviour.LookAtTarget)
            {
                lerpRate = TARGET_LERP_RATE;
            }
            else if (behaviour == Behaviour.LookForTarget)
            {
                lerpRate = TARGET_LERP_RATE;
            }

            head.localRotation = Quaternion.Lerp(lastHeadRotation, HeadTargetRotation(), lerpRate * Time.deltaTime);
            sight.localRotation = Quaternion.Lerp(lastSightRotation, SightTargetRotation(), lerpRate * Time.deltaTime);
        }

        private Quaternion HeadTargetRotation()
        {
            return Quaternion.Euler(-targetHeadYRot, 0, targetHeadXRot);
        }

        private Quaternion SightTargetRotation()
        {
            return Quaternion.Euler(0, 0, targetSightXRot);
        }

        private void RandomRotation()
        {
            nextRandomTime = Time.time + Random.value * 3f + 1f;
            theoXRot = Mathf.Pow(Random.value, 3) * (headXAngleBound + sightXAngleBound);
            theoYRot = Random.value * headYAngleBound * 2 - headYAngleBound;
        }

        public void LookAtTarget(GameObject tar)
        {
            behaviour = Behaviour.LookAtTarget;
            target = tar.transform;
            detectionSystem.Stop();
        }

        public void LookForTarget(GameObject tar, float duration)
        {
            behaviour = Behaviour.LookForTarget;
            target = tar.transform;
            detectionSystem.Play();
        }

        public void NoTarget()
        {
            behaviour = Behaviour.NoTarget;
            RandomRotation();
            detectionSystem.Stop();
        }
    }

}