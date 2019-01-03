using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Knight
{
    public class Attack2Caster : MonoBehaviour
    {
        private const float MIN_TIME = .2f;
        private const float MAX_TIME = 2f;

        public float timeBeforeNext;
        public int remainingShots;
        public float range;
        public float radius;

        public GameObject ImpactPrefab;

        public CollisionAbility ability;

        void Update()
        {
            timeBeforeNext -= Time.deltaTime;

            if (timeBeforeNext <= 0)
            {
                timeBeforeNext = Random.value * (MAX_TIME - MIN_TIME) + MIN_TIME;
                CreateImpactZone();
            }

            if (remainingShots == 0)
            {
                Destroy(gameObject);
            }
        }

        private void CreateImpactZone()
        {
            remainingShots -= 1;
            Vector3 relativePosition = Quaternion.AngleAxis(Random.value * 360, Vector3.up) * Vector3.forward * Random.value * range;
            GameObject impact = Instantiate(ImpactPrefab, transform.position + relativePosition, Quaternion.identity);
            Attack2Impact a2i = impact.GetComponent<Attack2Impact>();
            a2i.radius = radius;
            a2i.ability = ability;
        }
    }

}
