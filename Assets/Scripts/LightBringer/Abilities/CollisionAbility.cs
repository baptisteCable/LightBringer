using UnityEngine;

namespace LightBringer
{
    public interface CollisionAbility
    {
        void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col);
    }
}