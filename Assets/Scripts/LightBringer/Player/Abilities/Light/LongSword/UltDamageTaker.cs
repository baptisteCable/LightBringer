using UnityEngine;
using LightBringer.Enemies;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class UltDamageTaker : DamageTaker
    {
        private const float QUARTER_DAMAGE = 6f;
        private const float ALL_QUARTER_DAMAGE = 34f;
        private const float ROTATION_SPEED = 12f;

        public GameObject[] quarters;
        private int qCount = 4;

        private Transform anchor;

        private void Start()
        {
            anchor = statusManager.transform;
        }

        private void Update()
        {
            transform.position = anchor.position;
            transform.Rotate(Vector3.up, Time.deltaTime * ROTATION_SPEED);

            if (statusManager.isDead)
            {
                Destroy(gameObject);
            }
        }

        protected override Damage modifyDamage(Damage dmg, Character dealer, Vector3 origin)
        {
            int quarterId = QuarterFromDamageOrigin(origin);
            if (dmg.element == DamageElement.Light && quarters[quarterId] != null)
            {
                dmg.amount = QUARTER_DAMAGE;

                quarters[quarterId].transform.Find("Flash").gameObject.SetActive(true);

                Destroy(quarters[quarterId], .12f);
                quarters[quarterId] = null;
                qCount -= 1;

                if (qCount == 0)
                {
                    dmg.amount += ALL_QUARTER_DAMAGE;
                    Destroy(transform.parent.gameObject, .7f);

                    transform.Find("AllBrokenEffect").GetComponent<ParticleSystem>().Play();
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


