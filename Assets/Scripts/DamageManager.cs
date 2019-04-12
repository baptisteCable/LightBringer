using UnityEngine;

namespace LightBringer
{
    public class DamageManager : MonoBehaviour
    {

        public static DamageManager dm;

        public Material EnergyMaterial;
        public Material FireMaterial;
        public Material IceMaterial;
        public Material LightMaterial;
        public Material PureMaterial;
        public Material PhysicalMaterial;

        void Start()
        {
            dm = this;
        }

        public Material ElementMaterial(DamageElement element)
        {
            switch (element)
            {
                case DamageElement.Energy:
                    return EnergyMaterial;
                case DamageElement.Fire:
                    return FireMaterial;
                case DamageElement.Ice:
                    return IceMaterial;
                case DamageElement.Light:
                    return LightMaterial;
                case DamageElement.Pure:
                    return PureMaterial;
                default:
                    return PhysicalMaterial;
            }
        }
    }
}


