using LightBringer.Networking;
using LightBringer.Player.Abilities;
using LightBringer.Tools;
using LightBringer.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerStatusManager))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerController))]
    public abstract class PlayerMotor : DelayedNetworkBehaviour
    {
        // constants
        private const float ROTATION_SPEED = 24f;
        private const float MOVE_SPEED = 8f;

        private float moveSpeed = MOVE_SPEED;
        private float rotationSpeed = ROTATION_SPEED;

        // game objects
        public Transform characterContainer;

        // Components
        public Animator animator;
        [HideInInspector] public PlayerStatusManager psm;
        private CharacterController charController;
        [HideInInspector] public PlayerController pc;

        // misc
        [HideInInspector] public float abilityMoveMultiplicator;
        [HideInInspector] public float abilityMaxRotation = 0f;
        [HideInInspector] public bool visible = true;
        private float currentRotationSpeed;

        // body parts
        public Transform weaponSlotR;

        // Movement
        private Vector3 movementDirection;
        private MovementMode movementMode;
        public float stickToGroundForce;
        private Vector3 previousPosition;

        // Camera
        [SerializeField] private GameObject cameraPrefab;
        [HideInInspector] public GameObject playerCamera;

        // User Interface
        public GameObject userInterfacePrefab;
        private GameObject userInterface;

        // Networking
        [SerializeField] private GameObject networkSynchronizerPrefab;

        /* Abilities :
         *      0: None
         *      1: Jump
         *      
         * Mechanics :
         *      Step 1: Casting. Starting channeling
         *      Step 2: Channeling. Can be cancelled. Target can be modified. Channeling animation.
         *      Step 3: Casting. When channeling ends, cast the ability. Can't be cancelled manually. 
         */
        [HideInInspector] public Ability currentAbility = null;
        [HideInInspector] public Ability currentChanneling = null;
        [HideInInspector] public Ability specialCancelAbility = null;

        // Inputs and abilities
        [HideInInspector] public Ability[] abilities;

        // Use this for initialization
        public virtual void Start()
        {
            /* ********* Everyone ********** */
            charController = GetComponent<CharacterController>();
            psm = GetComponent<PlayerStatusManager>();
            pc = GetComponent<PlayerController>();

            Init();

            /* ********* Server ********** */
            if (isServer && isLocalPlayer)
            {
                // NetworkSynchronizer
                GameObject ns = Instantiate(networkSynchronizerPrefab);
                NetworkServer.Spawn(ns);
            }

            /* ********* Local player ********** */
            if (isLocalPlayer)
            {
                // Test Manager
                if (TestManager.singleton != null)
                {
                    TestManager.singleton.SetPlayer(this);
                }

                // Camera
                playerCamera = Instantiate(cameraPrefab);
                playerCamera.GetComponent<PlayerCamera>().player = transform;
                pc.cam = playerCamera.GetComponent<Camera>();

                if (CameraManager.singleton != null)
                {
                    CameraManager.singleton.ActivatePlayerCamera();
                }

                // UI
                userInterface = Instantiate(userInterfacePrefab);
                userInterface.GetComponent<UserInterface>().SetPlayerMotor(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (psm.isDead)
            {
                return;
            }

            // look at mouse point
            if (!psm.isStunned)
            {
                LookAtMouse();
            }

            // cooldowns
            RefreshCooldowns();

            if (!isServer)
            {
                return;
            }

            // Move
            Move();

            // CC progression
            psm.CCComputation();

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

            // Special computations on abilities
            ComputeAbilitiesSpecial();
        }

        private void RefreshCooldowns()
        {
            foreach (Ability ab in abilities)
            {
                if (ab.coolDownRemaining > 0)
                {
                    if (GameManager.gm.ignoreCD)
                    {
                        ab.coolDownRemaining = -.01f;
                    }
                    else
                    {
                        ab.coolDownRemaining -= Time.deltaTime;
                    }
                    ab.coolDownUp = ab.coolDownRemaining < 0;
                }
            }
        }

        private void ComputeAbilitiesSpecial()
        {
            foreach (Ability ab in abilities)
            {
                ab.ComputeSpecial();
            }
        }

        private void StartAbilities()
        {
            // Check queue first
            if (pc.queue >= 0 && pc.queue < 6 && abilities[pc.queue].CanStart())
            {
                abilities[pc.queue].StartChanneling();
                pc.queue = PlayerController.IN_NONE;
            }

            // Check pressed button in a second time
            if (pc.pressedButton >= 0 && pc.pressedButton < 6 && abilities[pc.pressedButton].CanStart())
            {
                abilities[pc.pressedButton].StartChanneling();
            }

            // Cancel
            if (pc.queue == PlayerController.IN_CANCEL)
            {
                Cancel();
            }

            // TEST: Test button
            if (pc.queue == PlayerController.IN_TEST)
            {
                psm.AddAndStartState(new Immaterial(5f));
            }

            // Clear queue if nothing happening
            if (currentAbility == null && currentChanneling == null && pc.queue != PlayerController.IN_NONE)
            {
                pc.queue = PlayerController.IN_NONE;
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
            else if (movementMode == MovementMode.Player)
            {
                Vector3 desiredMove = Vector3.zero;

                if (!psm.isRooted && !psm.isStunned && pc.desiredMove.magnitude > .01f) // TODO : remove last condition if we know if really moving
                {
                    desiredMove = new Vector3(pc.desiredMove.x, 0, pc.desiredMove.y);

                    animator.SetFloat("zVel", Vector3.Dot(desiredMove, characterContainer.forward));
                    animator.SetFloat("xVel", Vector3.Dot(desiredMove, characterContainer.right));

                    bool reallyMoving = ((previousPosition - transform.position).magnitude / Time.deltaTime > moveSpeed / 6f);

                    animator.SetBool("isMoving", reallyMoving);

                    // get a normal for the surface that is being touched to move along it
                    RaycastHit hitInfo;
                    LayerMask mask = LayerMask.GetMask("Environment");

                    Physics.SphereCast(transform.position, charController.radius, Vector3.down, out hitInfo,
                                       charController.height / 2f, mask, QueryTriggerInteraction.Ignore);
                    desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
                }
                else
                {
                    animator.SetBool("isMoving", false);
                }

                // Store position before moving
                previousPosition = transform.position;

                // add moveSpeed
                movementDirection.x = desiredMove.x * MoveSpeed();
                movementDirection.z = desiredMove.z * MoveSpeed();

                if (charController.isGrounded)
                {
                    movementDirection.y = -stickToGroundForce;
                }
                else
                {
                    movementDirection += Physics.gravity * Time.deltaTime;
                }
                charController.Move(movementDirection * Time.deltaTime);
            }
        }

        public void AbilityMove(Vector3 speed)
        {
            charController.Move(speed * Time.deltaTime);
        }

        void LookAtMouse()
        {
            if (isServer)
            {
                if ((pc.pointedWorldPoint - new Vector3(transform.position.x, GameManager.gm.currentAlt, transform.position.z)).magnitude > 0)
                {
                    // Smoothly rotate towards the target point.
                    var targetRotation = Quaternion.LookRotation(
                            pc.pointedWorldPoint - new Vector3(transform.position.x, GameManager.gm.currentAlt, transform.position.z)
                        );
                    Quaternion rotation = Quaternion.Slerp(
                            characterContainer.rotation,
                            targetRotation,
                            rotationSpeed * Time.deltaTime
                        );

                    currentRotationSpeed = Vector3.SignedAngle(characterContainer.forward, rotation * Vector3.forward, Vector3.up) / Time.deltaTime;

                    float mrs = MaxRotationSpeed();

                    if (Mathf.Abs(currentRotationSpeed) > mrs)
                    {
                        characterContainer.Rotate(Vector3.up, ((currentRotationSpeed > 0) ? 1 : -1) * mrs * Time.deltaTime);
                    }
                    else
                    {
                        characterContainer.rotation = rotation;
                    }
                }
            }
        }

        private float MaxRotationSpeed()
        {
            float mrs = Mathf.Infinity;

            if (abilityMaxRotation >= 0)
            {
                mrs = abilityMaxRotation;
            }

            float stateMaxRS = psm.GetMaxRotationSpeed();

            if (stateMaxRS < mrs)
            {
                mrs = stateMaxRS;
            }

            return mrs;
        }

        private float MoveSpeed()
        {
            return moveSpeed * abilityMoveMultiplicator * psm.GetStateMoveSpeedMultiplicator();
        }

        private void CCConsequences()
        {
            if (currentChanneling != null)
            {
                if (psm.isStunned)
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
            }
        }

        public void SetMovementMode(MovementMode mode)
        {
            if (movementMode == MovementMode.Anchor && mode != MovementMode.Anchor)
            {
                charController.enabled = true;
                psm.anchor = null;
                CallForAll(M_MakeVisible);
            }

            movementMode = mode;
        }

        public void MergeWith(Transform anchor, bool hide = true)
        {
            charController.enabled = false;
            movementMode = MovementMode.Anchor;
            psm.anchor = anchor;
            CallForAll(M_MakeInvisible);
        }

        public void LockAbilitiesExcept(bool locked, Ability ability = null)
        {
            foreach (Ability ab in abilities)
            {
                if (ab != ability)
                {
                    ab.locked = locked;
                }
            }
        }

        public void Die()
        {
            if (currentChanneling != null)
            {
                currentChanneling.AbortChanelling();
            }
            if (currentAbility != null)
            {
                currentAbility.AbortCasting();
            }
            charController.enabled = false;
        }

        [Command]
        public virtual void CmdServerInit()
        {
            psm.Init();
            Init();

            charController.enabled = true;

            movementDirection = Vector3.zero;
            previousPosition = Vector3.zero;

            // call on clients too
            RpcClientInit();
        }

        [ClientRpc]
        public void RpcClientInit()
        {
            if (!isServer)
            {
                Init();
            }
        }

        protected virtual void Init()
        {
            psm.Init();

            abilityMoveMultiplicator = 1f;
            abilityMaxRotation = -1f;
        }

        private void OnDestroy()
        {
            Destroy(playerCamera);
            Destroy(userInterface);

            if (isLocalPlayer && CameraManager.singleton != null)
            {
                CameraManager.singleton.DisactivatePlayerCamera();
            }

            // Test Manager
            if (TestManager.singleton != null)
            {
                TestManager.singleton.RemovePlayer();
            }
        }

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_MakeVisible: MakeVisible(); return true;
                case M_MakeInvisible: MakeInvisible(); return true;
            }

            return false;
        }

        // Called by id
        public const int M_MakeVisible = 1000;
        private void MakeVisible()
        {
            visible = true;
            characterContainer.gameObject.SetActive(true);
        }

        // Called by id
        public const int M_MakeInvisible = 1001;
        private void MakeInvisible()
        {
            visible = false;
            characterContainer.gameObject.SetActive(false);
        }

        protected override bool CallById(int methdodId, int i)
        {
            if (base.CallById(methdodId, i))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_ClearIndicators: ClearIndicators(i); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_ClearIndicators = 1300;
        private void ClearIndicators(int id)
        {
            abilities[id].indicators.Clear();
        }

        protected override bool CallById(int methdodId, int i, float f)
        {
            if (base.CallById(methdodId, i, f))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_SetCdDuration: SetCdDuration(i, f); return true;
                case M_SetCdRemaining: SetCdRemaining(i, f); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_SetCdDuration = 1400;
        private void SetCdDuration(int id, float cd)
        {
            abilities[id].coolDownDuration = cd;
        }

        // Called by id
        public const int M_SetCdRemaining = 1401;
        private void SetCdRemaining(int id, float cd)
        {
            abilities[id].coolDownRemaining = cd;
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
     * La charge pousse le joueur sur le côté et ne monte pas dessus. Le Knight n'est pas bloqué par le joueur.
     * Effets Knight (tenter les slashs nouveaux ?)
     * 
     * Transparent pas comme ça. Shader
     * Shader lumière (ou particules ?)
     * 
     * Knight : mode rage impacte les CD et la vitesse de cast (?).
     * 
     * Synchronisation des idle et run top et bot ?
     * Ralentir l'animation de course en fonction du modificateur de vitesse
     * 
     * Camera quand on monte. Gestion du 1er étage en général (chute, compétences qui partent du niveau 0, etc.)
     * Variable d'état indiquant l'étage en cours ? Ou l'altitude du sol ? Que se passe-t-il alors quand on saute
     * par dessus un ilot ?
     * 
     * Compress textures
     * 
     * Pentes des ilots : bords progressifs pour la texture du chemin
     *  
     * Base.Start à remplacer. ne pas override ces méthodes.
     *  
     * Gêner le monstre : cancel de son attaque si on time bien un truc.
     *  
     * Commenter le code
     *  
     * Effet pour le scann
     * 
     * Network : channeling + compétence en cours
     * */
}
