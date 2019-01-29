﻿using LightBringer.Player.Abilities;
using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerStatusManager))]
    [RequireComponent(typeof(CharacterController))]
    public class Character : MonoBehaviour
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

        // misc
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

        // Movement
        private Vector3 movementDirection;
        private MovementMode movementMode;
        public float stickToGroundForce;
        private Vector3 previousPosition;

        // Training
        public bool ignoreCD = false;

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
        public virtual void Start()
        {
            charController = GetComponent<CharacterController>();
            psm = GetComponent<PlayerStatusManager>();

            abilityMoveMultiplicator = 1f;
            abilityMaxRotation = -1f;

            movementDirection = Vector3.zero;
            previousPosition = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            if (psm.isDead)
            {
                return;
            }

            // Move
            Move();

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
                    if (ignoreCD)
                    {
                        abilities[i].coolDownRemaining = -.01f;
                    }
                    else
                    {
                        abilities[i].coolDownRemaining -= Time.deltaTime;
                    }
                    abilities[i].coolDownUp = (abilities[i].coolDownRemaining < 0);
                }

            }

            // Special computations on abilities
            ComputeAbilitiesSpecial();
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

            // AbDef
            if (Input.GetButton("SkillDef"))
            {
                abilities[3].StartChanneling();
            }

            // AbOff
            if (Input.GetButton("SkillOff"))
            {
                abilities[4].StartChanneling();
            }

            // AbUlt
            if (Input.GetButtonDown("SkillUlt"))
            {
                abilities[5].StartChanneling();
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
                // Moving
                float v = Input.GetAxisRaw("Vertical");
                float h = Input.GetAxisRaw("Horizontal");
                Vector3 desiredMove;

                if (movementMode == MovementMode.Player && !psm.isRooted && !psm.isStunned && (v * v + h * h) > .01f)
                {
                    desiredMove = new Vector3(v + h, 0, v - h).normalized;

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
                    desiredMove = Vector3.zero;

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

        // look at mouse and camera positionning procedure
        void lookAtMouse()
        {
            if ((GameManager.gm.worldMousePoint - new Vector3(transform.position.x, GameManager.gm.currentAlt, transform.position.z)).magnitude > 0)
            {
                // Smoothly rotate towards the target point.
                var targetRotation = Quaternion.LookRotation(
                        GameManager.gm.worldMousePoint - new Vector3(transform.position.x, GameManager.gm.currentAlt, transform.position.z)
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

            // Set Physics material
            // TODO if needed
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

        public void Die()
        {
            if (currentChanneling != null)
            {
                currentChanneling.AbortChanelling();
            }
            if(currentAbility != null)
            {
                currentAbility.AbortCasting();
            }
            charController.enabled = false;
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
     * 
     * AbOff (seconde partie) ajouter channeling et slashEffect (type Ab1b)
     * 
     * Camera quand on monte. Gestion du 1er étage en général (chute, compétences qui partent du niveau 0, etc.)
     * Variable d'état indiquant l'étage en cours ? Ou l'altitude du sol ? Que se passe-t-il alors quand on saute
     * par dessus un ilot ?
     * 
     * Multijoueur
     * 
     * Pentes des ilots : bords progressifs pour la texture du chemin
     * 
     * Icones de compétence
     * Ulti visible au loin (Fiora)
     * Counter : on peut bouger et tourner. Il ne faut pas.
     * Attaque chargée qui ne met pas toujours les dégâts chargés (Bob qui part de loin)
     * Ulti : animation : pas toujours
     * Désactiver tous les indicateurs à la mort du monstre
     * 
     * Enemy not always focusing player. Laser indiquant la direction du regard. Se teinte avant une action offensive ?
     * 
     * mise en file des compétences :
     *  - pendant le cast, mais pas pendant le channeling
     *  - buttondown pour tout sauf clic gauche (pourrait dépendre de la compétence, faire ce test au chargement et laisser
     *      les variables dans cette classe)
     *  - test de la disponibilité à ce moment. Si pas dispo, file non remplacée et bruit d'erreur (et visible sur l'image)
     * */
}
