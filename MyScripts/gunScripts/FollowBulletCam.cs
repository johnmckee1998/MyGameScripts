using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBulletCam : MonoBehaviour
{
    private GameObject pCam;
    public GameObject thisCam;
    private Vector3 prevPos;
    // Start is called before the first frame update
    private void OnEnable()
    {
        /*
        if (thisCam == null)
            thisCam = GetComponentInChildren<Camera>().gameObject;
        pCam = CharacterControllerScript.instance.pCam;

        CharacterControllerScript.instance.LockMovement(true);
        pCam.SetActive(false);
        thisCam.SetActive(true);
        */

        prevPos = transform.position;

        //DisableCams();
        //TO DO ******IMPORTANT****** -> leaving a turret or opening/closing menus may unlock movement, stop that happening 
        //*************** ALSO*************** you can have more than 1 shot active at once (due to long shots taking longer than reloading), causing issues as there is more than 1 audio listener, either prevent shooting or make it disable all other cameras
        // SORT OF SOLVED THIS - but not efficently - see disable cams - NOPE
    }

    private void OnDestroy() //Have both on destroy and disable so it doesnt matter if this is a newly instansiated obj or a pooled one
    {
        ArtilleryCamManager.cameraOutput.SetActive(false);
        //if(thisCam.activeSelf)
        //    SwitchToPCam();
    }

    private void OnDisable()
    {
        ArtilleryCamManager.cameraOutput.SetActive(false);
        //if(thisCam.activeSelf)
        //    SwitchToPCam();
    }

    // Update is called once per frame
    void Update()
    {
        //if ((Input.GetKeyDown("q") || Input.GetButtonDown("Fire1")) && Time.timeScale > 0 && thisCam.activeSelf)
        //    SwitchToPCam();

        ArtilleryCamManager.cameraOutput.SetActive(true);

    }
    
    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, prevPos) < 0.05f) //stop moving -> hit something
            SwitchToOverview();

        prevPos = transform.position;
        //if (thisCam.activeSelf) //prevent the scenario where a second shot is fired but the first explodes, causing pcam to be re enabled while the second cam is still active
        //    pCam.SetActive(false);
    }
    /*
    private void SwitchToPCam()
    {
        CharacterControllerScript.instance.LockMovement(false);
        pCam.SetActive(true);
        thisCam.SetActive(false);
    }

    public void DisableCams()//broke
    {
        Camera[] cams = FindObjectsOfType<Camera>();
        for (int i = 0; i < cams.Length; i++)
            if (cams[i].gameObject != gameObject)
                cams[i].gameObject.SetActive(false);
    }
    */

    private void SwitchToOverview() //unparent cam so it stays back as sees explosion, then destroy cam
    {
        thisCam.transform.parent = null;
        Destroy(thisCam, 2f);
    }
}
