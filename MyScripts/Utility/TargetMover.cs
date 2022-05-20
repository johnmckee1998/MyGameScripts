using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour
{
    public Transform curTarget;
    public Transform prevTarget;
    private Vector3 tempTarget;

    private Vector3 smoothRef;
    // Start is called before the first frame update
    void Start()
    {
        tempTarget = curTarget.position;
    }

    // Update is called once per frame
    void Update()
    {
        prevTarget.position = Vector3.SmoothDamp(prevTarget.position, tempTarget, ref smoothRef, 0.1f);

        if(Vector3.Distance(tempTarget, curTarget.position) > 1.5f)
        {
            tempTarget = curTarget.position + curTarget.forward;
        }
    }
}
/* Probs set up serialisable object to store left/right leg info - only move a leg if opposing leg is not moving to create a more realistic walking
 * 
 * 
 */
