using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using EZCameraShake;
using UnityEngine.Events;

public class EmplacedGunScript : MonoBehaviour
{ //note: this script assume that the raycast bullet is already set up, so no damage and range settings need to be set in here
  //public Transform hipPos;
  //public Transform adsPos;
  //public float hipFov = 90f;
    public enum EmplacedAimType {normal, camWReticle, camWOReticle };
    public float AimFov = 40f;
    [Tooltip("Used for alternate aim method - useCamAim")]
    public GameObject aimCam;
    private Camera aimCameraComp;
    public EmplacedAimType AimType;
    private float camref;
    private float speedUp = 2f;
    private float speedAcross = 2f;
    public GameObject pitchObject;
    [Tooltip("If yaw object is not set, then the object this script is attached to is used as yaw object")]
    public Transform yawObject;
    public GameObject recoilObject;
    public Transform reticule;
    [Space]
    public Transform playerPosition;
    public Vector2 minMaxYaw;
    public Vector2 minMaxPitch;
    [Space]
    public ParticleSystem caseEffect;
    [Space]
    [Tooltip("Optional")]
    public Transform lookPos;
    [Header("Interaction")]
    public float interactDistance = 5;
    public LayerMask raycastIgnore;
    private bool active;
    [Header("GunStats")]
    public float fireRate = 10f;
    private float fireRatePerSecond;
    public int numOfShots = 1;
    public float spreadModifier = 1;
    public bool isSemiAuto = false;
    public bool singleShot = false;
    public bool usesAim = false;
    public bool usesShortReload = false;
    [Space]
    public Transform BulletSpawnPoint;
    [Header("Ammo")]
    public GameObject bullet;
    public int AmmoPool = 20;
    public bool infiniteAmmo;
    [HideInInspector]
    public int MaxAmmo = 10;
    public int MagSize = 10;
    [HideInInspector]
    public int CurMag;
    [Header("Reloading")]
    public GameObject visualMag;
    public float reloadTime = 1f;
    public AudioSource reloadSound;

    [Space]
    public float shakeMagnitude = 0.5f;
    public float shakeRoughness = 0.5f;
    public Vector2 shakeFadeInOut = new Vector2(0.1f,0.1f);
    [Header("Recoil")]
    public Vector3 recoilVector = Vector3.one;
    public float recoilRotForce = 10f;
    public float recoilResetTime = 0.1f;
    [Tooltip("Speed at which the gun lerps recoil rotation back to 0 -> different to recoilrest which is for linear movement")]
    public float recoilRotReset = 0.1f;
    public float adsSpeed = 0.1f;
    [HideInInspector]
    public Vector3 recoilResetForce;


    private float nextFire = 0f;
    private float spread = 0.025f;
    private VisualEffect flash;
    private AudioSource bang;



    private float recoilResetRotRef;


    [Header("Bolt Properties")]
    public GameObject boltObject;
    public Vector3 boltMovementVector = Vector3.one;
    public float boltRecoilForce = 0.2f;
    public float boltResetTime = 0.2f;
    private Vector3 boltResetVector;
    private Vector3 boltHomePos;
    public bool lockOpen = false;
    //private bool wasLockedOpen = false;
    [Header("To be used bolt properties")]
    public bool isOpenBolt = false;
    public Transform restPos; //position of the bolt when ready to fire
    public Transform firedPos; //position of the bolt when fired
    private bool useBoltPos = false;

    private bool started = false;

    private bool ads = false;


    //used to smoothly rotate the gun when turning (just using yawSpeed directly gives jittery movement)
    private float yRotAmount;
    private float yRotVel;
    private float xRotAmount;
    private float xRotVel;
    //Used in recoil
    private float zRotAmount;
    private float xRecoilRotAmount; //extra x rotation componet for recoil

    //values used to reset gun after 'animating' 
    private Vector3 startPos;
    private Quaternion startRot;
    private float startYRot;

    private float lastShotTime = 0;

    private Vector3 recoilAmountRemaining; //used to smooth out recoil - rather than apply all at once, some is applied instantly with the rest applied smoothly over time

