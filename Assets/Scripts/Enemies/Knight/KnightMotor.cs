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
        public GameObject attack1actGO;
        public GameObject attack1Container;
        public GameObject attack3act1GO;
        public GameObject attack3act2GO;
        public GameObject shieldCollider;

        [Header("Effects")]
        public ParticleSystem chargeEffect;

        private void Start()
        {
            BaseStart();

            // Colliders
            attack1actGO.SetActive(false);
            attack3act1GO.SetActive(false);
            attack3act2GO.SetActive(false);

            // Initial mode
            SetMode(Mode.Fight);
        }

        private void Update()
        {
            BaseUpdate();
        }
    }
}
