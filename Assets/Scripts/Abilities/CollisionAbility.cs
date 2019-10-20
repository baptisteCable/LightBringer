using UnityEngine;

namespace LightBringer.Abilities
{
    public interface CollisionAbility
    {
        void OnColliderEnter (AbilityColliderTrigger abilityColliderTrigger, Collider col);

        void OnColliderStay (AbilityColliderTrigger abilityColliderTrigger, Collider col);
    }
}