    private Vector2 rotationResetDecay; //used to reset camera to original pos after shooting
    // Start is called before the first frame update
    [Tooltip("If the gun uses SmartAi based shot detection")]
    public bool UsesShotDetection = false;
    public float detectRange;

    [HideInInspector]
    public bool reloading = false;
    [HideInInspector]
    public bool done = false; //triggers end of reload
    [HideInInspector]
    public bool cycling = false; //used to signal when gun is cycling action

    public Camera pCam;

    private float pitch;
    private float yaw;


    [System.Serializable]
    public struct ExtraBarrel
    {
        public Transform barrel;
        public Transform bulletSpawn;
        [HideInInspector]
        public Vector3 barrelStartPos;
        public VisualEffect muzzleFlash;
        public AudioSource shotSound;
    }


    [Header("Barrel Recoil")]
    public Transform barrel;
    private Vector3 barrelStartPos;
    public float barrelRecoil;
    public float barrelResetSpeed;

    [Header("Special Properties")]
    public GameObject expFlash;
    public float expFlashTime = 0.02f;
    public VisualEffect[] multiEffects;
    public bool parentPlayer;
    //public bool unAimOnReload = true;
    public int barrelCount = 1;
    public ExtraBarrel[] multiBarrels;
    private int curBarrel = 0;


    [Header("Events")]
    public UnityEvent enterEvent;
    public UnityEvent leaveEvent;


    private Vector3 reticuleWorldPos;
    [HideInInspector]
    public bool blockUse; // blocks the player from using gun - like when bot is using it
    
    void Start()
    {
        active = false;
        if (barrelCount == 1)
        {
            flash = transform.GetComponentInChildren<VisualEffect>();
            bang = transform.GetComponent<AudioSource>();
        }

        CurMag = MagSize;

        MaxAmmo = AmmoPool;

        started = true;

        if (boltObject != null)
        if (boltObject != null)
            boltHomePos = boltObject.transform.localPosition;

        if (yawObject == null)
            yawObject = transform;

        startPos = recoilObject.transform.localPosition;
        startRot = yawObject.localRotation;

        startYRot = yawObject.localEulerAngles.y;
        yRotAmount = 0;

        xRotAmount = 0;

        if (restPos != null && firedPos != null)
            useBoltPos = true;

        if (barrel != null)
            barrelStartPos = barrel.localPosition;


        if (barrelCount > 1)
        {
            for (int i = 0; i < multiBarrels.Length; i++)
                multiBarrels[i].barrelStartPos = multiBarrels[i].barrel.localPosition;
        }

        fireRatePerSecond = fireRate / 60f;


        if (aimCam != null)
            aimCameraComp = aimCam.GetComponent<Camera>();
    }


