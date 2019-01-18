using UnityEngine;
using LightBringer.Player;

namespace LightBringer.Enemies
{
    public class DamageTaker : MonoBehaviour
    {
        // False if this deals extra damage that should not stop a single target attack
        public bool extraDmg;

        public StatusManager statusManager;

        public void TakeDamage(Damage dmg, Character dealer, Vector3 origin, int id)
        {
            dmg = modifyDamage(dmg, dealer, origin);

            if (extraDmg)
            {
                id = Random.Range(int.MinValue, int.MaxValue);
            }

            statusManager.TakeDamage(dmg, dealer, id, (transform.position - origin).magnitude);
        }

        protected virtual Damage modifyDamage(Damage dmg, Character dealer, Vector3 origin)
        {
            return dmg;
        }
    }
}
