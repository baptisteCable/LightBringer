using UnityEngine;

namespace LightBringer.Player
{
    public class Immaterial : State
    {
        private const int IMMATERIAL_LAYER = 12;
        private const int PLAYER_LAYER = 10;

        public Immaterial(float duration) : base(duration) { }

        public override void Start(PlayerStatusManager psm)
        {
            base.Start(psm);

            recSetLayer(psm.gameObject, PLAYER_LAYER, IMMATERIAL_LAYER);

            Debug.Log("Début immaterial");
        }

        public override void Stop()
        {
            base.Stop();

            recSetLayer(psm.gameObject, IMMATERIAL_LAYER, PLAYER_LAYER);

            Debug.Log("Fin immaterial");
        }

        public override bool IsAffectedBy(Damage dmg, EnemyMotor dealer, Vector3 origin)
        {
            return false;
        }

        public override Damage AlterTakenDamage(Damage dmg, EnemyMotor dealer, Vector3 origin)
        {
            dmg.amount = 0;
            return dmg;
        }

        public override Damage AlterDealtDamage(Damage dmg)
        {
            dmg.amount /= 2f;
            return dmg;
        }

        private void recSetLayer(GameObject go, int from, int to)
        {
            if (go.layer == from)
            {
                go.layer = to;
            }

            foreach (Transform child in go.transform)
            {
                recSetLayer(child.gameObject, from, to);
            }
        }
    }
}

