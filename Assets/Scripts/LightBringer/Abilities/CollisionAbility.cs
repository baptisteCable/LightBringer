using UnityEngine;
using System.Collections.Generic;

namespace LightBringer
{
    public abstract class CollisionAbility : Ability
    {
        public CollisionAbility(float coolDownDuration, float channelingDuration, float abilityDuration, Character character, bool channelingCancellable) :
            base(coolDownDuration, channelingDuration, abilityDuration, character, channelingCancellable)
        {
        }

        public abstract void OnCollision(Collider col);
    }
}