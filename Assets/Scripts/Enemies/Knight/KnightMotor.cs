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
        public GameObject attack4ExplColliderPrefab;
        public GameObject attack4ExplRendererPrefab;

        [Header("Game Objects")]
        public GameObject attack1Container;
        public GameObject attack1actGO;
        public GameObject attack1actContainer;
        public RayRenderer attack1RayRenderer;
        public GameObject attack3act1GO;
        public GameObject attack3act2GO;
        public GameObject attack4Container;
        public GameObject attack4actGO;
        public GameObject attack4actContainer;
        public RayRenderer attack4RayRenderer;
        public GameObject shieldCollider;
        public GameObject charge1actGO;

        [Header("Effects")]
        public ParticleSystem chargeEffect;
        public ParticleSystem attack1ChannelingEffect;
        public ParticleSystem attack1ChannelingEffectRage;
        public ParticleSystem attack2ChannelingEffect;
        public ParticleSystem attack3ChannelingEffect;
        public ParticleSystem attack3Slash1Effect;
        public ParticleSystem attack3Slash2Effect;
        public ParticleSystem attack3ChannelingEffectRage;
        public ParticleSystem attack4ChannelingEffect;
        public ParticleSystem attack4ChannelingEffectRage;
        public ParticleSystem startRagePs;

        [Header("Scripts")]
        public Rage rage;

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

        public override void StartExhaustion()
        {
            rage.StartExhaustion();
        }

        public override void StopExhaustion()
        {
            rage.StopExhaustion();
        }
    }
}
