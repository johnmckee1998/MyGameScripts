using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKGunHandPositioning : MonoBehaviour
{
    public enum HandType {Left, Right};
    public HandType HType;
    private GunBase currentGunScript;

    // Update is called once per frame
    void Update()
    {
        //update gun
        if(currentGunScript==null || !currentGunScript.transform.parent.gameObject.activeSelf) //only update if there is no gun or if gun has been dsiabled - prevents premature switchover of hand positioning
            currentGunScript = WeaponSelection.instance.GetCurrentWeapon();


        //move to guns Ik pos
        if(HType == HandType.Left)
        {
            //move to left hand pos
            if (currentGunScript.leftHand != null)
            {
                transform.position = currentGunScript.leftHand.position;
                transform.rotation = currentGunScript.leftHand.rotation;
                //transform.position = Vector3.MoveTowards(transform.position, currentGunScript.leftHand.position, Time.deltaTime);
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, currentGunScript.leftHand.rotation, 180f*Time.deltaTime);
            }
        }
        else
        {
            //move to right hand Pos
            if (currentGunScript.rightHand != null)
            {
                transform.position = currentGunScript.rightHand.position;
                transform.rotation = currentGunScript.rightHand.rotation;
            }
        }


    }
}
