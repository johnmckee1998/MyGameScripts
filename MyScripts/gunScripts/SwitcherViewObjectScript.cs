using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherViewObjectScript : MonoBehaviour
{
    public Transform hipPos;
    public Transform aimPos;

    private Vector3 moveRef1;
    private Vector3 moveRef2;

    private bool ads = false;

    private bool IsEquipped = true;

    public GameObject SwitcherAi;
    public GameObject pistol;

    private MeshRenderer objMesh;
    private MeshRenderer redFilterMesh;
    // Start is called before the first frame update
    void Start()
    {
        //objMesh = GetComponent<MeshRenderer>();
        //redFilterMesh = objMesh.transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("h") && Time.timeScale > 0)
            IsEquipped = !IsEquipped;

        SwitcherAi.SetActive(IsEquipped);
        pistol.SetActive(IsEquipped);
        //objMesh.enabled = IsEquipped;
        //redFilterMesh.enabled = IsEquipped;

        if (Input.GetButtonDown("Fire2") && Time.timeScale > 0 && !WeaponSelection.IsReloading)
        {
            ads = !ads;

        }

        //More to or from ads pos NO LONGER USED AS VIEW IS ATTACHED TO GUN
        /*
        if (ads && false)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aimPos.localPosition, ref moveRef1, 0.25f);//reset recoil/move to ads pos
            transform.localRotation = aimPos.localRotation; //align with aim pos angles to enusre proper sight alignment

            //pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, AimFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        else if(false)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, hipPos.localPosition, ref moveRef2, 0.25f);//reset recoil/move to normal pos

            //pCam.fieldOfView = Mathf.SmoothDamp(pCam.fieldOfView, pCamFov, ref pCamRef, 0.25f); //resize cam fov when aiming
        }
        */
        
    }
}
