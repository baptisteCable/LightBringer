using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Knight
{

    public class Attack2Impact : MonoBehaviour
    {
        private GameObject bullet;
        private GameObject explosion;
        private GameObject indicator;
        private float startingTime;
        private bool exploded = false;

        public float radius;
        public CollisionAbility ability;


        void Start()
        {
            bullet = transform.Find("LightningBullet").gameObject;
            bullet.GetComponent<Rigidbody>().velocity = Vector3.down * 40;
            bullet.transform.localScale *= radius;

            indicator = transform.Find("Indicator").gameObject;
            indicator.GetComponent<Projector>().orthographicSize *= radius;

            explosion = transform.Find("Explosion").gameObject;
            explosion.transform.localScale *= radius;
            AbilityColliderTrigger act = explosion.transform.Find("Effect").GetComponent<AbilityColliderTrigger>();
            act.ForcedStart();
            act.SetAbility(ability);

            startingTime = Time.time;
        }

        void Update()
        {
            if (Time.time > startingTime + 1f && exploded == false)
            {
                exploded = true;
                Destroy(bullet);
                Destroy(indicator);
                explosion.SetActive(true);
                Destroy(gameObject, .5f);
            }
        }
    }

}
