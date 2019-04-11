using UnityEngine;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class UltMotor : MonoBehaviour
    {
        private const float ROTATION_SPEED = 12f;
        private const float EXTRA_DAMAGE_TAKER_DURATION = 10f;

        public GameObject[] quarters;
        public GameObject[] bigQuarters;
        public int qCount = 4;

        public Transform anchor;

        private void Start()
        {
            transform.Find("DamageTaker").gameObject.SetActive(true);
            transform.rotation = Quaternion.identity;
            Destroy(gameObject, EXTRA_DAMAGE_TAKER_DURATION);
            transform.localScale = Vector3.one * anchor.GetComponent<CharacterController>().radius;
        }

        private void Update()
        {
            if (anchor != null)
            {
                transform.position = anchor.position;
            }
            transform.Rotate(Vector3.up, Time.deltaTime * ROTATION_SPEED);
        }

        public void DestroyObject()
        {
            Destroy(gameObject, .7f);
            transform.Find("DamageTaker/AllBrokenEffect").GetComponent<ParticleSystem>().Play();
        }

        public void DestroyQuarter(int quarterId)
        {
            quarters[quarterId].transform.Find("Flash").gameObject.SetActive(true);
            Destroy(quarters[quarterId], .12f);
            quarters[quarterId] = null;
            Destroy(bigQuarters[quarterId]);
            bigQuarters[quarterId] = null;
            qCount -= 1;
        }
    }
}