    // Update is called once per frame
    void Update()
    {
        if (infiniteAmmo)
            AmmoPool = MagSize * 2;


        if (pCam == null && CharacterControllerScript.instance!=null)
            pCam = CharacterControllerScript.instance.pCam.GetComponentInChildren<Camera>();
        if(pCam!=null)
        {
            CheckSight();

            UpdateRot();

            UpdateReticule();

            if (active)
                StateUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (active)
            if (CharacterControllerScript.instance.health <= 0)
                Leave();
        

        if (pCam == null && CharacterControllerScript.instance != null)
            pCam = CharacterControllerScript.instance.pCam.GetComponentInChildren<Camera>();

        //ADS
        if (pCam != null)
        {
            if (active)
            {
                if (ads && Time.timeScale > 0) //ads lerp
                {
                    //CharacterControllerScript.instance.pCam.transform.position = Vector3.SmoothDamp(CharacterControllerScript.instance.pCam.transform.position, adsPos.position, ref recoilResetForce, adsSpeed);

                    if(usesAim) //normal aim
                        pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, AimFov, ref camref, 0.25f);

                    if (AimType != EmplacedAimType.normal) //camaim -> improvement -> make it start from player cam position then lerp to aim pos to make transition smooth
                    {
                        if (pCam.enabled)
                            pCam.enabled = false;
                        if (!aimCam.activeSelf)
                            aimCam.SetActive(true);
                    }
                }
                else if ((!ads || reloading) && Time.timeScale > 0) //Camera Reset
                {
                    //CharacterControllerScript.instance.pCam.transform.position = Vector3.SmoothDamp(CharacterControllerScript.instance.pCam.transform.position, hipPos.position, ref recoilResetForce, adsSpeed);

                    pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, CharacterControllerScript.instance.hipFov, ref camref, 0.25f);

                    if (AimType != EmplacedAimType.normal) //camaim disable
                    {
                        if (!pCam.enabled)
                            pCam.enabled = true;
                        if (aimCam.activeSelf)
                            aimCam.SetActive(false);
                    }
                }
            }

            //RecoilReset
            if (Time.timeScale > 0)
            {
                recoilObject.transform.localPosition = Vector3.SmoothDamp(recoilObject.transform.localPosition, startPos, ref recoilResetForce, recoilResetTime);
                if (barrel != null && barrelCount == 1)
                    barrel.localPosition = Vector3.MoveTowards(barrel.localPosition, barrelStartPos, barrelResetSpeed);
                else
                    ResetMultiBarrels();
            }


            //Bolt Resettings
            if (firedPos != null && lockOpen && CurMag <= 0 && Time.timeScale > 0)
                boltObject.transform.localPosition = firedPos.localPosition;
            else if (boltObject != null && !useBoltPos && Time.timeScale > 0)
            {
                boltObject.transform.localPosition = Vector3.Lerp(boltObject.transform.localPosition, boltHomePos, boltResetTime);
            }
            else if (useBoltPos && Time.timeScale > 0)
                boltObject.transform.localPosition = Vector3.Lerp(boltObject.transform.localPosition, restPos.localPosition, boltResetTime);
            //Reset bolt - note: uses lerp as this is consistent and makes more sense for mechanical movement

            xRecoilRotAmount = Mathf.SmoothDamp(xRecoilRotAmount, 0, ref recoilResetRotRef, recoilRotReset); //i jsut change xrotamount, dont really need xrecoilrotamount anymore

            //applying weapon sway
            // if (!ads && !reloading)
            //     transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(new Vector3(xRotAmount + xRecoilRotAmount, startYRot + yRotAmount, zRotAmount)), 1f * 2f); //MAKE THIS NOT HARD CODED
            // else if (!reloading)//when aiming do it slower
            //     transform.localEulerAngles = new Vector3(adsPos.localEulerAngles.x + xRecoilRotAmount + xRotAmount / 2, adsPos.localEulerAngles.y + yRotAmount / 2, zRotAmount);

            //camera pos reset - slowly reset to original aiming position
            if (rotationResetDecay.x > 0.05f || rotationResetDecay.x < -0.05f)
            {
                rotationResetDecay.x -= rotationResetDecay.x * Time.fixedDeltaTime * 2f;
                rotationResetDecay.y -= rotationResetDecay.y * Time.fixedDeltaTime * 2f;
            }
        }
    }

    private void StateUpdate() //basically the update function - separated to this to clean up update
    {
        CharacterControllerScript.isAiming = ads;

        if (bang != null)
            bang.pitch = Time.timeScale;
        if (CurMag <= 0 && Input.GetButtonDown("Fire1") && Time.timeScale > 0)
        {
            WeaponSelection.ClickSound.PlayOneShot(WeaponSelection.ClickSound.clip, 0.75f);

        }


        if (Input.GetButtonDown("Fire2") && (usesAim) && Time.timeScale > 0 && !reloading)
        {
            ads = !ads;

        }


        CanvasScript.reticle.SetActive(!(ads && AimType == EmplacedAimType.camWOReticle)); //update whether the reticle should be active - only inactive when ads and cam aim both true

        ShootUpdate();


        //if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && singleReload && reloading)
        //  EndReload(); //end reload early when clicking or sprinting during single loading

        if (Input.GetKeyDown("r") && !reloading && AmmoPool > 0 && MagSize != CurMag && Time.timeScale > 0)
        {
            StartCoroutine(Reload());
        }

        if (CurMag == 0 && !reloading && AmmoPool > 0 && Time.timeScale > 0) //auto reload on empty
            StartCoroutine(Reload());

    }

    private void ShootUpdate()
    {
        //single shot fire - e.g. bolt action/lever action/pump action
        if (singleShot)
        {
            if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && CurMag > 0 && !reloading && !cycling)
            {
                cycling = true;
                RayCastShoot();
                if (multiEffects.Length > 0)
                    for (int i = 0; i<multiEffects.Length; i++)
                        multiEffects[i].Play();
                else if (flash != null && barrelCount == 1) //do default flash if not using multiflash
                    flash.Play();
                if (expFlash != null && barrelCount == 1)
                    StartCoroutine(MuzzleFlash());
                if (bang != null && barrelCount == 1)
                    bang.PlayOneShot(bang.clip, bang.volume);


                CurMag--;
            }
        }
        else if (isSemiAuto)
        {
            //SemiAuto fire
            if (Input.GetButtonDown("Fire1") && Time.time >= nextFire && Time.timeScale > 0 && CurMag > 0 && !reloading)
            {
                nextFire = Time.time + 1.0f / fireRatePerSecond;
                RayCastShoot();
                if (multiEffects.Length > 0)
                    for (int i = 0; i<multiEffects.Length; i++)
                        multiEffects[i].Play();
                else if (flash != null && barrelCount == 1)
                    flash.Play();
                if (expFlash != null && barrelCount == 1)
                    StartCoroutine(MuzzleFlash());
                if (bang != null && barrelCount == 1)
                    bang.PlayOneShot(bang.clip, bang.volume);

                CurMag--;
            }
        }
        else
        {
            //Fullauto fire
            if (Input.GetButton("Fire1") && Time.time >= nextFire && Time.timeScale > 0 && CurMag > 0 && !reloading)
            {
                nextFire = Time.time + 1.0f / fireRatePerSecond;
                RayCastShoot();
                if (multiEffects.Length > 0)
                    for (int i = 0; i<multiEffects.Length; i++)
                        multiEffects[i].Play();
                else if (flash != null && barrelCount == 1)
                    flash.Play();
                if (expFlash != null && barrelCount == 1)
                    StartCoroutine(MuzzleFlash());
                if (bang != null && barrelCount == 1)
                    bang.PlayOneShot(bang.clip, bang.volume);

                CurMag--;
            }
        }
    }

    private void RayCastShoot()
    {
        for (int i = 0; i < numOfShots; i++)
        {
            if (caseEffect != null)
                caseEffect.Play();

            if (UsesShotDetection)
                ShotDetection();

            //Generate Random spread
            float ranX;
            float ranY;
            
            ranX = Random.Range(-spread * spreadModifier, spread * spreadModifier);
            ranY = Random.Range(-spread * spreadModifier, spread * spreadModifier);


            if (barrelCount == 1) //single barrel behaviour
            {
                GameObject shot = Instantiate(bullet, BulletSpawnPoint.position, Quaternion.LookRotation(BulletSpawnPoint.forward, BulletSpawnPoint.up)*Quaternion.Euler(ranX,ranY,0f));
                //RaycastBullet rcB = shot.GetComponent<RaycastBullet>();
                //shot.transform.eulerAngles += new Vector3(ranX, ranY, 0f);
                // rcB.spread = 0f;
                //float randomX = Random.Range(-spreadModifier, +spreadModifier);
                //float randomY = Random.Range(-spreadModifier, +spreadModifier);
                //shot.transform.eulerAngles += new Vector3(randomX, randomY, 0f);
            }
            else
            { //multibarrel behaviour
                GameObject shot = Instantiate(bullet, multiBarrels[curBarrel].bulletSpawn.position, Quaternion.LookRotation(multiBarrels[curBarrel].bulletSpawn.forward, multiBarrels[curBarrel].bulletSpawn.up) * Quaternion.Euler(ranX, ranY, 0f));
                //RaycastBullet rcB = shot.GetComponent<RaycastBullet>();
                //shot.transform.eulerAngles += new Vector3(ranX, ranY, 0f);
                //rcB.spread = 0f;
                //float randomX = Random.Range(-spreadModifier, +spreadModifier);
                //float randomY = Random.Range(-spreadModifier, +spreadModifier);
                //shot.transform.eulerAngles += new Vector3(randomX, randomY, 0f);
            }

            //rcB.transform.eulerAngles = new Vector3(rcB.transform.eulerAngles.x + ranX, rcB.transform.eulerAngles.y + ranY, rcB.transform.eulerAngles.z);

        }
        if (barrelCount > 1)
        {
            if (multiBarrels[curBarrel].shotSound != null)
                multiBarrels[curBarrel].shotSound.Play();
            if (multiBarrels[curBarrel].muzzleFlash != null)
                multiBarrels[curBarrel].muzzleFlash.Play();
            multiBarrels[curBarrel].barrel.localPosition = new Vector3(multiBarrels[curBarrel].barrel.localPosition.x, multiBarrels[curBarrel].barrel.localPosition.y, multiBarrels[curBarrel].barrelStartPos.z - barrelRecoil);

            if (curBarrel + 1 < multiBarrels.Length)
                curBarrel++;
            else
                curBarrel = 0;
        }
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeInOut.x, shakeFadeInOut.y);

        float recoilModifier = 1f;
        //ApplyRecoil
        if (ads)
        {
            recoilModifier = 0.5f;
        }
        else
        {
            
        }

        /////////NEW RECOIL!!
        //float isXpos = (Random.Range(0, 2) * 2 - 1); //randomly picks positive or negative
        float isYpos = (Random.Range(0, 2) * 2 - 1); //randomly picks positive or negative
        recoilObject.transform.localPosition -= recoilVector *recoilModifier; //Recoil hip
        pitch += Random.Range(recoilRotForce / 2f, recoilRotForce);
        yaw += Random.Range(-0.5f*recoilRotForce, recoilRotForce/2f);

        if (barrel != null)
            barrel.localPosition = new Vector3(barrel.localPosition.x, barrel.localPosition.y,  barrelStartPos.z - barrelRecoil);
        ////////

        lastShotTime = Time.time;


        //bolt movement
        if (boltObject != null && !useBoltPos)
            boltObject.transform.localPosition -= boltMovementVector * boltRecoilForce;
        else if (useBoltPos)
            boltObject.transform.localPosition = firedPos.localPosition;

        
    }



    IEnumerator Reload()
    {
        reloading = true;
        done = false;
        //ads = false;
        //yield return new WaitForSeconds(reloadTime);
        visualMag.SetActive(false);
        if (reloadSound != null)
            reloadSound.Play();
        yield return new WaitForSeconds(reloadTime);
        visualMag.SetActive(true);
        done = true;
        if (done)
        {
            if (CurMag < MagSize)
            {
                if (AmmoPool - (MagSize - CurMag) >= 0) //if there is enough ammo left
                {
                    AmmoPool -= (MagSize - CurMag);
                    CurMag = MagSize;
                }
                else if (AmmoPool > 0)//Use the last of the ammo
                {
                    CurMag += AmmoPool;
                    AmmoPool = 0;
                }

            }

            reloading = false;
        }

    }

    public void ShotDetection()
    {
        Vector3 shotPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(shotPos, detectRange); //Cast out detection sphere
        float randomChance = 0f;
        bool firstResponse = false;
        int responders = 0;
        foreach (Collider hit in colliders)
        {
            //Rigidbody rbH = hit.GetComponent<Rigidbody>();
            randomChance = Random.Range(0f, 5f); //Dont alert everyone, randomly select some (though always one, thats why firstResponse is used)
            if (hit.gameObject.tag == "SmartEnemy" && (randomChance > 2f || !firstResponse))
            {
                firstResponse = true; //record that at least one person has been alerted
                if (!hit.gameObject.GetComponent<SmartAI>().getDead())
                {
                    float distRelative = Vector3.Distance(transform.position, hit.transform.position);
                    float ranXoffset = Random.Range(0.1f * distRelative, -0.1f * distRelative);
                    float ranZoffset = Random.Range(0.1f * distRelative, -0.1f * distRelative);
                    int amountWander = Random.Range(1, 4);
                    //Debug.Log("Sound heard! " + hit.gameObject.name);
                    hit.gameObject.GetComponent<SmartAI>().setWanderDest(new Vector3(transform.position.x + ranXoffset, transform.position.y, transform.position.z + ranZoffset), amountWander); //Add random value to make response more realisitc - not knowing exact location
                }

                responders++;
                if (responders >= 3) //Maximum 3 responders
                    break;
            }

        }
    }

    public void FinishReload()
    {
        done = true;
    }

    public void FinishCycle()
    {
        cycling = false;
    }

    public void RefillAmmo()
    {
        AmmoPool = MaxAmmo;
    }
    public int GetMag()
    {
        return CurMag;
    }


    private void CheckSight()
    {
        bool lookingAt = false;
        if (CharacterControllerScript.instance != null && !active && !blockUse && !WeaponSelection.instance.IsPlacing()) //if player isnt null, or player isnt already using gun, or bot isnt using gun, then check sight
        {
            
            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < interactDistance)
            {
                RaycastHit hit;

                // if raycast hits, it checks if it hit this
                if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out hit, interactDistance, ~raycastIgnore))
                {
                    if (hit.collider.gameObject.Equals(gameObject))
                        lookingAt = true;
                    else
                        lookingAt = false;
                }
                else
                    lookingAt = false;

            }
            else lookingAt = false;
        }

        if (lookingAt && Time.timeScale > 0)
        {
            if (!active)
                CanvasScript.instance.popUp.text = "E to use";

            if (Input.GetButtonDown("Interact"))
            {
                if (active) //shouldn technically be possible to reach this, but ive left it coz why not
                    Leave();
                else
                    Enter();
            }
        }
        else if (active && Time.timeScale > 0 && Input.GetButtonDown("Interact"))
            Leave();
    }

    private void Leave()
    {
        active = false;

        WeaponSelection.instance.gameObject.SetActive(true);

        CharacterControllerScript.Active = true;

        if(parentPlayer)
            CharacterControllerScript.instance.transform.parent = null;

        CharacterControllerScript.instance.transform.position -= transform.forward;

        CameraMove.instance.Active = true;
        CameraMove.instance.transform.parent.localEulerAngles = Vector3.zero;

        OverlayCameraScript.instance.gameObject.SetActive(true);

        CanvasScript.reticle.GetComponent<RectTransform>().localPosition = Vector3.zero;

        ads = false;

        if (AimType != EmplacedAimType.normal)
        {
            pCam.enabled = true;
            aimCam.SetActive(false);
        }

        if (leaveEvent != null)
            leaveEvent.Invoke();
    }

    private void Enter()
    {
        active = true;

        WeaponSelection.instance.gameObject.SetActive(false);

        CharacterControllerScript.Active = false;

        CharacterControllerScript.isCrouching = false;

        if(parentPlayer)
            CharacterControllerScript.instance.transform.parent = transform;

        CharacterControllerScript.instance.transform.position = playerPosition.position;

        CameraMove.instance.Active = false;

        //CameraMove.instance.transform.localEulerAngles = new Vector3(0, 180, 0);

        //pCam.fieldOfView = hipFov;

        OverlayCameraScript.instance.gameObject.SetActive(false);

        ads = false;

        if (enterEvent != null)
            enterEvent.Invoke();
    }

    private void UpdateRot()
    {
        if (active)
        {
            if(infiniteAmmo)
                WeaponSelection.instance.AmmoText.text = CurMag.ToString();
            else
                WeaponSelection.instance.AmmoText.text = CurMag.ToString() + "/" + AmmoPool.ToString();

            if (Time.timeScale > 0)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }


            if (Time.timeScale > 0)
            {
                
                
                //Convert to negative
                if (pitch > 180) //shouldnt realistically pass 90 degree rotation unless going negative
                    pitch -= 360f;
                if (yaw > 180) //shouldnt realistically pass 90 degree rotation unless going negative
                    yaw -= 360f;
                
                yaw += Input.GetAxis("Mouse X") * speedAcross; //Input.GetAxis("Mouse X")
                pitch -= Input.GetAxis("Mouse Y") * speedUp;
                pitch = Mathf.Clamp(pitch, minMaxPitch.x, minMaxPitch.y);
                yaw = Mathf.Clamp(yaw, minMaxYaw.x, minMaxYaw.y);
                //Debug.Log("X: " + Input.GetAxis("Mouse X") + " Y: " + Input.GetAxis("Mouse Y") + " Pitch " + pitch + " Yaw " + yaw );
                /*if (Input.GetAxis("Mouse X") != 0)
                {
                    float x = Input.GetAxis("Mouse X");
                    transform.parent.Rotate(0f, 200.0f * x * Time.deltaTime, 0f);
                }*/ //OldStyle
                if (ads)
                {
                    Vector3 targetDir = (reticuleWorldPos - CameraMove.instance.transform.parent.position).normalized;
                    Vector3 newDirection = Vector3.RotateTowards(CameraMove.instance.transform.parent.forward, targetDir, (Vector3.Angle(CameraMove.instance.transform.parent.forward, targetDir)*3f) * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

                    CameraMove.instance.transform.parent.rotation = Quaternion.LookRotation(newDirection);
                    //CameraMove.instance.transform.parent.LookAt(reticuleWorldPos);
                }
                else if (lookPos != null)
                    CameraMove.instance.transform.parent.LookAt(lookPos);
                else
                    CameraMove.instance.transform.parent.LookAt(BulletSpawnPoint);
                if (!parentPlayer)
                {

                    if (ads) // *********** made this be ignored, dont think its needed
                        CameraMove.instance.transform.parent.eulerAngles += new Vector3(0f, 0f, 0f);//pitch / 2f
                    else
                    {
                        CameraMove.instance.transform.parent.eulerAngles += new Vector3(pitch / 2f - 10f, 0f, 0f);
                        CameraMove.instance.transform.parent.eulerAngles += new Vector3(0f, yaw / 2f, 0f);   //************** SHOULD CameraMove.instance.transform.parent BE REPLACED? IS THIS THE SAME AS pCam? ************************************
                    }
                }
                //convert back to normal if was negative
                if (pitch < 0)
                    pitch += 360;
                if (yaw < 0)
                    yaw += 360;

                if (yawObject == pitchObject.transform) //in the case pitch and yaw are applied to same object
                {
                    yawObject.localEulerAngles = new Vector3(pitch, yaw, 0f);
                }
                else
                {
                    yawObject.localEulerAngles = new Vector3(0f, yaw, 0f);
                    pitchObject.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
                }
            }
        }
    }

    private void UpdateReticule()
    {
        if(reticule!=null)
            reticule.gameObject.SetActive(active);

        if (active)
        {
            RaycastHit hit;

            // if raycast hits, it checks if it hit this
            if (barrelCount == 1)
            {
                if (Physics.Raycast(BulletSpawnPoint.position, BulletSpawnPoint.forward , out hit, 200f, ~raycastIgnore))
                {
                    reticuleWorldPos = hit.point;
                    //reticule.position = hit.point;
                    //RectTransform myRect = CanvasScript.reticle.GetComponent<RectTransform>();
                    Vector3 myPositionOnScreen;
                    if(ads && AimType==EmplacedAimType.camWReticle)
                        myPositionOnScreen = aimCameraComp.WorldToScreenPoint(hit.point);
                    else
                        myPositionOnScreen = pCam.WorldToScreenPoint(hit.point);


                    float scaleFactor = CanvasScript.instance.GetComponent<Canvas>().scaleFactor;


                    //Vector3 finalPosition = new Vector3(myPositionOnScreen.x/scaleFactor , myPositionOnScreen.y , myPositionOnScreen.z); //CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D.z
                    //myRect.anchoredPosition = finalPosition;
                    
                    CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(myPositionOnScreen.x - (Screen.width / 2f), myPositionOnScreen.y - (Screen.height / 2), myPositionOnScreen.z) / scaleFactor;  //Vector3.Scale(pCam.WorldToScreenPoint(hit.point), CanvasScript.instance.GetComponent<RectTransform>().localScale);

                    //Debug.Log("Final Position: " + finalPosition + " myPos: " + myPositionOnScreen + " Scale Factor: " + scaleFactor);
                    //Debug.Log(" ViewPoint: " + pCam.WorldToViewportPoint(hit.point) + " Hit: " + hit.point + " Scaled: " + (pCam.WorldToViewportPoint(hit.point)* scaleFactor) + " Screen: " + (myPositionOnScreen.x * Screen.width, myPositionOnScreen.y * Screen.height, finalPosition.z) + " Size " + Screen.width);

                }
                else 
                {
                    reticuleWorldPos = BulletSpawnPoint.position + (BulletSpawnPoint.forward * 200f);

                    Vector3 myPositionOnScreen;
                    if (ads && AimType == EmplacedAimType.camWReticle) //if aiming with aimcam and reticle
                        myPositionOnScreen = aimCameraComp.WorldToScreenPoint(BulletSpawnPoint.position + (BulletSpawnPoint.forward * 200f));
                    else
                        myPositionOnScreen = pCam.WorldToScreenPoint(BulletSpawnPoint.position + (BulletSpawnPoint.forward * 200f));

                    CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(myPositionOnScreen.x - Screen.width / 2f, myPositionOnScreen.y - Screen.height / 2, myPositionOnScreen.z);
                    //reticule.position = BulletSpawnPoint.position + (BulletSpawnPoint.forward * -125f);
                }
            }
            else
            {
                if (Physics.Raycast(multiBarrels[curBarrel].bulletSpawn.position, multiBarrels[curBarrel].bulletSpawn.forward, out hit, 200f, ~raycastIgnore))
                {
                    reticuleWorldPos = hit.point;

                    float scaleFactor = CanvasScript.instance.GetComponent<Canvas>().scaleFactor;
                    Vector3 myPositionOnScreen;
                    if(ads && AimType==EmplacedAimType.camWReticle)
                        myPositionOnScreen = aimCameraComp.WorldToScreenPoint(hit.point);
                    else
                        myPositionOnScreen = pCam.WorldToScreenPoint(hit.point);
                     
                    CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(myPositionOnScreen.x - (Screen.width / 2f), myPositionOnScreen.y - (Screen.height / 2f), myPositionOnScreen.z) / scaleFactor;  
                    //reticule.position = hit.point;
                }
                else
                {
                    reticuleWorldPos = multiBarrels[curBarrel].bulletSpawn.position + (multiBarrels[curBarrel].bulletSpawn.forward * 200f);

                    Vector3 myPositionOnScreen;
                    if(ads && AimType == EmplacedAimType.camWReticle)
                        myPositionOnScreen = aimCameraComp.WorldToScreenPoint(multiBarrels[curBarrel].bulletSpawn.position + (multiBarrels[curBarrel].bulletSpawn.forward * -200f));
                    else
                        myPositionOnScreen = pCam.WorldToScreenPoint(multiBarrels[curBarrel].bulletSpawn.position + (multiBarrels[curBarrel].bulletSpawn.forward * -200f));
                    
                    CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(myPositionOnScreen.x - Screen.width / 2f, myPositionOnScreen.y - Screen.height / 2, myPositionOnScreen.z); 
                    //reticule.position = multiBarrels[curBarrel].bulletSpawn.position + (multiBarrels[curBarrel].bulletSpawn.forward * -125f); //was -25f
                }
            }


            
            if(reticule!=null)
                if (reticule.localPosition.z > 10)
                    reticule.localPosition = new Vector3(reticule.localPosition.x, reticule.localPosition.y, 10f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (BulletSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Ray r = new Ray(BulletSpawnPoint.position, BulletSpawnPoint.forward);
            Gizmos.DrawRay(r);
        }
    }

    private void ResetMultiBarrels()
    {
        for(int i=0; i<multiBarrels.Length; i++)
        {
            multiBarrels[i].barrel.localPosition = Vector3.MoveTowards(multiBarrels[i].barrel.localPosition, multiBarrels[i].barrelStartPos, barrelResetSpeed);
        }
    }

    IEnumerator MuzzleFlash()
    {
        expFlash.SetActive(true);
        yield return new WaitForSeconds(expFlashTime);
        expFlash.SetActive(false);
    }

}
