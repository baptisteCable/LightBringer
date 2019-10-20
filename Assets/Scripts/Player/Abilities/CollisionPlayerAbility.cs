using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Player.Abilities
{
    public abstract class CollisionPlayerAbility : Ability, CollisionAbility
    {
        public CollisionPlayerAbility (float coolDownDuration, float channelingDuration, float abilityDuration, PlayerMotor playerMotor,
            bool channelingCancellable, bool castingCancellable, bool parallelizable, int id) :
            base (coolDownDuration, channelingDuration, abilityDuration, playerMotor, channelingCancellable, castingCancellable, parallelizable, id)
        {
        }

        public abstract void OnColliderEnter (AbilityColliderTrigger abilityColliderTrigger, Collider col);

        public virtual void OnColliderStay (AbilityColliderTrigger abilityColliderTrigger, Collider col)
        {
        }
    }
}