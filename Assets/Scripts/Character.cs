using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightBringer;

public class Character : MonoBehaviour {
    // status
    public float maxHP;
    public float currentHP;

    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    // inputs
    private string inputTop = "z";
    private string inputBottom = "s";
    private string inputLeft = "q";
    private string inputRight = "d";

    // game objects
    public Camera cam;
    public Transform characterContainer;
    public GameManager gm;

    public GameObject landingIndicatorPrefab;
    public GameObject rangeIndicatorPrefab;

    public Animator m_Animator;
    private Rigidbody rb;

    // misc
    public Vector3 lookingPoint;
    private bool physicsApplies = false;
    public bool canRotate;


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
    private Ability[] abilities;

   
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();

        // Abilities
        abilities = new Ability[2];

        // jump ability[0]
        abilities[0] = new Jump0(this);
        abilities[1] = new Attack1(this);

        canRotate = true;
}

    // Update is called once per frame
    void Update () {

        // look at mouse point and move camera
        lookAtMouse();

        // jump
        if (Input.GetKey(KeyCode.Space) && currentAbility == null && abilities[0].coolDownUp)
        {
            if (currentChanneling != null)
            {
                currentChanneling.CancelChanelling();
            }

            // animation
            m_Animator.Play("JumpChanneling");
            abilities[0].StartChanneling();
        }

        // main attack
        if (Input.GetMouseButton(0) && currentAbility == null && currentChanneling == null && abilities[1].coolDownUp)
        {
            abilities[1].StartChanneling();
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
        move();
    }

    // look at mouse and camera positionning procedure
    void lookAtMouse()
    {
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        
        float distance;

        if (gm.lookingPlane.Raycast(mouseRay, out distance))
        {
            lookingPoint = mouseRay.GetPoint(distance);

            if (canRotate)
            {
                // Smoothly rotate towards the target point.
                var targetRotation = Quaternion.LookRotation(lookingPoint - new Vector3(transform.position.x, gm.lookingHeight, transform.position.z));
                characterContainer.rotation = Quaternion.Slerp(characterContainer.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

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
        if (currentAbility == null && (Input.GetKey(inputTop) || Input.GetKey(inputBottom) || Input.GetKey(inputLeft) || Input.GetKey(inputRight)))
        {
            Vector3 direction = new Vector3(
                    (Input.GetKey(inputTop) ? 1f : 0f) - (Input.GetKey(inputBottom) ? 1f : 0f)
                    + (Input.GetKey(inputRight) ? 1f : 0f) - (Input.GetKey(inputLeft) ? 1f : 0f),
                    0,
                    (Input.GetKey(inputTop) ? 1f : 0f) - (Input.GetKey(inputBottom) ? 1f : 0f)
                    - (Input.GetKey(inputRight) ? 1f : 0f) + (Input.GetKey(inputLeft) ? 1f : 0f)
                );

            rb.velocity = (direction.normalized * moveSpeed);
            m_Animator.SetBool("isMoving", true);
        }
        else
        {
            m_Animator.SetBool("isMoving", false);
            if (!physicsApplies)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
            }
        }
    }

    private void OnGUI()
    {
        GUI.contentColor = Color.black;
        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Position sur le plan: " + (lookingPoint ));
        GUILayout.Label("Position camera: " + cam.transform.position);
        GUILayout.Label("Static: " + gm.staticCamera);
        GUILayout.EndArea();
        
    }
}

/*
 * Compétences, combat, CD des compétences
 * Mouvement réduit pendant la canalisation, possibilité de bouger encore le curseur.
 * Compétence peut être annulée. int pour canalisation avec le numéro de la capacité canalisée.
 * 
 * */