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
        public ParticleSystem chargeEffect;

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
    }
}
