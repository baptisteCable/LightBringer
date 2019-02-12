using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public class NetworkSynchronization : NetworkBehaviour
    {
        public const int MESSAGE_PING = 41;
        public const int MESSAGE_PONG = 42;

        [HideInInspector] public static NetworkSynchronization singleton;

        private const float LERP_RATE = .07f;

        // ServerTime - LocalTime
        [HideInInspector] public float serverLocalTimeDiff;

        [HideInInspector] public float networkPing;

        // Time between control and backCommand, including syncInterval
        // reduce it implies lag, but allows network latency
        // hard reduce, smooth increase
        [HideInInspector] public float simulatedPing;

        // Movement sync interval
        public float syncInterval = .06f;
        public float safetyInterval = .01f;

        private float lastSyncTime = 0;
        private float timeSyncInterval = .06f;
        private float pingInterval = .5f;

        private float lastPingTime;

        public NetworkClient client;

        private class FloatMessage : MessageBase
        {
            public float value;
        }

        void Start()
        {
            // start high. It will be hard reduced.
            serverLocalTimeDiff = Mathf.Infinity;

            singleton = this;

            // Handler on server: register on NetworkServer
            if (isServer)
            {
                NetworkServer.RegisterHandler(MESSAGE_PING, OnServerPing);
            }

            // Handler on the client: register on client
            if (!isServer)
            {
                NetworkServer.RegisterHandler(MESSAGE_PONG, OnClientPong);
            }

            client = NetworkManager.singleton.client;
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
            if (!isServer && Time.time > lastPingTime + pingInterval)
            {
                lastPingTime = Time.time;

                // send message to the server
                FloatMessage message = new FloatMessage();
                message.value = Time.time;
                client.Send(MESSAGE_PING, message);
            }
        }

        private void OnServerPing(NetworkMessage netMsg)
        {
            FloatMessage message = netMsg.ReadMessage<FloatMessage>();
            NetworkServer.SendToClient(netMsg.conn.connectionId, MESSAGE_PONG, message);
        }

        private void OnClientPong(NetworkMessage netMsg)
        {
            FloatMessage message = netMsg.ReadMessage<FloatMessage>();
            networkPing = (int)((Time.time - message.value) * 1000);
            simulatedPing = networkPing + (int)((syncInterval + safetyInterval) * 1000);
        }

        private void OnGUI()
        {
            if (!isServer)
            {
                GUI.contentColor = Color.black;
                GUILayout.BeginArea(new Rect(500, 20, 250, 120));
                GUILayout.Label("TimeDiff: " + serverLocalTimeDiff);
                GUILayout.Label("Ping: " + networkPing);
                GUILayout.Label("Simulated ping: " + simulatedPing);
                GUILayout.EndArea();
            }
        }

        public float GetLocalTimeFromServerTime(float serverTime)
        {
            return serverTime - serverLocalTimeDiff + syncInterval + safetyInterval;
        }
    }
}
