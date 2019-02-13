using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Networking
{
    public abstract class DelayedNetworkBehaviour : NetworkBehaviour
    {
        public void CallByName(string methodName)
        {
            if (isServer)
            {
                RpcCallByName(methodName, Time.time);

                MethodInfo mi = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(this, null);
            }
        }

        [ClientRpc]
        private void RpcCallByName(string methodName, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallByNameWithDelay(methodName, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallByNameWithDelay(string methodName, float delay)
        {
            yield return new WaitForSeconds(delay);

            MethodInfo mi = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(this, null);
        }
    }
}
