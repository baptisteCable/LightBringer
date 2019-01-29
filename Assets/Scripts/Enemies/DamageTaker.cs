using UnityEngine;
using LightBringer.Player;

namespace LightBringer.Enemies
{
    public class DamageTaker : MonoBehaviour
    {
        // False if this deals extra damage that should not stop a single target attack
        public bool extraDmg = false;

        // True if some attacks can bounce on it and stun the player
        public bool bouncing = false;

        public StatusManager statusManager;

        public virtual void TakeDamage(Damage dmg, Character dealer, Vector3 origin, int id)
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
