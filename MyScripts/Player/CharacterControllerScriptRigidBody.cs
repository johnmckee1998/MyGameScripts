using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControllerScriptRigidBody : MonoBehaviour
{
    private Rigidbody characterRB;

    public float speed = 6.0f;
    public float sprintSpeed = 9.0f;
    public float crouchSpeed = 0.5f;
    private float stanSpeed;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public bool moving = false;
    public float health = 100;
    public GameObject canvas;
    public Image healthBar;
    public Image detectBar;
    public bool useDetection = false;
    public bool useSphereGrav = false;
    public Vector3 PlanetCentre;

    [Tooltip("ye")]
    public float CamYOffset = 0.125f;

    private AudioSource walkSound;
    private float nextSound;

    [Tooltip("Changes TimeScale based off of movement speed - faster movement = slower timeScale")]
    public bool scaleTime = false;

    private Vector3 moveDirection = Vector3.zero;

    private bool isCrouching = false;
    private bool isInLight = false;
    public static bool isAiming;

    private float crouchHeight;

    private float detection;
    public bool beingDetected;

    public GameObject pCam;

    private Vector3 camSmoothDampRef;
    private float heightSmoothDampRef;
    private Vector3 centerSmoothDampRef;

    private bool grounded = true;

    void Start()
    {
        characterRB = GetComponent<Rigidbody>();
        walkSound = GetComponent<AudioSource>();
        stanSpeed = speed;

        if(healthBar != null)
            healthBar.fillAmount = (health/100);

        detection = 0;
        if(detectBar != null)
            detectBar.fillAmount = (detection / 100);
        
        AudioListener.pause = false;
        

        isAiming = false;

        if(pCam == null)
            pCam = gameObject.GetComponentInChildren<Camera>().transform.parent.gameObject;
        

    }

    void Update()
    {
        

        if (healthBar != null)
            healthBar.fillAmount = (health / 100);

        if (useDetection && detectBar!=null)
        {
            detectBar.fillAmount = (detection / 100);
            float bVal = 255f - (detection * 2.55f);
            detectBar.color = Color.Lerp(Color.blue, Color.red, (detection / 100));
        }

        if (true)
        {
            if (Input.GetAxis("Horizontal") != 0)
                moving = true;
            else if (Input.GetAxis("Vertical") != 0)
                moving = true;
            else
                moving = false;

            //Check if the player is sprinting, crouching, or walking normally to determine speed
            if (Input.GetAxisRaw("Fire3") > 0 && !isCrouching && !isAiming)
                speed = sprintSpeed;
            else if (isCrouching || isAiming)
                speed = stanSpeed / 2;
            else
                speed = stanSpeed;
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = (transform.right*Input.GetAxis("Horizontal"));
            moveDirection += (transform.forward * Input.GetAxis("Vertical"));
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                //moveDirection.y = jumpSpeed;
                characterRB.AddForce(transform.up * jumpSpeed, ForceMode.Impulse);
            }
            if (moving && Time.time >= nextSound)
            {
                nextSound = Time.time + 1.0f / (speed/2);
                walkSound.Play();
            }
        }

        if (Input.GetButtonDown("Crouch"))
            isCrouching = !isCrouching;

        //If the player is not crouching
        if (isCrouching)
            Crouch();
            //else the player is crouching
        else
            StopCrouching();

            
        

        if (scaleTime)
        {
            float fa = 0f;
            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.z))
                fa = Mathf.Abs(moveDirection.x);
            else
                fa = Mathf.Abs(moveDirection.z);

            if (fa > speed)
                fa = speed;


            Time.timeScale = 1.0f - (0.5f * fa / speed);
        }


        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)

        if (!useSphereGrav)
        {
            characterRB.useGravity = true;
            //Normal Gravity
            //moveDirection.y -= gravity * Time.deltaTime;
            // Move the controller
            //characterRB.AddForce(moveDirection);
            characterRB.velocity = new Vector3(moveDirection.x, characterRB.velocity.y, moveDirection.z);
        }
        else
        {
            //SphereicalGravity
            characterRB.useGravity = false;
            moveDirection += ((PlanetCentre - transform.position).normalized * gravity * Time.deltaTime); //Accelerates towards center - gravity
            //moveDirection.y -= 0;
            transform.rotation = Quaternion.LookRotation(PlanetCentre - transform.position, transform.up); //causes rotation so the object is upright

            //characterRB.AddForce(moveDirection);
            characterRB.velocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);
        }
        

        if (health <= 0)
        {
            //Debug.Log("Player is dead");
            canvas.GetComponent<CanvasScript>().FailMenu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            AudioListener.pause = true;
        }
    }

    public bool getCrouch()
    {
        return isCrouching;
    }

    public bool GetInLight()
    {
        return isInLight;
    }

    public void SetInLight(bool l)
    {
        isInLight = l;
    }

    private void Crouch()
    {
        //characterController.height -= crouchHeight;
        //characterController.center -= new Vector3(0, crouchHeight / 2, 0);


        //
        //characterController.height = Mathf.SmoothDamp(characterController.height, crouchHeight, ref heightSmoothDampRef, crouchSpeed);
        //characterController.center = Vector3.SmoothDamp(characterController.center, new Vector3(0, crouchHeight/2,0), ref centerSmoothDampRef, crouchSpeed);

        //pCam.transform.localPosition = Vector3.SmoothDamp(pCam.transform.localPosition, new Vector3(0, crouchHeight-CamYOffset, 0), ref camSmoothDampRef, crouchSpeed);
        //


        //gameObject.GetComponentInChildren<Camera>().transform.position -= new Vector3(0, crouchHeight, 0);
    }

    private void StopCrouching()
    {
        //gameObject.GetComponent<CharacterController>().height += crouchHeight;
        //gameObject.GetComponent<CharacterController>().center += new Vector3(0, crouchHeight / 2, 0);

        //gameObject.GetComponentInChildren<Camera>().transform.position += new Vector3(0, crouchHeight, 0);


        //
        //characterController.height = Mathf.SmoothDamp(characterController.height, crouchHeight*2, ref heightSmoothDampRef, crouchSpeed);
        //characterController.center = Vector3.SmoothDamp(characterController.center, new Vector3(0, crouchHeight, 0), ref centerSmoothDampRef, crouchSpeed);

        //pCam.transform.localPosition = Vector3.SmoothDamp(pCam.transform.localPosition, new Vector3(0, crouchHeight*2-CamYOffset, 0), ref camSmoothDampRef, crouchSpeed);
    }

    public void SetDetect(float d)
    {
            detection = d;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            isInLight = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            isInLight = false;
        }
    }


    public void SetHealth(float h)
    {
        health = h;
    }

}
