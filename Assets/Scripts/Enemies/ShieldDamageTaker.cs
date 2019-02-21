using UnityEngine;
using LightBringer.Player;
using LightBringer.Effects;

namespace LightBringer.Enemies.Knight
{
    public class ShieldDamageTaker : DamageTaker
    {
        public override void TakeDamage(Damage dmg, PlayerMotor dealer, Vector3 origin, int id)
        {
            if (dmg.type == DamageType.Melee)
            {
                statusManager.CallForAll(StatusManager.M_ShieldFlash);
            }
        }
    }
}
