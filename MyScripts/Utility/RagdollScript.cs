using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollScript : MonoBehaviour
{
    public Transform ragdoll;
    public Animator anim;
    public float ragdollLife = 5f;
    public Rigidbody[] ragdollRBs; //all the rigid bodies of a ragdoll - could have this as auto detect but that adds a performance hit when spawning 
    public Rigidbody[] detachableObjects;

    public void Ragdoll()
    {
        ragdollLife = Mathf.Abs(ragdollLife); //prevents negative values being used

        if (ragdoll != null)
            ragdoll.parent = null;
        

        if (anim != null)
            anim.enabled = false;

        for (int i = 0; i < ragdollRBs.Length; i++)
        {
            ragdollRBs[i].isKinematic = false;
            ragdollRBs[i].useGravity = true;
        }

        for(int j=0; j<detachableObjects.Length; j++)
        {
            detachableObjects[j].transform.parent = null;
            detachableObjects[j].isKinematic = false;
            detachableObjects[j].useGravity = true;

            Destroy(detachableObjects[j].gameObject, ragdollLife);
        }

        if (ragdoll != null)
            Destroy(ragdoll.gameObject, ragdollLife);
        
    }
}
