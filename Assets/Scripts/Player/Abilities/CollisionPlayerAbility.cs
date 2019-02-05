using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public abstract class CollisionPlayerAbility : Ability, CollisionAbility
    {
        public CollisionPlayerAbility(float coolDownDuration, float channelingDuration, float abilityDuration, PlayerMotor playerMotor,
            bool channelingCancellable, bool castingCancellable) :
            base(coolDownDuration, channelingDuration, abilityDuration, playerMotor, channelingCancellable, castingCancellable)
        {
        }

        public abstract void OnCollision(AbilityColliderTrigger abilityColliderTrigger, Collider col);
    }
}