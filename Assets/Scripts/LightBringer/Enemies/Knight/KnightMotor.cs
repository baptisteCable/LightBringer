using UnityEngine;

namespace LightBringer.Enemies.Knight
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightMotor : Motor
    {
        // Colliders GO
        [HideInInspector]
        public GameObject attack1act1GO;
        [HideInInspector]
        public GameObject attack1act2GO;
        [HideInInspector]
        public GameObject attack1act3GO;
        [HideInInspector]
        public GameObject attack3act1GO;
        [HideInInspector]
        public GameObject attack3act2GO;
        [HideInInspector]
        public GameObject shieldCollider;

        // Indicators
        public GameObject Attack1Indicator1;
        public GameObject Attack1Indicator2;
        public GameObject Attack1Indicator3;
        public GameObject Attack3Indicator1;
        public GameObject Attack3Indicator2;

        public override void Start()
        {
            base.Start();

           // Colliders
            attack1act1GO = transform.Find("EnemyContainer/Attack1Trigger").gameObject;
            attack1act2GO = transform.Find("EnemyContainer/Armature/BoneControlerShield/ShieldAttackTrigger").gameObject;
            attack1act3GO = transform.Find("EnemyContainer/Armature/BoneControlerSpear/SpearAttackTrigger").gameObject;
            attack3act1GO = transform.Find("EnemyContainer/Armature/BoneControlerSpear/Attack3aTrigger").gameObject;
            attack3act2GO = transform.Find("EnemyContainer/Armature/BoneControlerShield/Attack3bTrigger").gameObject;
            attack1act1GO.SetActive(false);
            attack1act2GO.SetActive(false);
            attack1act3GO.SetActive(false);
            attack3act1GO.SetActive(false);
            attack3act2GO.SetActive(false);

            shieldCollider = transform.Find("EnemyContainer/Armature/BoneControlerShield/ShieldCollider").gameObject;

            // Initial mode
            SetMode(Mode.Fight);
        }
    }
}
