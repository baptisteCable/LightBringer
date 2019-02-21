using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public abstract class DelayedNetworkBehaviour2 : NetworkBehaviour
    {
        // ---------- void (000) ----------------
        protected virtual bool CallById(int methdodId)
        {
            return false;
        }

        public void CallForAll(int methodId)
        {
            if (isServer)
            {
                RpcCallForAll2(methodId, Time.time);
                CallById(methodId);
            }
        }

        [ClientRpc]
        private void RpcCallForAll2(int methodId, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallForAllWithDelay(methodId, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallForAllWithDelay(int methodId, float delay)
        {
            yield return new WaitForSeconds(delay);
            CallById(methodId);
        }

        // ---------- Vector3 (100) ----------------
        protected virtual bool CallById(int methdodId, Vector3 vec)
        {
            return false;
        }

        public void CallForAll(int methodid, Vector3 vec)
        {
            if (isServer)
            {
                RpcCallForAllVector32(methodid, vec, Time.time);
                CallById(methodid, vec);
            }
        }

        [ClientRpc]
        private void RpcCallForAllVector32(int methodId, Vector3 vec, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallForAllWithDelay(methodId, vec, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallForAllWithDelay(int methodId, Vector3 vec, float delay)
        {
            yield return new WaitForSeconds(delay);
            CallById(methodId, vec);
        }

        // ---------- float (200) ----------------
        protected virtual bool CallById(int methdodId, float value)
        {
            return false;
        }

        public void CallForAll(int methodid, float value)
        {
            if (isServer)
            {
                RpcCallForAllFloat2(methodid, value, Time.time);
                CallById(methodid, value);
            }
        }

        [ClientRpc]
        private void RpcCallForAllFloat2(int methodId, float value, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallForAllWithDelay(methodId, value, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallForAllWithDelay(int methodId, float value, float delay)
        {
            yield return new WaitForSeconds(delay);
            CallById(methodId, value);
        }

        // ---------- int (300) ----------------
        protected virtual bool CallById(int methdodId, int i)
        {
            return false;
        }

        public void CallForAll(int methodid, int i)
        {
            if (isServer)
            {
                RpcCallForAllInt2(methodid, i, Time.time);
                CallById(methodid, i);
            }
        }

        [ClientRpc]
        private void RpcCallForAllInt2(int methodId, int i, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallForAllWithDelay(methodId, i, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallForAllWithDelay(int methodId, int i, float delay)
        {
            yield return new WaitForSeconds(delay);
            CallById(methodId, i);
        }

        // ---------- int + float (400) ----------------
        protected virtual bool CallById(int methdodId, int i, float f)
        {
            return false;
        }

        public void CallForAll(int methodid, int i, float f)
        {
            if (isServer)
            {
                RpcCallForAllIntFloat2(methodid, i, f, Time.time);
                CallById(methodid, i, f);
            }
        }

        [ClientRpc]
        private void RpcCallForAllIntFloat2(int methodId, int i, float f, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallForAllWithDelay(methodId, i, f, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallForAllWithDelay(int methodId, int i, float f, float delay)
        {
            yield return new WaitForSeconds(delay);
            CallById(methodId, i, f);
        }

        // ---------- int + bool (500) ----------------
        protected virtual bool CallById(int methdodId, int i, bool b)
        {
            return false;
        }

        public void CallForAll(int methodid, int i, bool b)
        {
            if (isServer)
            {
                RpcCallForAllIntBool2(methodid, i, b, Time.time);
                CallById(methodid, i, b);
            }
        }

        [ClientRpc]
        private void RpcCallForAllIntBool2(int methodId, int i, bool b, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallForAllWithDelay(methodId, i, b, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallForAllWithDelay(int methodId, int i, bool b, float delay)
        {
            yield return new WaitForSeconds(delay);
            CallById(methodId, i, b);
        }
    }
}
