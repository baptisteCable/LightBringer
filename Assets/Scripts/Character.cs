using UnityEngine;
using LightBringer;

public class Character : MonoBehaviour {
    // status
    public float maxHP;
    public float currentHP;
    public float maxMP;
    public float currentMP;

    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    // game objects
    public Camera cam;
    public Transform characterContainer;
    public GameManager gm;

    public Animator animator;
    private Rigidbody rb;

    // misc
    public Vector3 lookingPoint;
    private bool physicsApplies = false;
    public bool canRotate;
    public float abilityMoveMultiplicator;
    public float abilityRotationMultiplicator;

    // body parts
    public Transform weaponSlotR;
    public GameObject weaponR;

    // crowd control
    public bool isInterrupted = false;
    public float interruptedDuration;


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
    void Start () {
        rb = GetComponent<Rigidbody>();

        // TEST
        GameObject sword = Resources.Load("Weapons/Sword") as GameObject;
        weaponR = Instantiate(sword, weaponSlotR);
        
        // Abilities
        abilities = new Ability[5];

        // jump ability[0]
        abilities[0] = new Jump0(this);
        abilities[1] = new MeleeAttack1(this, weaponR);
        abilities[2] = new MeleeAoE1(this);
        abilities[3] = new RaySpell(this);
        abilities[4] = new CubeSkillShot(this);

        canRotate = true;
        abilityMoveMultiplicator = 1f;
        abilityRotationMultiplicator = 1f;

        
    }

    // Update is called once per frame
    void Update () {
        // CC progression
        if (isInterrupted)
        {
            interruptedDuration -= Time.deltaTime;
            if (interruptedDuration <= 0f)
            {
                isInterrupted = false;
                animator.SetBool("isInterrupted", false);
            }
        }

        // look at mouse point and move camera
        lookAtMouse();

        // depending on blocking CC
        if (!isInterrupted)
        {
            // jump
            if (Input.GetButtonDown("Jump") && currentAbility == null && abilities[0].coolDownUp)
            {
                if (currentChanneling != null)
                {
                    if (currentChanneling.channelingCancellable)
                        currentChanneling.CancelChanelling();
                    abilities[0].StartChanneling();
                }
                else
                {
                    abilities[0].StartChanneling();
                }

            }

            // Sword attack
            if (Input.GetButton("Skill1") && currentAbility == null && currentChanneling == null && abilities[1].coolDownUp)
            {
                abilities[1].StartChanneling();
            }

            // Melee AoE
            if (Input.GetButton("Skill2") && currentAbility == null && currentChanneling == null && abilities[2].coolDownUp)
            {
                abilities[2].StartChanneling();
            }

            // Ray Spell
            if (Input.GetButton("Skill3") && currentAbility == null && currentChanneling == null && abilities[3].coolDownUp)
            {
                abilities[3].StartChanneling();
            }

            // Cube skill shot
            if (Input.GetButton("Skill4") && currentAbility == null && currentChanneling == null && abilities[4].coolDownUp)
            {
                abilities[4].StartChanneling();
            }

            // Cancel
            if (Input.GetButtonDown("CancelChanneling") && currentChanneling != null && currentChanneling.channelingCancellable)
            {
                currentChanneling.CancelChanelling();
            }
        }
        

        // Channel
        if (currentChanneling != null)
        {
            currentChanneling.Channel();
        }

        // Do ability
        if (currentAbility != null)
        {
            currentAbility.DoAbility();
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

    private void FixedUpdate()
    {
        // depending on blocking CC
        if (!isInterrupted)
        {
            move();
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    // look at mouse and camera positionning procedure
    void lookAtMouse()
    {
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        
        float distance;

        if (gm.lookingPlane.Raycast(mouseRay, out distance))
        {
            lookingPoint = mouseRay.GetPoint(distance);

            
            // Smoothly rotate towards the target point.
            var targetRotation = Quaternion.LookRotation(lookingPoint - new Vector3(transform.position.x, gm.lookingHeight, transform.position.z));
            characterContainer.rotation = Quaternion.Slerp(
                    characterContainer.rotation,
                    targetRotation,
                    abilityRotationMultiplicator * rotationSpeed * Time.deltaTime
                );
            

            // camera positionning
            if (gm.staticCamera)
            {
                cam.transform.position = new Vector3(
                        transform.position.x + gm.camPositionFromPlayer.x,
                        gm.camPositionFromPlayer.y,
                        transform.position.z + gm.camPositionFromPlayer.z
                    );
            }
            else
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(
                        transform.position.x + gm.camPositionFromPlayer.x + (lookingPoint.x - transform.position.x) * .3f,
                        gm.camPositionFromPlayer.y,
                        transform.position.z + gm.camPositionFromPlayer.z + (lookingPoint.z - transform.position.z) * .3f
                    ), Time.deltaTime * 8f);
            }
                 
        }     
    }

    // move procedure
    void move()
    {
        // Moving
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        if ((v * v + h * h) > .01f)
        {
            Vector3 direction = new Vector3(v + h, 0, v - h);

            animator.SetBool("isMoving", rb.velocity != Vector3.zero);
            rb.velocity = (direction.normalized * moveSpeed * abilityMoveMultiplicator);
        }
        else
        {
            animator.SetBool("isMoving", false);
            if (!physicsApplies)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }
    }

    public void Interrupt()
    {
        Debug.Log("Interrupt");
        // animation
        animator.SetBool("isInterrupted", true);

        isInterrupted = true;
        interruptedDuration = 1f;
    }

    /*
    private void OnGUI()
    {
        GUI.contentColor = Color.black;
        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Range : " + ((RaySpell)abilities[3]).range);
        GUILayout.EndArea();
        
    }
    */
}

/*
 * Barre de statut qui suit le personnage (world canvas)
 * 
 * Attack type l'épée touche. Attaque de rayon. Attaque de projectile.
 * 
 * IA ennemi
 * 
 * */