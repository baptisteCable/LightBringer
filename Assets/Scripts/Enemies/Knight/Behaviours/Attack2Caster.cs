using LightBringer.Abilities;
using LightBringer.Networking;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    public class Attack2Caster : DelayedNetworkBehaviour
    {
        private const float MIN_TIME = .2f;
        private const float MAX_TIME = 2f;

        public float nextShotTime = 0f;
        public int remainingShots;
        public float range;
        public float radius;

        public GameObject ImpactPrefab;

        public CollisionAbility ability;

        void Update()
        {
            if (isServer)
            {
                if (Time.time >= nextShotTime)
                {
                    nextShotTime = Time.time + Random.value * (MAX_TIME - MIN_TIME) + MIN_TIME;
                    CreateImpactZone();
                }

                if (remainingShots == 0)
                {
                    Destroy(gameObject, 2f);
                }
            }
        }

        private void CreateImpactZone()
        {
            remainingShots -= 1;
            Vector3 relativePosition = Quaternion.AngleAxis(Random.value * 360, Vector3.up) * Vector3.forward * Random.value * range;
            CallForAll(M_CreateImpactZoneAtPosition, transform.position + relativePosition, radius);
        }

        protected override bool CallById(int methdodId, Vector3 pos, float f)
        {
            if (base.CallById(methdodId, pos))
            {
                return true;
            }
            switch (methdodId)
            {
                case M_CreateImpactZoneAtPosition: CreateImpactZoneAtPosition(pos, f); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_CreateImpactZoneAtPosition = 600;
        private void CreateImpactZoneAtPosition(Vector3 pos, float rad)
        {
            GameObject impact = Instantiate(ImpactPrefab, pos, Quaternion.identity);
            Attack2Impact a2i = impact.GetComponent<Attack2Impact>();
            a2i.radius = rad;
            a2i.ability = ability;
        }
    }
}
