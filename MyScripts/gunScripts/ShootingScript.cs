using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using UnityEngine.VFX;

public class ShootingScript : GunBase
{
    public GunRecoil recoilScript;
    public bool useAnimRecoil = false;
    [Space]
    public float shakeMagnitude = 0.5f;
    public float shakeRoughness = 0.5f;
    [Header("Raycast Stuff")]
    //public GameObject impactEffect;
    [Tooltip("Used for recticle position")]
    public LayerMask rayCastIgnore;
    public float rayCastDropFactor = 1f;

    [Header("Base Attributes")]
    public float damage = 10;
    public float range = 100;
    public float fireRateRPM = 500f;
    private float fireRatePerSecond = 10.0f;
    public float bulletSpeed = 50f;
    public float penetrationForce = 100f;
    public int numOfShots = 1;
    public float spreadModifier = 1;
    public bool isSemiAuto = false;
    public bool singleShot = false;
    public bool usesAim = false;
    [Tooltip("Uses individual rounds when reloading. E.g. tube shotgun")]
    public bool singleReload = false;

    //private bool aiming = false;

    
    
    public Transform BulletSpawnPoint;

    private Animator gunAnim;

    [Tooltip("0 = Standard bullet. 1 = Grenade round. 2 = Heavy Sniper round, 3= raycast")]
    public int ammoType = 0;
    public bool usesShortReload = false;

    private float nextFire = 0f;
    private float spread= 0.025f;

    private ParticleSystem flash;
    private VisualEffect vfxFlash;
    [Space]
    public GameObject expFlash;
    [Space]
    public GameObject bullet;
    private AudioSource bang;

    [Header("Camera Properties")]
    public float AimFov = 40f;
    private Camera pCam;
    //private float pCamFov;
    private float pCamRef;
    private CameraMove pCamScript;
    

    private float recoilResetRotRef;

    public Transform adsPos;
    public Transform sprintPos;

    [Header("Bolt Properties")]
    public GameObject boltObject;
    public Vector3 boltMovementVector = Vector3.one;
    public float boltRecoilForce = 0.2f;
    public float boltResetSpeed = 0.2f;
    private Vector3 boltResetVector;
    private Vector3 boltHomePos;
    public bool lockOpen = false;
    //private bool wasLockedOpen = false;
    [Header("To be used bolt properties")]
    public bool isOpenBolt = false;
    public Transform restPos; //position of the bolt when ready to fire
    public Transform firedPos; //position of the bolt when fired
    private bool useBoltPos = false;

    [Header("Other")]
    public float meleeDamage = 55f;
    public bool noPartialReload;

    private bool started = false;

    private bool ads = false;
    private float adsSpread;
    private float hipSpread;


    //used to smoothly rotate the gun when turning (just using yawSpeed directly gives jittery movement)
    private float yRotAmount;
    private float yRotVel; //used as reference for sway smoothdamp
    private float xRotAmount;
    private float xRotVel;

    //values used to reset gun after 'animating' 
    private Vector3 startPos;
    private Quaternion startRot;
    private float startYRot;
    private float startXRot;


    private bool meleeAvaliable = true;


    private float lastShotTime = 0;

    private Vector3 recoilAmountRemaining; //used to smooth out recoil - rather than apply all at once, some is applied instantly with the rest applied smoothly over time

    private Vector2 rotationResetDecay; //used to reset camera to original pos after shooting

    [Header("SoundTesting")]
    public AudioClip fullShot;
    public AudioClip startShot;
    public AudioClip midShot;
    public AudioClip endShot;
    private float shotLength;
    private bool playedMidSound;
    private bool firePressed;
    private bool prevFirePressed;

