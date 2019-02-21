using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightMotor : Motor
    {
        [Header("Prefabs")]
        public GameObject bulletPrefab;
        public GameObject casterPrefab;

        [Header("Game Objects")]
        // Colliders GO
        public GameObject attack1act1GO;
        public GameObject attack1act2GO;
        public GameObject attack1act3GO;
        public GameObject attack3act1GO;
        public GameObject attack3act2GO;
        public GameObject shieldCollider;

        [Header("Effects")]
        [SerializeField] private ParticleSystem chargeEffect;

        private GameObject bullet;

        private void Start()
        {
            BaseStart();

            // Colliders
            attack1act1GO.SetActive(false);
            attack1act2GO.SetActive(false);
            attack1act3GO.SetActive(false);
            attack3act1GO.SetActive(false);
            attack3act2GO.SetActive(false);

            // Initial mode
            SetMode(Mode.Fight);
        }

        private void Update()
        {
            BaseUpdate();
        }

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }
            switch (methdodId)
            {
                case M_AnimAttack1: AnimAttack1(); return true;
                case M_SpearChargeEffect: SpearChargeEffect(); return true;
                case M_AnimAttack2: AnimAttack2(); return true;
                case M_InitBullet: InitBullet(); return true;
                case M_FireBullet: FireBullet(); return true;
                case M_AnimAttack3: AnimAttack3(); return true;
                case M_HeadNoTarget: HeadNoTarget(); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_AnimAttack1 = 0;
        private void AnimAttack1()
        {
            anim.Play("Attack1");
        }

        // Called by id
        public const int M_SpearChargeEffect = 1;
        private void SpearChargeEffect()
        {
            chargeEffect.GetComponent<ParticleSystem>().Play();
        }

        // Called by id
        public const int M_AnimAttack2 = 2;
        private void AnimAttack2()
        {
            anim.Play("Attack2");
        }

        // Called by id
        public const int M_InitBullet = 3;
        private void InitBullet()
        {
            bullet = GameObject.Instantiate(bulletPrefab, transform, false);
            bullet.transform.localPosition = new Vector3(1.68f, 14.4f, .34f);
            bullet.transform.SetParent(null, true);
        }

        // Called by id
        public const int M_FireBullet = 4;
        private void FireBullet()
        {
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 30f, ForceMode.Impulse);
            Destroy(bullet, .5f);
        }

        // Called by id
        public const int M_AnimAttack3 = 5;
        private void AnimAttack3()
        {
            anim.Play("Attack3");
        }

        // Called by id
        public const int M_HeadNoTarget = 6;
        private void HeadNoTarget()
        {
            head.NoTarget();
        }

        protected override bool CallById(int methdodId, GameObject go, float f)
        {
            if (base.CallById(methdodId, go, f))
            {
                return true;
            }
            switch (methdodId)
            {
                case M_HeadLookForTarget: HeadLookForTarget(go, f); return true;
                case M_HeadLookAtTarget: HeadLookAtTarget(go, f); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_HeadLookForTarget = 700;
        private void HeadLookForTarget(GameObject go, float duration)
        {
            head.LookForTarget(go, duration);
        }

        // Called by id
        public const int M_HeadLookAtTarget = 701;
        private void HeadLookAtTarget(GameObject go, float dummy)
        {
            head.LookAtTarget(go);
        }
    }
}
