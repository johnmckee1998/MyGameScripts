using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuppressionEvent : MonoBehaviour
{
    
    public float maxSuppression = 50;

    public void AddSuppression()
    {
        if(SuppressionManager.instance!=null)
            SuppressionManager.instance.AddSurpression(maxSuppression - Vector3.Distance(transform.position, CharacterControllerScript.instance.pCam.transform.position)*10f);
    }
}
