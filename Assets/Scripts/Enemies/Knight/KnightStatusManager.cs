using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    class KnightStatusManager : StatusManager
    {
        [SerializeField] private float[] interruptionThresholds = null;
        [SerializeField] private float interruptionActivationThreshold = 0;

        protected override float[] InterruptionThresholds
        {
            get { return interruptionThresholds; }
        }
        protected override float InterruptionActivationThreshold
        {
            get { return interruptionActivationThreshold; }
        }
    }
}
