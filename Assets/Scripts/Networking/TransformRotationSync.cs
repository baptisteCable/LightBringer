using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public class TransformRotationSync : NetworkBehaviour
    {
        private struct RotAtTime
        {
            public float time;
            public float rotation;

            public RotAtTime(float time, float rotation)
            {
                this.time = time;
                this.rotation = rotation;
            }
        }

        [SerializeField]
        private Transform syncedTransform;

        private List<RotAtTime> incomingRotations;
        // private float averageTimeDelta = Mathf.Infinity;

        private float lastSyncTime = 0;


        private void Start()
        {
            incomingRotations = new List<RotAtTime>();
            float rotation = syncedTransform.rotation.eulerAngles.y;
            incomingRotations.Add(new RotAtTime(Time.time, rotation));
        }

        private void FixedUpdate()
        {
            UpdateSyncRotation();
        }

        private void Update()
        {
            InterpolateRotation();
        }

        void InterpolateRotation()
        {
            if (!isServer)
            {
                if (incomingRotations.Count <= 1)
                {
                    return;
                }

                while (Time.time > incomingRotations[1].time)
                {
                    incomingRotations.RemoveAt(0);

                    if (incomingRotations.Count <= 1)
                    {
                        return;
                    }

                    float rotDelta = incomingRotations[1].rotation - incomingRotations[0].rotation;

                    if (rotDelta > 180)
                    {
                        RotAtTime rotAtTime = incomingRotations[0];
                        rotAtTime.rotation += 360;
                        incomingRotations[0] = rotAtTime;
                    }
                    else if (rotDelta < -180)
                    {
                        RotAtTime rotAtTime = incomingRotations[0];
                        rotAtTime.rotation -= 360;
                        incomingRotations[0] = rotAtTime;
                    }
                }

                float alpha = (Time.time - incomingRotations[0].time) / (incomingRotations[1].time - incomingRotations[0].time);

                syncedTransform.rotation = Quaternion.Euler(0, Mathf.Lerp(incomingRotations[0].rotation, incomingRotations[1].rotation, alpha), 0);
            }
        }

        void UpdateSyncRotation()
        {
            if (isServer && NetworkSynchronization.singleton != null && Time.time > lastSyncTime + NetworkSynchronization.singleton.syncInterval - .0001f)
            {
                RpcSynchronizeRotation(syncedTransform.rotation.eulerAngles.y, Time.time);
                lastSyncTime = Time.time;
            }
        }

        [ClientRpc]
        private void RpcSynchronizeRotation(float syncRotation, float time)
        {
            if (!isServer && NetworkSynchronization.singleton != null && incomingRotations != null)
            {
                float localTime = NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time);

                if (localTime > 0)
                {
                    incomingRotations.Add(new RotAtTime(localTime, syncRotation));
                }
            }
        }
    }
}
