using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Knight
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightMotor : EnemyMotor
    {
        // Colliders GO
        [HideInInspector]
        public GameObject attack1act1GO;
        [HideInInspector]
        public GameObject attack1act2GO;
        [HideInInspector]
        public GameObject attack1act3GO;

        private void Start()
        {
            StartProcedure();

           // Colliders
            attack1act1GO = transform.Find("EnemyContainer/Attack1Trigger").gameObject;
            attack1act2GO = transform.Find("EnemyContainer/Armature/BoneControlerShield/ShieldAttackTrigger").gameObject;
            attack1act3GO = transform.Find("EnemyContainer/Armature/BoneControlerSpear/SpearAttackTrigger").gameObject;
            attack1act1GO.SetActive(false);
            attack1act2GO.SetActive(false);
            attack1act3GO.SetActive(false);

            // Initial mode
            SetMode(EnemyMode.Fight);
        }

        private void Update()
        {
            UpdateProcedure();
        }
    }
}
