using LightBringer.Abilities;
using UnityEngine;

namespace LightBringer.Enemies.Knight
{

    public class Attack2Impact : MonoBehaviour
    {
        private const float FALLDOWN_TIME = 1f;

        [SerializeField] private GameObject bullet = null;
        [SerializeField] private GameObject explosion = null;
        [SerializeField] private GameObject indicator = null;
        [SerializeField] private GameObject indicatorPS = null;
        private float startingTime;
        private bool exploded = false;

        public float radius;
        public CollisionAbility ability;


        void Start ()
        {
            bullet.GetComponent<Rigidbody> ().velocity = Vector3.down * 40 / FALLDOWN_TIME;
            bullet.transform.localScale *= radius;

            indicator.transform.localScale *= radius;
            indicatorPS.transform.localScale *= radius;

            explosion.transform.localScale *= radius;

            // if on server
            if (ability != null)
            {
                explosion.transform.Find ("Effect").GetComponent<AbilityColliderTrigger> ().SetAbility (ability);
                explosion.transform.Find ("Effect").GetComponent<Collider> ().enabled = true;
            }

            startingTime = Time.time;
        }

        void Update ()
        {
            if (Time.time > startingTime + FALLDOWN_TIME && exploded == false)
            {
                exploded = true;
                Destroy (bullet);
                Destroy (indicator);
                Destroy (indicatorPS);
                explosion.SetActive (true);
                Destroy (gameObject, .5f);
            }
        }
    }

}
