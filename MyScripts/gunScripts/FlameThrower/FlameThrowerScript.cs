using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlameThrowerScript : GunBase
{
    public float detectRangeOld = 40f;

    [Header("Base Attributes")]
    //public float damage = 10;
    //public float range = 100;
    public bool usesAim = false;

    //private bool aiming = false;

    
    
    
    

    private Animator gunAnim;

    private float nextFire = 0f;

    private ParticleSystem flame;
    
    private AudioSource bang;

    [Header("Camera Properties")]
    public float AimFov = 40f;
    private Camera pCam;
    private float pCamFov;
    private float pCamRef;
    private CameraMove pCamScript;
    
    private float recoilResetRotRef;

    public Transform adsPos;

    private bool started = false;

    private bool ads = false;
    private float adsSpread;
    private float hipSpread;


    //used to smoothly rotate the gun when turning (just using yawSpeed directly gives jittery movement)
    private float yRotAmount;
    private float yRotVel;
    private float xRotAmount;
    private float xRotVel;
    //Used in recoil
    private float zRotAmount;

    //values used to reset gun after 'animating' 
    private Vector3 startPos;
    private float startYRot;
    
    void Start()
    {
        flame = transform.GetComponentInChildren<ParticleSystem>();
        bang = transform.GetComponent<AudioSource>();

        gunAnim = GetComponentInParent<Animator>();

        CurMag = MagSize;

        MaxAmmo = AmmoPool;

        started = true;



        startPos = transform.localPosition;

        startYRot = transform.localEulerAngles.y;
        yRotAmount = 0;

        xRotAmount = 0;
        

        pCam = GetComponentInParent<Camera>();
        pCamFov = pCam.fieldOfView;

        pCamScript = pCam.GetComponent<CameraMove>();

        
    }


    // Update is called once per frame
    void Update()
    {
        CharacterControllerScript.isAiming = ads;

        bang.pitch = Time.timeScale;
        if (CurMag <= 0 && Input.GetButtonDown("Fire1"))
            WeaponSelection.ClickSound.PlayOneShot(WeaponSelection.ClickSound.clip, 0.75f);


        if (Input.GetButtonDown("Fire2") && usesAim && Time.timeScale > 0 && !reloading)
        {
            ads = !ads;

        }

        try
        {
            gunAnim.SetBool("Reload", reloading);
        }
        catch
        {
            //Debug.Log("nah reload");
            //Debug.Log(flame.isEmitting);
        }
        CanvasScript.reticle.SetActive(!ads); //update whether the reticle should be active


        
        //semi auto fire
        if (Input.GetButton("Fire1") && Time.time >= nextFire && Time.timeScale > 0 && CurMag > 0 && !reloading)
        {
            nextFire = Time.time + 1.0f / 20;
            
            //Shoot();
            if(!flame.isEmitting)
                flame.Play();
            bang.Play();

            CurMag--;
        }
        else if(!Input.GetButton("Fire1") || CurMag<=0)
        {
            flame.Stop();
            bang.Stop();
        }
        


        if (Input.GetKeyDown("r") && !reloading && AmmoPool > 0 && MagSize != CurMag)
        {
            StartCoroutine(Reload());
        }

        //Recoil reset - note: uses smoothdamp as this gives a smoother, more natural movement 
        if (ads)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, adsPos.localPosition, ref adsRef, adsSpeed);//reset recoil/move to ads pos
            transform.localRotation = adsPos.localRotation; //align with aim pos angles to enusre proper sight alignment

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, AimFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        else
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref adsRef, adsSpeed);//reset recoil/move to normal pos

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, pCamFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }


        //Caluclations for the weapon 'sway'/movement when spining 
        if (CameraMove.yawSpeed != 0)
            yRotAmount = Mathf.SmoothDamp(yRotAmount, CameraMove.yawSpeed, ref yRotVel, 0.1f);
        else //recenter gun smoothly to stop jittering
            yRotAmount = Mathf.SmoothDamp(yRotAmount, 0, ref yRotVel, 0.5f);


        if (CameraMove.pitchSpeed != 0)
            xRotAmount = Mathf.SmoothDamp(xRotAmount, CameraMove.pitchSpeed, ref xRotVel, 0.1f);
        else
            xRotAmount = Mathf.SmoothDamp(xRotAmount, 0, ref xRotVel, 0.5f);


        //if (ads)
        //yRotAmount += 1; //just to line up aiming
        //if (ads)
        //transform.localEulerAngles = new Vector3(1 * 0 + xRotAmount, adsPos.rotation.y + yRotAmount, -1 * 0);
        //else

        //resent recoil rot
        zRotAmount = Mathf.SmoothDamp(zRotAmount, 0, ref recoilResetRotRef, adsSpeed);

        //applying weapon sway/spin ONLY IF NOT AIMING
        if (!ads)
            transform.localEulerAngles = new Vector3(xRotAmount, startYRot + yRotAmount, zRotAmount);

    }

    
    private void Shoot()
    {
        if (UsesShotDetection)
            ShotDetection();

        /*
        //ApplyRecoil
        if (ads)
            transform.localPosition -= new Vector3(Vector3.forward.x * recoilVector.x, Vector3.forward.y * recoilVector.y, Vector3.forward.z * recoilVector.z) * recoilForce / 5; //Recoil Aim
        else
        {
            transform.localPosition -= new Vector3(Vector3.forward.x * recoilVector.x, Vector3.forward.y * recoilVector.y, Vector3.forward.z * recoilVector.z) * recoilForce; //Recoil movement
            //transform.localEulerAngles = new Vector3(0,0,1) * recoilRotForce;
            int randTurn = Random.Range(1, 3);
            if (randTurn == 1)
                zRotAmount += XrecoilRotForce;//Recoilturn
            else
                zRotAmount -= XrecoilRotForce;
        }
        */
        //Apply Camera Recoil
        //pCam.transform.localEulerAngles += new Vector3(RecoilCamRotX, RecoilCamRotY, 0);
        if (pCamScript != null)
        {
            pCamScript.ChangePitch(Random.Range(hipRecoilPattern.RecoilCamRotXMin, hipRecoilPattern.RecoilCamRotXMax));
            pCamScript.ChangeYaw(Random.Range(hipRecoilPattern.RecoilCamRotYMin, hipRecoilPattern.RecoilCamRotYMax));
        }
    }

    

    IEnumerator Reload()
    {
        reloading = true;
        done = false;
        ads = false;
        WeaponSelection.IsReloading = reloading;
        //Debug.Log(gunAnim.GetCurrentAnimatorStateInfo(0).tagHash);

        //yield return new WaitForSeconds(reloadTime);

        while (reloading)
        {
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
                WeaponSelection.IsReloading = reloading;
            }
            yield return null;
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
                gunAnim.SetBool("Switch", false);
            }
            catch
            {
                Debug.Log("no reaload anim");
            }
            //gunAnim.enabled = true;
        }
    }

    

    //Alerts nearby smartai of gunshot
    

    
}
