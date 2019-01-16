using UnityEngine;
using LightBringer.Player.Abilities;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerStatusManager))]
    [RequireComponent(typeof(Rigidbody))]
    public class Character : MonoBehaviour
    {
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
        [HideInInspector]
        public Collider coll;

        // misc
        private MovementMode movementMode;
        [HideInInspector]
        public float abilityMoveMultiplicator;
        [HideInInspector]
        public float abilityMaxRotation = 0f;
        public bool visible = true;
        float currentRotationSpeed;

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
        public Ability specialCancelAbility = null;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            psm = GetComponent<PlayerStatusManager>();
            coll = GetComponent<Collider>();

            // TEST
            GameObject swordPrefab = Resources.Load("Player/Light/LongSword/Sword/LightLongSword") as GameObject;
            swordObject = Instantiate(swordPrefab, weaponSlotR);
            Abilities.Light.LongSword.LightSword sword = swordObject.GetComponent<Abilities.Light.LongSword.LightSword>();

            // Abilities
            abilities = new Ability[5];

            // jump ability[0]
            abilities[0] = new Abilities.Light.LongSword.AbEsc(this, sword);
            abilities[1] = new Abilities.Light.LongSword.Ab1(this, sword);
            abilities[2] = new Abilities.Light.LongSword.Ab2(this, sword);
            abilities[3] = new Abilities.Light.LongSword.AbOff(this, sword);
            abilities[4] = new Abilities.Light.LongSword.AbDef(this, sword);

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

            // Check buttons and start abilities
            StartAbilities();

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

            // Special computations on abilities
            ComputeAbilitiesSpecial();

            // Anchor move (no action, most of the time)
            Move();
        }

        private void ComputeAbilitiesSpecial()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].ComputeSpecial();
            }
        }

        private void StartAbilities()
        {
            // Ab1
            if (Input.GetButton("Skill1"))
            {
                abilities[1].StartChanneling();
            }

            // Ab2
            if (Input.GetButton("Skill2"))
            {
                abilities[2].StartChanneling();
            }

            // AbOff
            if (Input.GetButton("SkillOff"))
            {
                abilities[3].StartChanneling();
            }

            // AbDef
            if (Input.GetButton("SkillDef") && currentAbility == null && currentChanneling == null && abilities[4].coolDownUp)
            {
                abilities[4].StartChanneling();
            }

            // Escape
            if (Input.GetButton("SkillEsc"))
            {
                abilities[0].StartChanneling();
            }

            // Cancel
            if (Input.GetButtonDown("CancelChanneling"))
            {
                Cancel();
            }

            // Test
            if (Input.GetButtonDown("testButton"))
            {
                psm.AddAndStartState(new Haste());
            }
        }

        public void Cancel()
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

            // Special effect of cancelling on some abilities
            if (specialCancelAbility != null)
            {
                specialCancelAbility.SpecialCancel();
            }

            // Cancel states
            psm.CancelStates();
        }

        private void Move()
        {
            if (movementMode == MovementMode.Anchor)
            {
                if (psm.anchor != null)
                {
                    transform.position = psm.anchor.position;
                }
                else
                {
                    Debug.LogError("No anchor object");
                }
            }
        }

        private void FixedUpdate()
        {
            MoveFixed();
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
        void MoveFixed()
        {
            // Moving
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");
            if (movementMode == MovementMode.Player && !psm.isInterrupted && !psm.isRooted && !psm.isStunned && (v * v + h * h) > .01f)
            {
                Vector3 direction = new Vector3(v + h, 0, v - h).normalized;

                animator.SetFloat("zVel", Vector3.Dot(direction, characterContainer.forward));
                animator.SetFloat("xVel", Vector3.Dot(direction, characterContainer.right));
                animator.SetBool("isMoving", rb.velocity != Vector3.zero);
                rb.velocity = (direction * MoveSpeed());
            }
            else
            {
                if (movementMode != MovementMode.Anchor)
                {
                    animator.SetBool("isMoving", false);
                }

                if (movementMode == MovementMode.Player)
                {
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                }
            }
        }

        private float MoveSpeed()
        {
            return moveSpeed * abilityMoveMultiplicator * psm.hasteMoveMultiplicator;
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
            if (movementMode == MovementMode.Anchor && mode != MovementMode.Anchor)
            {
                psm.anchor = null;
                MakeVisible();
                coll.enabled = true;
            }

            movementMode = mode;

            // Set Physics material
            // TODO if needed
        }

        public void MergeWith(Transform anchor, bool hide = true)
        {
            psm.anchor = anchor;
            coll.enabled = false;
            MakeInvisible();
            rb.velocity = Vector3.zero;
            movementMode = MovementMode.Anchor;
        }

        private void MakeVisible()
        {
            visible = true;
            characterContainer.gameObject.SetActive(true);
        }

        private void MakeInvisible()
        {
            visible = false;
            characterContainer.gameObject.SetActive(false);
        }
        
        public void LockAbilitiesExcept(bool locked, Ability ab = null)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] != ab)
                {
                    abilities[i].locked = locked;
                }
            }
        }
        /*
        private void OnGUI()
        {
            GUI.contentColor = Color.black;
            GUILayout.BeginArea(new Rect(20, 20, 250, 120));
            GUILayout.Label("States count: " + states.Count);
            GUILayout.EndArea();
        }*/
    }

    /* 
     * 
     * La charge pousse le joueur et ne monte pas dessus.
     * 
     * Chercher autre méthode Slash
     * 
     * Cumuler les dégâts par type puis les afficher tous les .5 secondes
     * 
     * Afficher les dégâts
     * 
     * Transparent pas comme ça. Shader
     * Shader lumière (ou particules ?)
     * 
     * Trigger charge Knight
     * Knight : ralentir rotation
     * 
     * Knight : mode rage impacte les CD et la vitesse de cast (?).
     * 
     * State Cancel animation (pas d'animator dans le psm ?)
     * 
     * Synchronisation des idle et run top et bot ?
     * 
     * */
}

