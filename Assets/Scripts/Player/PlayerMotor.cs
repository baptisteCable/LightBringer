using System.Collections.Generic;
using LightBringer.Player.Abilities;
using LightBringer.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerStatusManager))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerController))]
    public abstract class PlayerMotor : NetworkBehaviour
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
        [HideInInspector]
        public PlayerStatusManager psm;
        private CharacterController charController;
        public PlayerController pc;

        // misc
        [HideInInspector]
        public float abilityMoveMultiplicator;
        [HideInInspector]
        public float abilityMaxRotation = 0f;
        public bool visible = true;
        float currentRotationSpeed;

        // body parts
        public Transform weaponSlotR;

        // Movement
        private Vector3 movementDirection;
        private MovementMode movementMode;
        public float stickToGroundForce;
        private Vector3 previousPosition;

        // Training
        public bool ignoreCD = false;

        // Camera
        public GameObject cameraPrefab;
        public GameObject playerCamera;

        // User Interface
        public GameObject userInterfacePrefab;
        private GameObject userInterface;

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
        public Ability specialCancelAbility = null;

        // Inputs and abilities
        public Dictionary<string, Ability> abilities;

        // Use this for initialization
        public virtual void Start()
        {
            /* ********* Everyone ********** */

            charController = GetComponent<CharacterController>();
            psm = GetComponent<PlayerStatusManager>();
            pc = GetComponent<PlayerController>();

            Init();
            

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

            // cooldowns
            RefreshCooldowns();

            // Special computations on abilities
            ComputeAbilitiesSpecial();
        }

        private void RefreshCooldowns()
        {
            foreach (Ability ab in abilities.Values)
            {
                if (ab.coolDownRemaining > 0)
                {
                    if (ignoreCD)
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
            foreach (Ability ab in abilities.Values)
            {
                ab.ComputeSpecial();
            }
        }

        private void StartAbilities()
        {
            foreach (KeyValuePair<string, Ability> abPair in abilities)
            {
                if (Input.GetButton(abPair.Key))
                {
                    abPair.Value.StartChanneling();
                }
            }

            // Cancel
            if (Input.GetButtonDown("CancelChanneling"))
            {
                Cancel();
            }

            // Test
            /*if (Input.GetButtonDown("testButton"))
            {
                psm.AddAndStartState(new Immaterial(20f));
            }*/
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
            if (isServer || isLocalPlayer)
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
                psm.anchor = null;
                MakeVisible();
                charController.enabled = true;
            }

            movementMode = mode;
        }

        public void MergeWith(Transform anchor, bool hide = true)
        {
            psm.anchor = anchor;
            charController.enabled = false;
            MakeInvisible();
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

        public void LockAbilitiesExcept(bool locked, Ability ability = null)
        {
            foreach (Ability ab in abilities.Values)
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

            abilities = new Dictionary<string, Ability>();
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
     * Flasher uniquement le DamageTaker qui a finalement pris les dégâts
     * Impact Effects only on hurt part?
     *  Shield qui clignotte quand on le tape
     * 
     * Camera quand on monte. Gestion du 1er étage en général (chute, compétences qui partent du niveau 0, etc.)
     * Variable d'état indiquant l'étage en cours ? Ou l'altitude du sol ? Que se passe-t-il alors quand on saute
     * par dessus un ilot ?
     * 
     * Compress textures
     * 
     * Pentes des ilots : bords progressifs pour la texture du chemin
     * 
     * Enemy not always focusing player. Laser indiquant la direction du regard. Se teinte avant une action offensive ?
     * 
     * mise en file des compétences :
     *  - pendant le cast, mais pas pendant le channeling
     *  - buttondown pour tout sauf clic gauche (pourrait dépendre de la compétence, faire ce test au chargement et laisser
     *      les variables dans cette classe)
     *  - test de la disponibilité à ce moment. Si pas dispo, file non remplacée et bruit d'erreur (et visible sur l'image)
     *  
     *  Base.Start à remplacer. ne pas override ces méthodes.
     *  
     *  Init() : voir ce qui est serveur, ce qui est local et ce qui est tout le monde
     *  
     *  Gêner le mostre : cancel de son attaque si on time bien un truc.
     *  
     *  Trouver comment compenser le ping avec les temps de canalisation.
     *  
     *  Ajouter un NetworkTimer qui calcule le ping qui stocke le delay. Il les affiche. Le delay est prélevé dans le transform sync (lié par l'inspector)
     *  Il calcule le delay en prennant le min des 10 dernières valeurs et des 10 derniers mins (donc 100 valeurs)
     *  
     * */
}
