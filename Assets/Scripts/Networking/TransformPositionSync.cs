using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public class TransformPositionSync : NetworkBehaviour
    {
        private struct PosAtTime
        {
            public float time;
            public Vector3 position;

            public PosAtTime(float time, Vector3 position)
            {
                this.time = time;
                this.position = position;
            }
        }

        [SerializeField]
        private Transform syncedTransform;

        private List<PosAtTime> incomingPositions;
        // private float averageTimeDelta = Mathf.Infinity;

        private float lastSyncTime = 0;


        private void Start()
        {
            incomingPositions = new List<PosAtTime>();
            incomingPositions.Add(new PosAtTime(Time.time, syncedTransform.position));
        }

        private void FixedUpdate()
        {
            UpdateSyncPosition();
        }

        private void Update()
        {
            InterpolatePosition();
        }

        void InterpolatePosition()
        {
            if (!isServer)
            {
                if (incomingPositions.Count <= 1)
                {
                    return;
                }

                while (Time.time > incomingPositions[1].time)
                {
                    incomingPositions.RemoveAt(0);

                    if (incomingPositions.Count <= 1)
                    {
                        return;
                    }
                }

                float alpha = (Time.time - incomingPositions[0].time) / (incomingPositions[1].time - incomingPositions[0].time);

                syncedTransform.position = Vector3.Lerp(incomingPositions[0].position, incomingPositions[1].position, alpha);
            }
        }

        [ClientRpc]
        private void RpcSynchronizePosition(Vector3 syncPosition, float time)
        {
            if (!isServer && NetworkSynchronization.singleton != null && incomingPositions != null)
            {
                float localTime = NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time);

                if (localTime > 0)
                {
                    incomingPositions.Add(new PosAtTime(localTime, syncPosition));
                }
            }
        }

        void UpdateSyncPosition()
        {
            if (isServer && NetworkSynchronization.singleton != null && Time.time > lastSyncTime + NetworkSynchronization.singleton.syncInterval - .0001f)
            {
                RpcSynchronizePosition(syncedTransform.position, Time.time);
                lastSyncTime = Time.time;
            }
        }
    }
}
