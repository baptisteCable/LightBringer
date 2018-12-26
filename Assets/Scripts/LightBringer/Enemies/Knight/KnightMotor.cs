using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.Knight
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightMotor : EnemyMotor
    {
        protected override float MAX_MOVE_SPEED_PASSIVE { get { return 4f; } }
        protected override float MAX_MOVE_SPEED_FIGHT { get { return 12f; } }
        protected override float MAX_MOVE_SPEED_RAGE { get { return 20f; } }

        protected override float ACCELERATION_PASSIVE { get { return 4f; } }
        protected override float ACCELERATION_FIGHT { get { return 40f; } }
        protected override float ACCELERATION_RAGE { get { return 80f; } }

        protected override float ROTATION_ACCELERATION_PASSIVE { get { return 750f; } }
        protected override float ROTATION_ACCELERATION_FIGHT { get { return 1500f; } }
        protected override float ROTATION_ACCELERATION_RAGE { get { return 3000f; } }

        protected override float MAX_ROTATION_SPEED_PASSIVE { get { return 180f; } }
        protected override float MAX_ROTATION_SPEED_FIGHT { get { return 520f; } }
        protected override float MAX_ROTATION_SPEED_RAGE { get { return 800f; } }

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
