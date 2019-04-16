using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightMotor : Motor
    {
        [Header("Prefabs")]
        public GameObject bulletPrefab;
        public GameObject casterPrefab;
        public GameObject attack1GroundActGOPrefab;
        public GameObject attack1GroundRendererGOPrefab;
        public GameObject attack4RayColliderPrefab;
        public GameObject attack4RayRendererPrefab;
        public GameObject attack4ExplColliderPrefab;
        public GameObject attack4ExplRendererPrefab;

        [Header("Game Objects")]
        public GameObject attack1actGO;
        public GameObject attack1Container;
        public GameObject attack3act1GO;
        public GameObject attack3act2GO;
        public GameObject shieldCollider;
        public GameObject charge1actGO;

        [Header("Effects")]
        public ParticleSystem chargeEffect;
        public ParticleSystem attack4ChannelingEffect;

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