    //private Vector3 startRot;
    void Start()
    {
        flash = transform.GetComponentInChildren<ParticleSystem>();
        vfxFlash = transform.GetComponentInChildren<VisualEffect>();
        bang = transform.GetComponent<AudioSource>();
        hipSpread = spread * spreadModifier;
        adsSpread = spread * spreadModifier / 5;
        

        gunAnim = GetComponentInParent<Animator>();

        CurMag = MagSize;

        MaxAmmo = AmmoPool;

        started = true;

        if(boltObject!=null)
            boltHomePos = boltObject.transform.localPosition;



        startPos = transform.localPosition;
        startRot = transform.localRotation;

        startYRot = transform.localEulerAngles.y;
        startXRot = transform.localEulerAngles.x;
        yRotAmount = 0;

        xRotAmount = 0;

        if (restPos != null && firedPos != null)
            useBoltPos = true;

        //pCam = GetComponentInParent<Camera>();
       // pCamFov = pCam.fieldOfView; //this should be regularly updated from a stored value set by player **********************************

        //pCamScript = pCam.GetComponent<CameraMove>();

        fireRatePerSecond = fireRateRPM / 60f;
        shotLength = 1f / fireRatePerSecond;
    }


    // Update is called once per frame
    void Update()
    {
        if (pCam == null)
        {
            pCam = CameraMove.instance.GetComponent<Camera>();
            pCamScript = CameraMove.instance;
            //pCamFov = CharacterControllerScript.instance.hipFov;
        }

        CharacterControllerScript.isAiming = ads;

        bang.pitch = Time.timeScale;
        if (!WeaponSelection.instance.IsPlacing() && CurMag <= 0 && Input.GetButtonDown("Fire1") && Time.timeScale > 0)
        {
            WeaponSelection.ClickSound.PlayOneShot(WeaponSelection.ClickSound.clip, 0.75f);

        }

        
        if (Input.GetButtonDown("Fire2") && usesAim && Time.timeScale > 0 && !reloading && !CharacterControllerScript.isSprinting)
        {
            ads = !ads;

        }


        CanvasScript.reticleImage.enabled = !ads; //update whether the reticle should be active

        if (WeaponSelection.instance.IsPlacing()) //disable aim while palcing
            ads = false;



        if (!WeaponSelection.instance.IsPlacing() && Time.timeScale > 0) //dont allow shooting when placing or paused
        {
            //single shot fire - e.g. bolt action/lever action/pump action
            if (singleShot)
            {
                if (Input.GetButtonDown("Fire1") && CurMag > 0 && !reloading && !cycling && !CharacterControllerScript.isSprinting)
                {
                    cycling = true;
                    Shoot();
                    gunAnim.SetBool("Cycle", cycling);
                }
            }
            else if (isSemiAuto)
            {
                //SemiAuto fire
                if (Input.GetButtonDown("Fire1") && Time.time >= nextFire && CurMag > 0 && !reloading && !CharacterControllerScript.isSprinting)
                {
                    nextFire = Time.time + 1.0f / fireRatePerSecond;
                    Shoot();
                }
            }
            else
            {
                //Fullauto fire
                if (Input.GetButton("Fire1") && Time.time >= nextFire && CurMag > 0 && !reloading && !CharacterControllerScript.isSprinting)
                {
                    nextFire = Time.time + 1.0f / fireRatePerSecond;
                    Shoot();
                }
            }

            if (Input.GetButtonDown("Fire1") && singleReload && reloading)
                EndReload(); //end reload early when clicking or sprinting during single loading
        }


        

        if (Input.GetKeyDown("r") && !reloading && AmmoPool > 0 && MagSize != CurMag && Time.timeScale > 0)
        {
            if(CurMag<=0 || !noPartialReload)
                StartCoroutine(Reload());
        }

        if (Input.GetKeyDown("v") && meleeAvaliable && Time.timeScale > 0)
            StartCoroutine(MeleeAttack());


        firePressed = Input.GetButton("Fire1");
        if (midShot!=null && (!firePressed || CurMag<=0) && prevFirePressed && playedMidSound)
            StartCoroutine(PlayEndSoundAfterTime());
        prevFirePressed = firePressed;


        if (useAnimRecoil && (!firePressed || CurMag <= 0))
            CancelShooting();

    }

