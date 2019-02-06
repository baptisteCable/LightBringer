using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public class TransformSync : NetworkBehaviour
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

        [SerializeField]
        private NetworkSynchronization ns;

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
            if (!isServer && incomingPositions != null)
            {
                /*
                float delta = time - Time.time;

                if (delta < averageTimeDelta)
                {
                    averageTimeDelta = delta;
                }
                */
                // (time - averageTimeDelta) is the local time corresponding to the server time, including network latency.
                // The local time for this position is (time - averageTimeDelta) + syncInterval
                // We add a little safe time to avoid waiting for next positions
                float localTime = time - ns.serverLocalTimeDiff + ns.syncInterval + ns.safetyInterval;

                incomingPositions.Add(new PosAtTime(localTime, syncPosition));
            }
        }

        void UpdateSyncPosition()
        {
            if (isServer && Time.time > lastSyncTime + ns.syncInterval - .0001f)
            {
                RpcSynchronizePosition(syncedTransform.position, Time.time);
                lastSyncTime = Time.time;
            }
        }
    }
}
