using UnityEngine;
using LightBringer.Enemies;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    [RequireComponent(typeof(UltMotor))]
    public class UltDamageTaker : DamageTaker
    {
        private const float QUARTER_DAMAGE = 6f;
        private const float ALL_QUARTER_DAMAGE = 34f;

        private UltMotor um;

        private void Start()
        {
            um = GetComponent<UltMotor>();
        }

        private void Update()
        {
            if (statusManager.isDead)
            {
                Destroy(gameObject);
            }
        }

        protected override Damage modifyDamage(Damage dmg, PlayerMotor dealer, Vector3 origin)
        {
            int quarterId = QuarterFromDamageOrigin(origin);
            if (dmg.element == DamageElement.Light && um.quarters[quarterId] != null)
            {
                dmg.amount = QUARTER_DAMAGE;

                um.DestroyQuarter(quarterId);
                
                if (um.qCount == 0)
                {
                    dmg.amount += ALL_QUARTER_DAMAGE;
                    um.DestroyObject();
                }
            }
            else
            {
                dmg.amount = 0;
            }

            return dmg;
        }

        private int QuarterFromDamageOrigin(Vector3 origin)
        {
            float angle = Vector3.SignedAngle(transform.forward, origin - transform.position, Vector3.up) + 180;
            int quarter = (int)(angle / 90f);
            if (quarter == 4)
            {
                quarter = 3;
            }
            return quarter;
        }
    }
}


