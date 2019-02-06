using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public class NetworkSynchronization : NetworkBehaviour
    {
        private const float LERP_RATE = .07f;

        // ServerTime - LocalTime
        public float serverLocalTimeDiff;

        public float networkPing;

        // Time between control and backCommand, including syncInterval
        // reduce it implies lag, but allows network latency
        // hard reduce, smooth increase
        public float simulatedPing;

        // Movement sync interval
        public float syncInterval = .06f;
        public float safetyInterval = .01f;

        private float lastSyncTime = 0;
        private float timeSyncInterval = .06f;
        private float pingInterval = .5f;

        private float lastPingTime;

        void Start()
        {
            // start high. It will be reduced.
            serverLocalTimeDiff = Mathf.Infinity;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            SyncTime();
            Ping();

        }

        [ClientRpc]
        private void RpcSynchronizeTime(float serverTime)
        {
            if (!isServer)
            {
                float delta = serverTime - Time.time;

                if (delta < serverLocalTimeDiff)
                {
                    serverLocalTimeDiff = delta;
                }
                else
                {
                    serverLocalTimeDiff -= LERP_RATE * (serverLocalTimeDiff - delta);
                }
            }
        }

        void SyncTime()
        {
            if (isServer && Time.time > lastSyncTime + timeSyncInterval - .0001)
            {
                RpcSynchronizeTime(Time.time);
                lastSyncTime = Time.time;
            }
        }

        private void Ping()
        {
            if (!isServer && isLocalPlayer && Time.time > lastPingTime + pingInterval)
            {
                lastPingTime = Time.time;
                CmdPing();
            }
        }

        [Command]
        private void CmdPing()
        {
            RpcPong();
        }

        [ClientRpc]
        private void RpcPong()
        {
            if (isLocalPlayer)
            {
                networkPing = (int)((Time.time - lastPingTime) * 1000);
                simulatedPing = networkPing + (int)((syncInterval + safetyInterval) * 1000);
            }
        }

        private void OnGUI()
        {
            if (!isServer && isLocalPlayer)
            {
                GUI.contentColor = Color.black;
                GUILayout.BeginArea(new Rect(500, 20, 250, 120));
                GUILayout.Label("TimeDiff: " + serverLocalTimeDiff);
                GUILayout.Label("Ping: " + networkPing);
                GUILayout.Label("Simulated ping: " + simulatedPing);
                GUILayout.EndArea();
            }
        }
    }
}
