﻿using UnityEngine;

public class Head : MonoBehaviour
{
    private const float TARGET_LERP_RATE = 10f;
    private const float LOOK_AROUND_LERP_RATE = 1f;
    private const float MAX_HEAD_Y_ANGLE = 80f;
    private const float MAX_HEAD_X_ANGLE = 20f;
    private const float MAX_SIGHT_X_ANGLE = 50f;
    private const float TIME_BETWEEN_2_RAND = 2f;
    private const float LOOK_AROUND_TURN_PER_SEC = 2f;
    private const float LOOK_AROUND_X_ERROR = 10f;
    private const float LOOK_AROUND_Y_ERROR = 20f;

    [SerializeField] Transform head;
    [SerializeField] Transform sight;
    public Transform target;
    private float theoYRot;
    private float theoXRot;
    private float targetHeadYRot;
    private float targetHeadXRot;
    private float targetSightXRot;

    public float lookAroundError;

    private float nextRandomTime;

    private Quaternion lastHeadRotation, lastSightRotation;

    private Behaviour behaviour;

    private enum Behaviour
    {
        NoTarget,
        LookAtTarget,
        LookAroundTarget
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
        else if (behaviour == Behaviour.LookAtTarget)
        {
            Vector3 targetDirection = target.transform.position - head.transform.position;
            targetDirection.y = 0;
            theoYRot = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

            float b = head.transform.position.y - target.transform.position.y - 1.8f;
            float c = Vector3.Distance(head.transform.position, target.transform.position + 1.8f * Vector3.up);
            theoXRot = 180 / 3.141592654f * Mathf.Asin(b / c);
        }
        else if (behaviour == Behaviour.LookAroundTarget)
        {
            Vector3 targetDirection = target.transform.position - head.transform.position;
            targetDirection.y = 0;
            theoYRot = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up)
                + lookAroundError * Mathf.Sin(Time.time * 3.14159f * LOOK_AROUND_TURN_PER_SEC) * LOOK_AROUND_Y_ERROR;

            float b = head.transform.position.y - target.transform.position.y - 1.8f;
            float c = Vector3.Distance(head.transform.position, target.transform.position + 1.8f * Vector3.up);
            theoXRot = 180 / 3.141592654f * Mathf.Asin(b / c) 
                + lookAroundError * Mathf.Cos(Time.time * 3.14159f * LOOK_AROUND_TURN_PER_SEC) * LOOK_AROUND_X_ERROR;
        }
    }

    private void ComputeHeadAndSightRotation()
    {
        if (theoYRot < -MAX_HEAD_Y_ANGLE)
        {
            targetHeadYRot = -MAX_HEAD_Y_ANGLE;
        }
        else if (theoYRot > MAX_HEAD_Y_ANGLE)
        {
            targetHeadYRot = MAX_HEAD_Y_ANGLE;
        }
        else
        {
            targetHeadYRot = theoYRot;
        }

        if (theoXRot / 2 > MAX_HEAD_X_ANGLE)
        {
            targetHeadXRot = MAX_HEAD_X_ANGLE;
        }
        else
        {
            targetHeadXRot = theoXRot / 2;
        }

        if (theoXRot - targetHeadXRot > MAX_SIGHT_X_ANGLE)
        {
            targetSightXRot = MAX_SIGHT_X_ANGLE;
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
        else if (behaviour == Behaviour.LookAroundTarget)
        {
            lerpRate = TARGET_LERP_RATE;
        }

        head.localRotation = Quaternion.Lerp(lastHeadRotation, HeadTargetRotation(1), lerpRate * Time.deltaTime);
        sight.localRotation = Quaternion.Lerp(lastSightRotation, SightTargetRotation(1), lerpRate * Time.deltaTime);
    }

    private Quaternion HeadTargetRotation(int mode)
    {
        if (mode == 1)
        {
            return Quaternion.Euler(-targetHeadYRot, 0, targetHeadXRot);
        }
        else
        {
            return Quaternion.Euler(targetHeadXRot, targetHeadYRot, 0);
        }
    }

    private Quaternion SightTargetRotation(int mode)
    {
        if (mode == 1)
        {
            return Quaternion.Euler(0, 0, targetSightXRot);
        }
        else
        {
            return Quaternion.Euler(targetSightXRot, 0, 0);
        }
    }

    private void RandomRotation()
    {
        nextRandomTime = Time.time + Random.value * 3f + 1f;
        theoXRot = Mathf.Pow(Random.value, 3) * (MAX_HEAD_X_ANGLE + MAX_SIGHT_X_ANGLE);
        theoYRot = Random.value * MAX_HEAD_Y_ANGLE * 2 - MAX_HEAD_Y_ANGLE;

    }

    public void LookAtTarget(Transform tar)
    {
        behaviour = Behaviour.LookAtTarget;
        target = tar;
    }

    public void LookAroundTarget(Transform tar, float error)
    {
        behaviour = Behaviour.LookAroundTarget;
        target = tar;
        lookAroundError = error;
    }

    public void NoTarget()
    {
        behaviour = Behaviour.NoTarget;
        RandomRotation();
    }


    private void OnGUI()
    {
        GUI.contentColor = Color.black;
        GUILayout.BeginArea(new Rect(400, 20, 250, 120));
        GUILayout.Label("targetYRot: " + targetHeadYRot);
        GUILayout.Label("targetXRot: " + targetHeadXRot);
        GUILayout.EndArea();
    }
}
