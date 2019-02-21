using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Enemies.Knight
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
            indicator.transform.localScale *= radius;

            explosion = transform.Find("Explosion").gameObject;
            explosion.transform.localScale *= radius;

            // if on server
            if(ability != null)
            {
                explosion.transform.Find("Effect").GetComponent<AbilityColliderTrigger>().SetAbility(ability);
                explosion.transform.Find("Effect").GetComponent<Collider>().enabled = true;
            }

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
