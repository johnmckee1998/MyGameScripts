using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public ShootingScript gunScript;
    public Transform recoilObject;

    [System.Serializable]
    public struct PhysicalRecoilPattern
    {
        [Tooltip("0.2 default")]
        public float recoilForce;
        [Tooltip("10 default")]
        public float XrecoilRotForce;
        [Tooltip("10 default")]
        public float YrecoilRotForce;
    }

    [Header("Recoil Properties")]
    public bool randomPosNeg;
    public Vector3 recoilVector = Vector3.one;

    public PhysicalRecoilPattern hipRecoil;
    public PhysicalRecoilPattern adsRecoil;

    public float recoilResetTime = 0.1f;
    [Tooltip("Speed at which the gun lerps recoil rotation back to 0 -> different to recoilrest which is for linear movement")]
    public float recoilRotReset = 0.1f;
    public float recoilLinearReset = 0.1f;
    [HideInInspector]
    public Vector3 recoilResetForce;

    private Vector3 recoilAmountRemaining; //used to smooth out recoil - rather than apply all at once, some is applied instantly with the rest applied smoothly over time

    //private float xRecoilRotAmount; //extra x rotation componet for recoil

    private float yRotAmount;
    private float xRotAmount;

    //values used to reset gun after 'animating' 
    private Vector3 startPos;
    private Quaternion startRot;
    private float startYRot;
    private float startXRot;

    private float XrecoilResetRotRef;
    private float YrecoilResetRotRef;
    // Start is called before the first frame update
    void Start()
    {
        startPos = recoilObject.localPosition;
        startRot = recoilObject.localRotation;
        startYRot = recoilObject.localEulerAngles.y;
        startXRot = recoilObject.localEulerAngles.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        xRotAmount = Mathf.SmoothDamp(xRotAmount, 0, ref XrecoilResetRotRef, recoilRotReset); //rotation recoil reset
        yRotAmount = Mathf.SmoothDamp(yRotAmount, 0, ref YrecoilResetRotRef, recoilRotReset); //rotation recoil reset
        //Resetting linear Recoil
        recoilObject.localPosition = Vector3.SmoothDamp(recoilObject.localPosition, startPos, ref recoilResetForce, recoilLinearReset);//reset recoil/move to normal pos


        //Applying Rotational Recoil
        //if (!gunScript.GetAim() && (!CharacterControllerScript.isSprinting || gunScript.GetReloading()))

        recoilObject.localRotation = Quaternion.RotateTowards(recoilObject.localRotation, Quaternion.Euler(new Vector3(startXRot + xRotAmount, startYRot + yRotAmount, 0f)), 1f * 2f); //MAKE THIS NOT HARD CODED ************************************************
        
        //else if (!CharacterControllerScript.isSprinting)//when aiming do it slower
        //    recoilObject.localEulerAngles = new Vector3(adsPos.localEulerAngles.x + xRecoilRotAmount + xRotAmount / 2, adsPos.localEulerAngles.y + yRotAmount / 2, zRotAmount);
        //else if (!gunScript.GetReloading())//when sprinting use sprint rotation
        //{ //sprint condition
        //    recoilObject.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(new Vector3(sprintPos.localEulerAngles.x + xRecoilRotAmount + xRotAmount / 2, sprintPos.localEulerAngles.y + yRotAmount / 2, zRotAmount)), 1f * 2f); //make THIS NOT HARD CODED
        
        //}


        //Smooth recoil - the left over recoil (half is applied instantly, half applied over time)
        if (recoilAmountRemaining.x > 0.01f)
        {
            xRotAmount += recoilAmountRemaining.x * 0.75f; //xrecoil
            yRotAmount += recoilAmountRemaining.y * 0.75f; //yrecoil
            recoilObject.localPosition -= recoilVector * (recoilAmountRemaining.z * 0.75f);  //liner recoil
            //diminishing recoil leftover
            recoilAmountRemaining.x *= 0.25f;
            recoilAmountRemaining.y *= 0.25f;
            recoilAmountRemaining.z *= 0.25f;
        }
    }

    public void ApplyRecoil()
    {
        Debug.Log("REcoil!");
        float recoilModifier = 1f;
        if (!gunScript.GetAim() || !gunScript.separateADSRecoil)
        {
            float isYpos = 1;
            if (randomPosNeg)
                isYpos = (Random.Range(0, 2) * 2 - 1); //randomly picks positive or negative
            recoilObject.localPosition -= recoilVector * (hipRecoil.recoilForce / 2) * recoilModifier; //Recoil 
            xRotAmount += hipRecoil.XrecoilRotForce * 0.5f * isYpos * recoilModifier;
            yRotAmount += hipRecoil.YrecoilRotForce * 0.5f * isYpos * recoilModifier;
            recoilAmountRemaining.z = hipRecoil.recoilForce * 0.5f * recoilModifier; //remaing backward recoil
            recoilAmountRemaining.x = hipRecoil.XrecoilRotForce * 0.5f * isYpos * recoilModifier; //remaining x rotation
            recoilAmountRemaining.y = hipRecoil.YrecoilRotForce * 0.5f * isYpos * recoilModifier;//remaining y rotation
        }
        else
        {
            float isYpos = 1;
            if (randomPosNeg)
                isYpos = (Random.Range(0, 2) * 2 - 1); //randomly picks positive or negative
            recoilObject.localPosition -= recoilVector * (adsRecoil.recoilForce / 2); //Recoil 
            xRotAmount += adsRecoil.XrecoilRotForce * 0.5f * isYpos;
            yRotAmount += adsRecoil.YrecoilRotForce * 0.5f * isYpos;
            recoilAmountRemaining.z = adsRecoil.recoilForce * 0.5f; //remaing backward recoil
            recoilAmountRemaining.x = adsRecoil.XrecoilRotForce * 0.5f * isYpos; //remaining x rotation
            recoilAmountRemaining.y = adsRecoil.YrecoilRotForce * 0.5f * isYpos;//remaining y rotation
        }
    }
}