    private void FixedUpdate()
    {
        if (pCam == null)//this check is in update and fixed update, is this needed?
        {
            pCam = CameraMove.instance.GetComponent<Camera>();
            pCamScript = CameraMove.instance;
            //pCamFov = CharacterControllerScript.instance.hipFov;
        }

        //Recoil reset AND ads movement - note: uses smoothdamp as this gives a smoother, more natural movement 
        if (ads && !CharacterControllerScript.isSprinting && Time.timeScale > 0) //ads lerp
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, adsPos.localPosition, ref adsRef, adsSpeed);//reset recoil/move to ads pos
                                                                                                                                               //transform.localRotation = adsPos.localRotation; //align with aim pos angles to enusre proper sight alignment //the bit that handles sway does this

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, AimFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        else if (!ads && (!CharacterControllerScript.isSprinting || reloading) && Time.timeScale > 0) //hip lerp - also used while reloading even if sprinting
        {
            //if(lastShotTime + recoilResetTime*1.5 < Time.time)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startPos, ref adsRef, adsSpeed);//reset recoil/move to normal pos
            //transform.localRotation = startRot; //the bit that handles sway does this

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, CharacterControllerScript.instance.hipFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        else if (Time.timeScale > 0)//sprint lerp
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, sprintPos.localPosition, ref adsRef, adsSpeed);//reset recoil/move to normal pos
            //transform.localRotation = sprintPos.localRotation;

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, CharacterControllerScript.instance.hipFov + 5f, ref pCamRef, 0.25f); //resize cam fov when aiming
        }

        //clamp FOV between 10 and 110
        pCam.fieldOfView = Mathf.Clamp(pCam.fieldOfView, 10, 110); //################## Maybe make this variable? ################################################# Min/Max Fov




        //Bolt Resettings
        if (firedPos != null && lockOpen && CurMag <= 0 && Time.timeScale > 0)
            boltObject.transform.localPosition = firedPos.localPosition;
        else if (boltObject != null && !useBoltPos && Time.timeScale > 0)
        {
            boltObject.transform.localPosition = Vector3.MoveTowards(boltObject.transform.localPosition, boltHomePos, boltResetSpeed);
        }
        else if (useBoltPos && Time.timeScale > 0)
            boltObject.transform.localPosition = Vector3.MoveTowards(boltObject.transform.localPosition, restPos.localPosition, boltResetSpeed);
        //Reset bolt - note: uses lerp as this is consistent and makes more sense for mechanical movement


        //Caluclations for the weapon 'sway'/movement when spining
        if (CameraMove.yawSpeed != 0)
            yRotAmount = Mathf.SmoothDamp(yRotAmount, CameraMove.yawSpeed, ref yRotVel, 0.1f /2f);
        else //recenter gun smoothly to stop jittering
            yRotAmount = Mathf.SmoothDamp(yRotAmount, 0, ref yRotVel, 0.5f /2f); //MAKE THESE USE EXPOSED VARIABLES NOT HARD CODED SHIT


        if (CameraMove.pitchSpeed != 0)
            xRotAmount = Mathf.SmoothDamp(xRotAmount, -CameraMove.pitchSpeed, ref xRotVel, 0.1f /2f);
        else
            xRotAmount = Mathf.SmoothDamp(xRotAmount, 0, ref xRotVel, 0.5f /2f); //should this be restting to 1? or maybe store starting x like i do with y?


        //applying weapon sway
        if (!ads && (!CharacterControllerScript.isSprinting || reloading))
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(new Vector3(startXRot + xRotAmount , startYRot + yRotAmount, 0f)), 1f * 2f); //MAKE THIS NOT HARD CODED ************************************************
        else if (!CharacterControllerScript.isSprinting)//when aiming do it slower
            transform.localEulerAngles = new Vector3(adsPos.localEulerAngles.x + xRotAmount / 2, adsPos.localEulerAngles.y + yRotAmount / 2, 0f);   
        else if (!reloading)//when sprinting use sprint rotation //sprint condition
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(new Vector3(sprintPos.localEulerAngles.x + xRotAmount / 2, sprintPos.localEulerAngles.y + yRotAmount / 2, 0f)), 1f * 2f); //make THIS NOT HARD CODED
            
        


        //camera pos reset - slowly reset to original aiming position - resets camera rotation
        if (rotationResetDecay.x > 0.05f || rotationResetDecay.x <-0.05f)
        {
            

            pCamScript.ChangePitch(rotationResetDecay.x * Time.fixedDeltaTime * 2f);
            pCamScript.ChangeYaw(rotationResetDecay.y * Time.fixedDeltaTime * 2f);
            
            rotationResetDecay.x -= rotationResetDecay.x * Time.fixedDeltaTime * 2f;
            rotationResetDecay.y -= rotationResetDecay.y * Time.fixedDeltaTime * 2f;
        }

        if (!ads)
            UpdateReticle();

    }

    private void Shoot()
    {
        RayCastShoot();
        if (flash != null)
            flash.Play();
        else if (vfxFlash != null)
            vfxFlash.Play();
        else if (expFlash != null)
            StartCoroutine(MuzzleFlash());
        if (midShot != null)
            PlayRandomSound();
        else
            bang.PlayOneShot(bang.clip, bang.volume);

        CurMag--;
    }

    private void RayCastShoot()
    {
        if (UsesShotDetection)
            ShotDetection();
        for (int i = 0; i < numOfShots; i++)
        {
            float randomX = Random.Range(-spreadModifier, +spreadModifier);
            float randomY = Random.Range(-spreadModifier, +spreadModifier);
            GameObject shot = Instantiate(bullet, BulletSpawnPoint.position, Quaternion.LookRotation(BulletSpawnPoint.forward, BulletSpawnPoint.up)*Quaternion.Euler(randomX,randomY,0f));
            RaycastBullet rcB = shot.GetComponent<RaycastBullet>();
            rcB.SetNewStats(bulletSpeed, damage);
            rcB.range = range;
            rcB.dropFactor = rayCastDropFactor;
            
            //shot.transform.eulerAngles += new Vector3(randomX, randomY, 0f);

            rcB.PenForce = penetrationForce;

            

        }
        CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, 0.1f, 0.1f);

        /////////NEW RECOIL!! #@# -> Physical Recoil
        //float isXpos = (Random.Range(0, 2) * 2 - 1); //randomly picks positive or negative
        if (recoilScript != null) //dont necessarily prevent both recoil script and anim from running together - script may animate the body whil the anim will animate the bolt or other things on the gun
            recoilScript.ApplyRecoil();
        else //#@# Animated Recoil  - replaces the physical recoil (does not effect camera recoil) 
        {
            try
            {
                gunAnim.SetBool("Shoot", true);
            }
            catch
            {
                Debug.Log("Anim not set up: Shoot");
            }
        }


        lastShotTime = Time.time;


        //bolt movement
        if (boltObject != null && !useBoltPos)
            boltObject.transform.localPosition -= boltMovementVector * boltRecoilForce;
        else if (useBoltPos)
            boltObject.transform.localPosition = firedPos.localPosition;

        //Apply Camera Recoil Pattern #@#
        //pCam.transform.localEulerAngles += new Vector3(RecoilCamRotX, RecoilCamRotY, 0);
        if (pCamScript != null)
        {
            if (!ads || !separateADSRecoil) //if not aiming or not using a separate pattern for ads or not
            {
                float x = Random.Range(hipRecoilPattern.RecoilCamRotXMin, hipRecoilPattern.RecoilCamRotXMax);
                float y = Random.Range(hipRecoilPattern.RecoilCamRotYMin, hipRecoilPattern.RecoilCamRotYMax);
                pCamScript.ChangePitch(x);
                rotationResetDecay.x += x * -1f;
                pCamScript.ChangeYaw(y);
                rotationResetDecay.y += y * -1f;
            }
            else
            {
                float x = Random.Range(adsRecoilPattern.RecoilCamRotXMin, adsRecoilPattern.RecoilCamRotXMax);
                float y = Random.Range(adsRecoilPattern.RecoilCamRotYMin, adsRecoilPattern.RecoilCamRotYMax);
                pCamScript.ChangePitch(x);
                rotationResetDecay.x += x * -1f;
                pCamScript.ChangeYaw(y);
                rotationResetDecay.y += y * -1f;
            }
        }
    }

    

    IEnumerator Reload()
    {
        reloading = true;
        done = false;
        ads = false;
        WeaponSelection.IsReloading = reloading;
        //Debug.Log(gunAnim.GetCurrentAnimatorStateInfo(0).tagHash);
        bool shortre = false;
        try
        {
            if (usesShortReload && CurMag > 0)
            {
                gunAnim.SetBool("Reload2", reloading);
                shortre = true;
            }
            else 
                gunAnim.SetBool("Reload", reloading);
        }
        catch
        {
            Debug.Log("Anim not set up: Reload");
        }
        //yield return new WaitForSeconds(reloadTime);

        while (reloading)
        {
            if (done)
            {
                if (CurMag < MagSize && !singleReload)
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
                WeaponSelection.IsReloading = reloading;
            }
            yield return null;
        }
        try
        {
            if (shortre)
                gunAnim.SetBool("Reload2", reloading);
            else
                gunAnim.SetBool("Reload", reloading);
        }
        catch
        {
            Debug.Log("Anim not set up: Reload");
        }
        //gunAnim.SetBool("Reload", false);
        
    }


    private void OnEnable()
    {
        reloading = false;

        if (started)
        {
            try
            {
                ads = false;
                gunAnim.SetBool("Switch", false);
                if (singleShot)
                {
                    FinishCycle();
                    gunAnim.SetBool("Cycle", false);
                }
            }
            catch
            {
                Debug.Log("Gun anim not set up: Switch");
            }
            //gunAnim.enabled = true;
        }
    }

    private IEnumerator MeleeAttack()
    {
        meleeAvaliable = false;
        RaycastHit rayHit;
        if (Physics.Raycast(pCam.transform.position, pCam.transform.forward, out rayHit, 2f, ~rayCastIgnore))
        {
            //Apply force to rigidbodies 
            Rigidbody uniRb = rayHit.transform.GetComponent<Rigidbody>();

            if (uniRb != null)
                uniRb.AddForce(80 * pCam.transform.forward);

            UniversalStats uniStats = rayHit.transform.GetComponent<UniversalStats>();

            if (uniStats != null) //Damage objects/enemies with universal stats
            {
                uniStats.DoDamage(meleeDamage);
                uniStats.PlayHitSound();
                
            }
            else
            {
                HitboxScript hb = rayHit.transform.GetComponent<HitboxScript>();
                if (hb != null)
                {
                    hb.DoDamage(55f);
                    //TODO - ADD HIT SOUND OR USE THE MORE COMPLEX DODAMAGE
                }
                    
            }
            

            MaterialEffects matVfx = rayHit.transform.GetComponent<MaterialEffects>();

            if (matVfx != null) // if the bullet hits a static material with effect, play the effects
            {
                matVfx.playHitEffect(rayHit.point, Quaternion.LookRotation(rayHit.normal));
                matVfx.playHitSound(rayHit.point);
                matVfx.AddDamage(rayHit.point, Quaternion.LookRotation(-rayHit.normal));
            }
        }
        transform.position += pCam.transform.forward/2;
        yield return new WaitForSeconds(0.75f);
        meleeAvaliable = true;
    }

    private void EndReload() //used primarily by singleReload to end early when player clicks
    {
        GetComponentInParent<WeaponSwitchAnimationManager>().ReloadDone();
    }

    public void UpdateReticle()
    {
        RaycastHit hit;
        if (Physics.Raycast(BulletSpawnPoint.position, BulletSpawnPoint.forward, out hit, 200f, ~rayCastIgnore))
        {
            Vector3 reticuleWorldPos = hit.point;
            //reticule.position = hit.point;
            //RectTransform myRect = CanvasScript.reticle.GetComponent<RectTransform>();
            Vector3 myPositionOnScreen = pCam.WorldToScreenPoint(hit.point);


            float scaleFactor = CanvasScript.instance.GetComponent<Canvas>().scaleFactor;


            //Vector3 finalPosition = new Vector3(myPositionOnScreen.x/scaleFactor , myPositionOnScreen.y , myPositionOnScreen.z); //CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D.z
            //myRect.anchoredPosition = finalPosition;

            CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(myPositionOnScreen.x - (Screen.width / 2f), myPositionOnScreen.y - (Screen.height / 2), myPositionOnScreen.z) / scaleFactor;  //Vector3.Scale(pCam.WorldToScreenPoint(hit.point), CanvasScript.instance.GetComponent<RectTransform>().localScale);

            //Debug.Log("Final Position: " + finalPosition + " myPos: " + myPositionOnScreen + " Scale Factor: " + scaleFactor);
            //Debug.Log(" ViewPoint: " + pCam.WorldToViewportPoint(hit.point) + " Hit: " + hit.point + " Scaled: " + (pCam.WorldToViewportPoint(hit.point)* scaleFactor) + " Screen: " + (myPositionOnScreen.x * Screen.width, myPositionOnScreen.y * Screen.height, finalPosition.z) + " Size " + Screen.width);

        }
        else
        {
            Vector3 reticuleWorldPos = BulletSpawnPoint.position + (BulletSpawnPoint.forward * 200f);

            Vector3 myPositionOnScreen = pCam.WorldToScreenPoint(BulletSpawnPoint.position + (BulletSpawnPoint.forward * 200f));
            CanvasScript.reticle.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(myPositionOnScreen.x - Screen.width / 2f, myPositionOnScreen.y - Screen.height / 2, myPositionOnScreen.z);
            //reticule.position = BulletSpawnPoint.position + (BulletSpawnPoint.forward * -125f);
        }
    }

    IEnumerator MuzzleFlash()
    {
        expFlash.SetActive(true);
        yield return new WaitForSeconds(0.02f);
        expFlash.SetActive(false);
    }

    private void PlayRandomSound()
    {
        //if (!firePressed)//first shot of burst
        //{
        //    Debug.Log("FS");
        //    bang.clip = midShot; //start shot sound
        //    bang.PlayOneShot(bang.clip, bang.volume);
        //    playedMidSound = true;
        //}
        //else //follow up shots
        //{
            if (CurMag > 1)
            {//more than 1 shot left
                Debug.Log("MS");
                bang.clip = midShot; //mid shot sound
                bang.PlayOneShot(bang.clip, bang.volume);
                playedMidSound = true;
            }
            else //last shot, do full sound
            {
                Debug.Log("LS");
                bang.clip = fullShot; //full shot sound
                bang.PlayOneShot(bang.clip, bang.volume);
                playedMidSound = false;
            }
        //}
    }

    IEnumerator PlayEndSoundAfterTime()
    {
        Debug.Log("Delay Sound");
        float sttw = shotLength - (Time.time - lastShotTime);
        playedMidSound = false;
        yield return new WaitForSeconds(sttw);
        if (!firePressed && CurMag > 0) //prevent lots of end shots being played when tap firing
        {
            bang.clip = endShot;
            bang.PlayOneShot(bang.clip, bang.volume);
        }
    }

    private void CancelShooting()
    {
        try
        {
            gunAnim.SetBool("Shoot", false);
        }
        catch
        {
            Debug.Log("Anim not set up: Shoot");
        }
    }

    public bool GetAim()
    {
        return ads;
    }
    public bool GetReloading()
    {
        return reloading;
    }
}
