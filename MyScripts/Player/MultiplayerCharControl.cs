using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using UnityEngine.Rendering;
using Unity.Netcode;

public class MultiplayerCharControl : NetworkBehaviour
{
    public static CharacterController characterController;

    public static bool Active;

    public static MultiplayerCharControl instance;
    public Transform playerModel;
    public bool allowRespawn = false;
    public float speed = 6.0f;
    public float sprintSpeed = 9.0f;
    [Tooltip("Not Used?")]
    public float crouchSpeed = 0.5f;
    //[Tooltip("Used to modify the base speed values during events - e.g. mud slowing player")]
    protected float speedModifier = 1.0f;
    protected float stanSpeed;
    public float jumpSpeed = 8.0f;
    [Tooltip("After leaving ground, have a sight leeway to allow last second jumping")]
    public float jumpLeeway = 0.5f;
    protected float leeWayTimer = 0;
    public float gravity = 20.0f;
    public bool moving = false;
    public float health = 100;
    public GameObject canvas;
    public Image healthBar;
    public Image detectBar;
    public bool useDetection = false;

    [Tooltip("ye")]
    public float CamYOffset = 0.125f;

    [Tooltip("Changes TimeScale based off of movement speed - faster movement = slower timeScale")]
    public bool scaleTime = false;

    protected Vector3 moveDirection = Vector3.zero;

    public static bool isCrouching = false;
    protected bool isInLight = false;
    public static bool isAiming;
    public static bool isSprinting;

    protected float crouchHeight;

    protected float detection;
    public bool beingDetected;
    //protected float moveSpeed;

    public GameObject pCam;
    //protected Vector3 pCamHomePos;

    protected Vector3 camSmoothDampRef;
    protected float heightSmoothDampRef;
    protected Vector3 centerSmoothDampRef;

    [Header("Other Shit")]
    public AudioSource walkSound;
    protected float walkSoundTimer;
    protected MaterialEffects.Sound walkSoundType;

    protected Vector3 healthBarScale;
    protected float prevHealth;

    CameraShakeInstance curShake;

    //used to properly calculate fill values when using max health values other than 100
    protected float startHealth;

    [HideInInspector]
    public bool canWallRun;
    [Space]
    public bool allowAirMovement = false;

    protected Vector3 hitNormal = Vector3.up; //slope hit point normal
    protected bool sliding;

    protected bool prevGrounded;
    [Space]
    public Volume healthPostProcess;

    public Transform heldObjectPosition;

    protected bool godMode;

    protected bool tribesMode;
    [HideInInspector]
    public float hipFov;//make this read from a stored variable so it can be changed by player

    [HideInInspector]
    public bool ignoreGravity;

    protected Vector3 realPlayerSpeed;
    protected Vector3 prevPos;

    protected bool started;

    protected virtual void Start()
    {
        if (!IsOwner)
        {
            //this isnt the current Player
            enabled = false;
            return;
            //disable it
        }

        Active = true;
        instance = this;
        characterController = GetComponent<CharacterController>();
        stanSpeed = speed;

        startHealth = health;

        if (healthBar != null)
            healthBar.fillAmount = (health / startHealth);

        detection = 0;
        //if (useDetection)
        //{
        if (detectBar != null)
            detectBar.fillAmount = (detection / 100);
        //}

        //smg.SetActive(true);
        //shotgun.SetActive(false);
        //grenadeLauncher.SetActive(false);
        AudioListener.pause = false;

        crouchHeight = characterController.height / 2;
        //moveSpeed = speed;

        isAiming = false;

        if (pCam == null)
        {
            pCam = gameObject.GetComponentInChildren<Camera>().transform.parent.gameObject;
        }
        pCam.SetActive(true);// ## I disable the camera by default, only enabling it when needed to prevent weirdness with camera shaker

        if (healthBar != null)
            healthBarScale = healthBar.transform.localScale;
        prevHealth = health;

        hipFov = pCam.GetComponentInChildren<Camera>().fieldOfView;
        //pCamHomePos = pCam.transform.localPosition;
        //Application.targetFrameRate = 90;

        prevPos = transform.position;

        started = true;
    }

