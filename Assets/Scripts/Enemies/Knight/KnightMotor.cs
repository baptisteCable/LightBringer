using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightMotor : Motor
    {
        [Header("Indicators")]
        // Indicators
        public GameObject Attack1Indicator1;
        public GameObject Attack1Indicator2;
        public GameObject Attack1Indicator3;
        public GameObject Attack3Indicator1;
        public GameObject Attack3Indicator2;

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

        public override void Start()
        {
            base.Start();

            // Colliders
            attack1act1GO.SetActive(false);
            attack1act2GO.SetActive(false);
            attack1act3GO.SetActive(false);
            attack3act1GO.SetActive(false);
            attack3act2GO.SetActive(false);

            // Initial mode
            SetMode(Mode.Fight);
        }
    }
}
