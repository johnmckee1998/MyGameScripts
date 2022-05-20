using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportBinocs : GunBase
{
    [Header("Binocs Config")]
    public float adsFov = 40f;
    public GameObject adsFilter;
    public LayerMask rayCastIgnore;
    public enum BinocMode {Artillery, Mortar};
    public BinocMode mode;
    public GameObject positionMarker;
    public float posMarkerLife = 1f;
    private bool ads;
    private Camera pCam;
    private float pCamRef;
    private Animator gunAnim;


    private bool started;
    // Start is called before the first frame update
    void Start()
    {
        gunAnim = GetComponentInParent<Animator>();


        started = true;

        posMarkerLife = Mathf.Abs(posMarkerLife); //just incase some idiot puts a negative value
    }

    // Update is called once per frame
    void Update()
    {
        if (CharacterControllerScript.isSprinting)
            ads = false;

        UpdateAim();

        if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && !WeaponSelection.instance.IsPlacing())
            if (mode == BinocMode.Artillery)
                UpdateArtySupport();
            else
                UpdateMortarPosition();
    }

    private void UpdateAim()
    {
        if (Input.GetButtonDown("Fire2") && Time.timeScale > 0 && !CharacterControllerScript.isSprinting)
            ads = !ads;

        adsFilter.SetActive(ads);

        if (pCam == null)//this check is in update and fixed update, is this needed?
        {
            pCam = CameraMove.instance.GetComponent<Camera>();
        }

        //Recoil reset - note: uses smoothdamp as this gives a smoother, more natural movement 
        if (ads && !CharacterControllerScript.isSprinting && Time.timeScale > 0) //ads lerp
        {
            //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, adsPos.localPosition, ref recoilResetForce, adsSpeed);//reset recoil/move to ads pos
                                                                                                                                        //transform.localRotation = adsPos.localRotation; //align with aim pos angles to enusre proper sight alignment //the bit that handles sway does this

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, adsFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        else if (!ads && (!CharacterControllerScript.isSprinting || reloading) && Time.timeScale > 0) //hip lerp - also used while reloading even if sprinting
        {
            
            //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startPos, ref recoilResetForce, adsSpeed);//reset recoil/move to normal pos
            

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, CharacterControllerScript.instance.hipFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        else if (Time.timeScale > 0)//sprint lerp
        {
            //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, sprintPos.localPosition, ref recoilResetForce, adsSpeed);//reset recoil/move to normal pos
            

            pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, CharacterControllerScript.instance.hipFov + 5f, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        pCam.fieldOfView = Mathf.Clamp(pCam.fieldOfView, 10, 110); 
    }

    private void UpdateArtySupport()
    {
        if(ads && Time.timeScale>0 && WeaponSelection.instance.callInAmount > 0)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(pCam.transform.position, pCam.transform.forward, out rayHit, 500f, ~rayCastIgnore))
            {
                Instantiate(WeaponSelection.instance.callIn, rayHit.point, Quaternion.Euler(Vector3.zero));
            }
            else
            {
                //play error sound
                Debug.Log("Invalid call in placement");
            }
        }
    }

    private void UpdateMortarPosition()
    {
        RaycastHit rayHit;
        if (Physics.Raycast(pCam.transform.position, pCam.transform.forward, out rayHit, 500f, ~rayCastIgnore))
        {
            AiMortarScript.mortarTarget = rayHit.point;
            //put a visual marker for the point
            GameObject g = Instantiate(positionMarker, rayHit.point, Quaternion.Euler(Vector3.zero));
            Destroy(g, posMarkerLife);
            //Instantiate(WeaponSelection.instance.callIn, rayHit.point, Quaternion.Euler(Vector3.zero));
        }
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
            }
            catch
            {
                Debug.Log("Gun anim not set up: Switch");
            }
        }
        
    }
}