    void Update()
    {
        if (!IsOwner || !started)
            return;

        if (godMode)
            health = startHealth;
        /*
        if (Input.GetKeyDown(KeyCode.V))
            Debug.Log(TESTCARRYOVER.yeet + " yo");
        if (Input.GetKeyDown(KeyCode.B))
            TESTCARRYOVER.yeet++;
        */
        //Debug.Log(detection + " Detect");
        if (healthBar!=null && health != prevHealth) //effects when taking damage
        {
            healthBar.transform.localScale = healthBarScale * 1.5f; //health bar pulse
            if (health > 0)
                CameraShaker.Instance.ShakeOnce(prevHealth / health, 2f, 0.1f, 0.1f);//camera shake - scales magnitude by damage
        }
        if (healthBar != null)
            healthBar.transform.localScale = Vector3.MoveTowards(healthBar.transform.localScale, healthBarScale, 0.1f);

        if (healthPostProcess != null) //once below 70% health, start lerping the healthPostProcess
        {
            if (health < startHealth * 0.7f)
                healthPostProcess.weight = 1f - (health / (startHealth * 0.7f));
            else
                healthPostProcess.weight = 0f;
        }




        if (healthBar != null)
        {
            healthBar.fillAmount = (health / startHealth);
            byte colourShift = (byte)(healthBar.fillAmount * 255);     //Colour shift amount 
            healthBar.color = new Color32(255, colourShift, colourShift, 225);		//As sanity drops, colour shifts more to red
        }

        if (useDetection && detectBar != null)
        {
            detectBar.fillAmount = (detection / 100);
            float bVal = 255f - (detection * 2.55f);
            //Debug.Log(bVal + " " + detection);
            //detectBar.color = new Color((detection*2.55f),0f, bVal);
            detectBar.color = Color.Lerp(Color.blue, Color.red, (detection / 100));
        }

        if (characterController.isGrounded) //Normal move
        {
            MovePlayer(1, true);
            leeWayTimer = 0;
        }
        else if (jumpLeeway > 0 && leeWayTimer < jumpLeeway) //if within leeway
        {
            MovePlayer(1, true); //allow movement/jumping
            leeWayTimer += Time.deltaTime;
        }
        else if (!allowAirMovement)//Normal Jumping
            isSprinting = false;

        if (canWallRun && !characterController.isGrounded)
            MovePlayer(0.9f * (sprintSpeed / stanSpeed), false); //sprint/stan gives the multiplier that would result in sprint speed, then I make it 90% of that 

        else if (!characterController.isGrounded && allowAirMovement) //non wall running air movement
            AirMove();

        //Sprint SHake
        if (isSprinting && curShake == null && CameraShaker.Instance != null)
        {
            //camshake/headbob
            curShake = CameraShaker.Instance.StartShake(0.2f, 1.5f, 0.2f);
        }
        else if (!isSprinting && curShake != null)
        {
            curShake.StartFadeOut(0.2f);
            curShake = null;
        }
        if (Active)
            if (Input.GetButtonDown("Crouch"))
                isCrouching = !isCrouching;
        //Crouch Code
        //Debug.Log("Crouch");

        //If the player is not crouching
        if (isCrouching)
            Crouch();
        //else the player is crouching
        else
            StopCrouching();




        if (scaleTime)
        {
            float fa = 0f;
            if (/*Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y) && */Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.z))
                fa = Mathf.Abs(moveDirection.x);
            //else if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.z))
            //fa = Mathf.Abs(moveDirection.y);
            else
                fa = Mathf.Abs(moveDirection.z);

            if (fa > speed)
                fa = speed;


            Time.timeScale = 1.0f - (0.5f * fa / speed);
        }


        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)


        //Normal Gravity
        if (!ignoreGravity)
        {
            if (canWallRun && moving && Time.timeScale > 0)
                moveDirection.y -= gravity * Time.deltaTime * 0.1f; //wallrun gravity - only when moving 
            else if (!characterController.isGrounded && Time.timeScale > 0)//!canWallRun || !moving || sliding
                moveDirection.y -= gravity * Time.deltaTime; //normal
        }



        /* The reson the gravity system has been changed alot is due to air movement - prevously, when grounded the player movement would reset the gravity force, 
         * and when not grounded gravity could apply normally, the result was gravity was only applied while in the air, and was reset when grounded. This meant it never compounded unrealistically.
         * However, once i added in airmovement I couldnt be constantly resetting it in the move function as that would make it so gravity was really weak as it was basically only ever doing the first frame of acceleration.
         * Now gravity is only applied in the air (like before) but when the player initially is re-grounded, it is reset to prevent compounding too much. The movement vector is no longer reset by the movement function.
         * 
         * I also removed spherical gravity coz it doesnt work with charcontroller
         * 
         */


        // Move the controller
        if (Active)
            characterController.Move(moveDirection * Time.deltaTime);


        if (health <= 0)
        {
            //Debug.Log("Player is dead");
            Death();

        }

        prevHealth = health;

        //Debug.Log("MoveDir: " + moveDirection*Time.deltaTime + " Grounded: " + characterController.isGrounded + " wallrun " + canWallRun);
        //Debug.Log("Move: " + moveDirection + " prevGround " + prevGrounded +" Grounded " + characterController.isGrounded);

        realPlayerSpeed = (transform.position - prevPos) / Time.deltaTime;

        prevPos = transform.position;

        hipFov = PlayerPrefs.GetInt("FieldOfView", 70);
        // ########################################################################## MOVE MOST OF THIS INTO SEPARATE FUNCTIONS TO CLEAN UP UPDATE #############################################################################
    }



    protected void FixedUpdate()
    {
        if (!IsOwner || !started)
            return;

        /* OLD SOUND CODE
        if (characterController.isGrounded)
            UpdateSound();
        //Walking sound stuff - Maybe should make this based off of distance traveled - this way no noice is made when running into walls and should scale nicley no matter the speed
        if(moving && characterController.isGrounded && walkSoundTimer <= Time.time)
        {
            //if (!isSprinting && !isCrouching)
                walkSoundTimer = Time.time + (2/speed);//was 0.45 standard, 0.65 crouch, 0.25 sprint //was +0.95f
                                                             // else if (isCrouching)
                                                             //    walkSoundTimer = Time.time + 0.65f;
                                                             // else
                                                             //    walkSoundTimer = Time.time + 0.25f; //shorter interval when sprinting
            if (walkSoundType != MaterialEffects.Sound.none && SoundManager.instance!=null)
            {
                walkSound.clip = SoundManager.instance.GetSound(walkSoundType, !isSprinting);

                walkSound.PlayOneShot(walkSound.clip, walkSound.volume);
            }
            //Play sound

        }
        */
        if (!prevGrounded && characterController.isGrounded) //This seems to be more reliable in fixed update, prehaps due to more consistent and slower updating 
        { //if wasnt previously grounded but now are, then reset gravity movement to prevent it compounding too much
            //Debug.Log("YEET " + moveDirection.y);
            moveDirection.y = 0f;
            //Debug.Log("YOTE " + moveDirection.y);
        }
        prevGrounded = characterController.isGrounded;
    }


    protected void UpdateSound() //No longer Used
    {
        RaycastHit rayHit;
        //Debug.DrawRay(pCam.transform.position, pCam.transform.forward*10, Color.white, 5.0f, false);
        if (Physics.Raycast(transform.position, -transform.up, out rayHit, 2f))
        {

            if (rayHit.transform.gameObject.GetComponent<MaterialEffects>())
                walkSoundType = rayHit.transform.gameObject.GetComponent<MaterialEffects>().typeOfMaterial;
        }

    }


    protected void Death()
    {
        if (allowRespawn)
        {
            PlayerRespawnManager.instance.RespawnPlayer();
        }
        else
        {
            if(CanvasScript.instance!=null)
                CanvasScript.instance.Lose();
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

    protected void Crouch()
    {
        //characterController.height -= crouchHeight;
        //characterController.center -= new Vector3(0, crouchHeight / 2, 0);
        //if (characterController.height - crouchHeight >0.1f)
        //{
        characterController.height = Mathf.SmoothDamp(characterController.height, crouchHeight, ref heightSmoothDampRef, crouchSpeed);
        characterController.center = Vector3.SmoothDamp(characterController.center, new Vector3(0, crouchHeight / 2, 0), ref centerSmoothDampRef, crouchSpeed);

        //if(Active)
        pCam.transform.localPosition = Vector3.SmoothDamp(pCam.transform.localPosition, new Vector3(0, crouchHeight - CamYOffset, 0), ref camSmoothDampRef, crouchSpeed);

        playerModel.localPosition = characterController.center;
        //}
        //gameObject.GetComponentInChildren<Camera>().transform.position -= new Vector3(0, crouchHeight, 0);
    }

    protected void StopCrouching()
    {
        //gameObject.GetComponent<CharacterController>().height += crouchHeight;
        //gameObject.GetComponent<CharacterController>().center += new Vector3(0, crouchHeight / 2, 0);

        //gameObject.GetComponentInChildren<Camera>().transform.position += new Vector3(0, crouchHeight, 0);

        //if (characterController.height - crouchHeight*2 < 0.1f || characterController.height - crouchHeight * 2 > 0.1f)
        //{
        characterController.height = Mathf.SmoothDamp(characterController.height, crouchHeight * 2, ref heightSmoothDampRef, crouchSpeed);
        characterController.center = Vector3.SmoothDamp(characterController.center, new Vector3(0, crouchHeight, 0), ref centerSmoothDampRef, crouchSpeed);

        //if(Active)
        pCam.transform.localPosition = Vector3.SmoothDamp(pCam.transform.localPosition, new Vector3(0, crouchHeight * 2 - CamYOffset, 0), ref camSmoothDampRef, crouchSpeed);

        playerModel.localPosition = characterController.center;
        //}
    }


    protected void MovePlayer(float modifier, bool canJump)
    {
        if (Input.GetAxis("Horizontal") != 0)
            moving = true;
        else if (Input.GetAxis("Vertical") != 0)
            moving = true;
        else
            moving = false;

        //Check if the player is sprinting, crouching, or walking normally to determine speed
        if (Input.GetAxisRaw("Fire3") > 0 && !isCrouching && !isAiming && moving) //Sprint
        {
            speed = sprintSpeed * speedModifier * modifier;
            isSprinting = !canWallRun; //replaced true with can wall run so that when wal running it isnt counted as sprinting
        }
        else if (isCrouching || isAiming)
        {
            speed = (stanSpeed / 2) * speedModifier * modifier;
            isSprinting = false;
        }
        else
        {
            speed = stanSpeed * speedModifier * modifier;
            isSprinting = false;
        }
        // We are grounded, so recalculate
        // move direction directly from axes
        //Reset x and z first, then add the movement by axis - this is better than before as it doesnt mess with gravity
        if (!tribesMode)
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
        }
        moveDirection += transform.right * Input.GetAxis("Horizontal") * speed;
        moveDirection += transform.forward * Input.GetAxis("Vertical") * speed;
        //moveDirection *= speed;

        if ((Vector3.Angle(Vector3.up, hitNormal) > characterController.slopeLimit) && !canWallRun && false) //slide down slope //DISABLED
        {
            sliding = true;
            float slideFriction = 0.3f;
            moveDirection.x = (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction);
            moveDirection.z = (1f - hitNormal.y) * hitNormal.z * (1f - slideFriction);
        }
        else
            sliding = false;

        if (Input.GetButton("Jump") && canJump && !ignoreGravity) //IS IGNORE GRAVITY NEEDED HERE? #################################################################
        {
            moveDirection.y = jumpSpeed;
            leeWayTimer = jumpLeeway; //max out leeway timer to prevent double jumping
            //WallRun.instance.jumping = true;
            //StartCoroutine(JumpSet());
        }
        else if (Input.GetButtonDown("Jump") && canWallRun)
        {
            StartCoroutine(WallJump());
            StartCoroutine(JumpSet());
        }
    }

    protected void AirMove()
    {
        if (Input.GetAxis("Horizontal") != 0)
            moving = true;
        else if (Input.GetAxis("Vertical") != 0)
            moving = true;
        else
            moving = false;

        if (moving && !canWallRun) //only if moving? this way it keeps moving normall when no input, but if i do it even when no moving it would lerp towards 0, which could be good idk needs testing
        {
            Vector3 newMove = Vector3.zero;
            //detect input
            newMove += transform.right * Input.GetAxis("Horizontal") * speed;
            newMove += transform.forward * Input.GetAxis("Vertical") * speed;
            newMove.y = moveDirection.y; //use existing gravity

            //change movedirection towards new input - dont just instantly change, this way movement is smoothed while in the air
            moveDirection = Vector3.MoveTowards(moveDirection, newMove, 10f * Time.deltaTime);
        }

        /*
        //Lerp x and z movement to 0
        moveDirection.x -= Time.deltaTime;
        moveDirection.z -= Time.deltaTime;
        //add slow x and z movement - similar to normal movement but scaled with deltatime not speed
        moveDirection += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * 4f;
        moveDirection += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 4f;
        */
    }

    public void SetDetect(float d)
    {
        //if (d > detection)
        detection = d;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            isInLight = true;
        }
    }

    protected void OnTriggerExit(Collider other)
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

    public void SetSpeedModifier(float sm)
    {
        speedModifier = sm;
    }

    public void ResetSpeedModifier()
    {
        speedModifier = 1f;
    }

    IEnumerator JumpSet() //keep jumping true for .1 second while jumping to help with wall jumping
    {
        float count = 0;
        while (count < 0.05f)
        {
            WallRun.instance.jumping = true;
            count += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator WallJump()
    {
        Vector3 moveVec = WallRun.instance.GetWallJumpDirection();
        moveVec += moveDirection / 3f; //Add wall jump vector to curret move vector
        float moveForce = 1f;
        for (float count = 0; count < 0.4f; count += Time.deltaTime)
        {
            moveDirection = (moveForce * jumpSpeed * 0.5f) * moveVec;
            moveForce -= Time.deltaTime;
            yield return null;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    public void GodeModeToggle()
    {
        godMode = !godMode;
    }

    public void FillHealth()
    {
        health = startHealth;
    }

    public Vector3 GetMoveDir()
    {
        return moveDirection;
    }

    public void AddMove(Vector3 vec)
    {
        moveDirection += vec;
    }

    protected void OnDrawGizmos()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.TransformPoint(characterController.center), 0.15f);
        }
    }

    public void LockMovement(bool b) //if b then camera move is also locked and mouse is unlocked
    {
        Active = false;
        if (b)
        {
            CameraMove.instance.Active = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            WeaponSelection.instance.SetPlacing(true);
        }
    }

    public void UnlockMovement()
    {
        Active = true;
        CameraMove.instance.Active = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        WeaponSelection.instance.SetPlacing(false);
    }


    public Vector3 GetRealPlayerSpeed()
    {
        return realPlayerSpeed;
    }

    public void DoDamage(float f)
    {
        health -= f;
    }

    public void SetMove(Vector3 dir)
    {
        moveDirection = dir;
    }
}
