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

    private string inputTop = "z";
    private string inputBottom = "s";
    private string inputLeft = "q";
    private string inputRight = "d";

    public Camera cam;
    public Transform characterContainer;
    public GameManager gm;

    public GameObject landingIndicatorPrefab;
    public GameObject rangeIndicatorPrefab;

    private Vector3 lookingPoint;

    /* Abilities :
     *      0: None
     *      1: Jump
     *      
     * Mechanics :
     *      Step 1: Casting. Starting channeling
     *      Step 2: Channeling. Can be cancelled. Target can be modified. Channeling animation.
     *      Step 3: Casting. When channeling ends, cast the ability. Can't be cancelled manually. 
     */
    private int currentAbility = -1;
    private int currentChanneling = -1;

    private Ability[] abilities;

    private bool physicsApplies = false;

    public Animator m_Animator;
    private Rigidbody rb;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();

        // Abilities
        abilities = new Ability[1];

        // jump ability[0]
        abilities[0] = new Jump0(landingIndicatorPrefab, rangeIndicatorPrefab);


}

    // Update is called once per frame
    void Update () {

        // look at mouse point and move camera
        lookAtMouse();

        // jump
        if (Input.GetKey(KeyCode.Space) && currentAbility == -1 && abilities[0].coolDownUp)
        {
            Jump();
        }

        if (currentChanneling == 0)
        {
            ChannelingJump();
        }

        if (currentAbility == 0)
        {
            Jumping();
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

    private void LateUpdate()
    {
        
    }

    private void FixedUpdate()
    {
        move();
    }

    void lookAtMouse()
    {
        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        
        float distance;

        if (gm.lookingPlane.Raycast(mouseRay, out distance))
        {
            lookingPoint = mouseRay.GetPoint(distance);
           
            
            var targetRotation = Quaternion.LookRotation(lookingPoint - new Vector3 (transform.position.x, gm.lookingHeight, transform.position.z));
            // Smoothly rotate towards the target point.
            characterContainer.rotation = Quaternion.Slerp(characterContainer.rotation, targetRotation, rotationSpeed * Time.deltaTime);

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

    void move()
    {
        // Moving
        if (currentAbility != 0 && (Input.GetKey(inputTop) || Input.GetKey(inputBottom) || Input.GetKey(inputLeft) || Input.GetKey(inputRight)))
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

    void Jump()
    {
        // animation
        m_Animator.Play("JumpChanneling");

        abilities[0].StartChanneling(transform.position);

        currentChanneling = 0;
    }

    void ChannelingJump()
    {
        abilities[0].channelingTime += Time.deltaTime;

        abilities[0].ModifyTarget(transform.position, lookingPoint);

        if (abilities[0].channelingTime > abilities[0].channelingDuration)
        {
            currentChanneling = -1;
            currentAbility = 0;
            abilities[0].StartAbility(transform.position, lookingPoint);
        }
        else
        {
            transform.position = abilities[0].GetChannelingPosition();
        }
    }

    void Jumping()
    {
        abilities[0].abilityTime += Time.deltaTime;
        if (abilities[0].abilityTime > abilities[0].abilityDuration)
        {
            currentAbility = -1;
            abilities[0].End();
        }
        else
        {
            transform.position = abilities[0].GetAbilityPosition();
        }
        
    }
    
    private void OnGUI()
    {
        GUI.contentColor = Color.black;
        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Position sur le plan: " + (lookingPoint ));
        GUILayout.Label("Position camera: " + cam.transform.position);
        GUILayout.EndArea();
    }
}

/*
 * Compétences, combat, CD des compétences
 * Mouvement réduit pendant la canalisation, possibilité de bouger encore le curseur.
 * Compétence peut être annulée. int pour canalisation avec le numéro de la capacité canalisée.
 * 
 * */