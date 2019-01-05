using UnityEngine;
using LightBringer.Player.Abilities;
using LightBringer.Player.Abilities.Light.LongSword;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerStatusManager))]
    [RequireComponent(typeof(Rigidbody))]
    public class Character : MonoBehaviour
    {
        float currentRotationSpeed;

        // constants
        private const float ROTATION_SPEED = 24f;

        public float moveSpeed;
        private float rotationSpeed = ROTATION_SPEED;

        // game objects
        public Camera cam;
        public Transform characterContainer;

        // Components
        public Animator animator;
        [HideInInspector]
        public Rigidbody rb;
        [HideInInspector]
        public PlayerStatusManager psm;

        // misc
        private MovementMode movementMode;
        [HideInInspector]
        public float abilityMoveMultiplicator;
        [HideInInspector]
        public float abilityMaxRotation = 0f;

        // body parts
        public Transform weaponSlotR;
        [HideInInspector]
        public GameObject swordObject;


        /* Abilities :
         *      0: None
         *      1: Jump
         *      
         * Mechanics :
         *      Step 1: Casting. Starting channeling
         *      Step 2: Channeling. Can be cancelled. Target can be modified. Channeling animation.
         *      Step 3: Casting. When channeling ends, cast the ability. Can't be cancelled manually. 
         */
        public Ability currentAbility = null;
        public Ability currentChanneling = null;
        public Ability[] abilities;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            psm = GetComponent<PlayerStatusManager>();

            // TEST
            GameObject swordPrefab = Resources.Load("Player/Light/LongSword/Sword") as GameObject;
            swordObject = Instantiate(swordPrefab, weaponSlotR);
            LightSword sword = swordObject.GetComponent<LightSword>();

            // Abilities
            abilities = new Ability[5];

            // jump ability[0]
            abilities[0] = new Jump0(this);
            abilities[1] = new Ab1(this, sword);
            abilities[2] = new Ab2(this, sword);
            abilities[3] = new RaySpell(this);
            abilities[4] = new CubeSkillShot(this);

            abilityMoveMultiplicator = 1f;
            abilityMaxRotation = -1f;


        }

        // Update is called once per frame
        void Update()
        {
            // CC progression
            psm.CCComputation();

            // look at mouse point and move camera
            if (!psm.isStunned)
            {
                lookAtMouse();
            }

            // depending on blocking CC
            if (!psm.isInterrupted && !psm.isStunned)
            {
                // jump
                if (Input.GetButton("Jump") && abilities[0].coolDownUp && !psm.isRooted)
                {
                    Cancel();
                    if (currentAbility == null)
                    {
                        abilities[0].StartChanneling();
                    }
                }

                // Ab1
                if (Input.GetButton("Skill1") && currentAbility == null && currentChanneling == null && abilities[1].coolDownUp)
                {
                    abilities[1].StartChanneling();
                }

                // Ab2
                if (Input.GetButton("Skill2") && currentAbility == null && currentChanneling == null && abilities[2].coolDownUp)
                {
                    abilities[2].StartChanneling();
                }

                // AbOff
                if (Input.GetButton("SkillOff") && currentAbility == null && currentChanneling == null && abilities[3].coolDownUp)
                {
                    abilities[3].StartChanneling();
                }

                // AbDeff
                if (Input.GetButtonDown("SkillDeff") && currentAbility == null && currentChanneling == null && abilities[4].coolDownUp)
                {
                    abilities[4].StartChanneling();
                }

                // Cancel
                if (Input.GetButtonDown("CancelChanneling"))
                {
                    Cancel();
                }
            }

            // CC effects on channeling and casting
            CCConsequences();

            // Channel
            if (currentChanneling != null)
            {
                currentChanneling.Channel();
            }

            // Cast ability
            if (currentAbility != null)
            {
                currentAbility.Cast();
            }

            // cooldowns
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].coolDownRemaining > 0)
                {
                    abilities[i].coolDownRemaining -= Time.deltaTime;
                    abilities[i].coolDownUp = (abilities[i].coolDownRemaining < 0);
                }

            }
        }

        private void Cancel()
        {
            if (currentChanneling != null && currentChanneling.channelingCancellable)
            {
                currentChanneling.CancelChanelling();
            }
            if (currentAbility != null && currentAbility.castingCancellable)
            {
                Debug.Log("Cancel casting");
                currentAbility.AbortCasting();
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        // look at mouse and camera positionning procedure
        void lookAtMouse()
        {
            // Smoothly rotate towards the target point.
            var targetRotation = Quaternion.LookRotation(
                    GameManager.gm.lookedPoint - new Vector3(transform.position.x, GameManager.gm.lookingHeight, transform.position.z)
                );
            Quaternion rotation = Quaternion.Slerp(
                    characterContainer.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

            currentRotationSpeed = Vector3.SignedAngle(characterContainer.forward, rotation * Vector3.forward, Vector3.up) / Time.deltaTime;

            if (abilityMaxRotation >= 0 && Mathf.Abs(currentRotationSpeed) > abilityMaxRotation)
            {
                characterContainer.Rotate(Vector3.up, ((currentRotationSpeed > 0) ? 1 : -1) * abilityMaxRotation * Time.deltaTime);
            }
            else
            {
                characterContainer.rotation = rotation;
            }
        }

        // move procedure
        void Move()
        {
            // Moving
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");
            if (movementMode == MovementMode.Player && !psm.isInterrupted && !psm.isRooted && !psm.isStunned && (v * v + h * h) > .01f)
            {
                Vector3 direction = new Vector3(v + h, 0, v - h);

                animator.SetBool("isMoving", rb.velocity != Vector3.zero);
                rb.velocity = (direction.normalized * moveSpeed * abilityMoveMultiplicator);
            }
            else
            {
                animator.SetBool("isMoving", false);
                if (movementMode == MovementMode.Player)
                {
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                }
            }
        }

        private void CCConsequences()
        {
            if (currentChanneling != null)
            {
                if (psm.isStunned)
                {
                    currentChanneling.AbortChanelling();
                }
                if (psm.isInterrupted)
                {
                    currentChanneling.AbortChanelling();
                }
            }
            if (currentAbility != null)
            {
                if (psm.isStunned)
                {
                    currentAbility.AbortCasting();
                }
                if (psm.isInterrupted)
                {
                    currentAbility.AbortCasting();
                }
            }
        }

        public void SetMovementMode(MovementMode mode)
        {
            movementMode = mode;

            // Set Physics material
            // TODO if needed
        }
        /*
        private void OnGUI()
        {
            GUI.contentColor = Color.black;
            GUILayout.BeginArea(new Rect(20, 20, 250, 120));
            GUILayout.Label("Knight velocity : " + velocity);
            GUILayout.Label("Update : " + agent.updatePosition);
            GUILayout.Label("Trans Position : " + transform.position);
            GUILayout.Label("Next  Position : " + agent.nextPosition);
            GUILayout.EndArea();
        }
        */
    }

    /* 
     * 
     * La charge pousse le joueur et ne monte pas dessus.
     * 
     * Ab2 : frapper dans l'ordre de proximité (car si shield)
     * 
     * Effets de particules chargement épée lumière
     * 
     * Particules à l'impact
     *
     * 
     * */
}

