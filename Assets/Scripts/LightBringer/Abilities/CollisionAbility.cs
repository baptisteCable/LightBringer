using UnityEngine;

namespace LightBringer.Abilities
{
    public interface CollisionAbility
    {
        void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col);
    }
}