using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLaser : MonoBehaviour
{
    public LineRenderer lineRen;

    public float maxLength = 500f;


    private int lineIndex;
    private int maxIndex = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        lineIndex = 1;
        lineRen.positionCount = lineIndex + 1;
        RaycastHit rayHit;
        if (Physics.Raycast(transform.position, transform.forward, out rayHit, maxLength))
        {
            if (rayHit.collider.tag.Equals("LaserTarget"))
            {
                rayHit.transform.GetComponent<LaserTarget>().powered = true;
                lineRen.SetPosition(1, transform.InverseTransformPoint(rayHit.point));
            }
            else if (rayHit.collider.tag.Equals("MirrorCube"))
                LineUpdateRecursive(rayHit.point, rayHit.normal, transform.forward);
            else
                lineRen.SetPosition(1, transform.InverseTransformPoint(rayHit.point));

            //Debug.Log("HIT: " + rayHit.point + " " + rayHit.collider.name);
        }
        else
        {
            lineRen.SetPosition(1, transform.InverseTransformPoint(transform.position + transform.forward * maxLength));
            //Debug.Log("MISS");
        }
    }

    private void LineUpdateRecursive(Vector3 point, Vector3 normal, Vector3 forward)
    {
        //Debug.Log("Recursion " + lineIndex);
        if (point != null && lineIndex<=maxIndex) //valid point is given, add it and check again
        {
            
            lineRen.SetPosition(lineIndex, transform.InverseTransformPoint(point)); //set point

            //index line
            lineIndex++;

            //if(lineIndex>= lineRen.positionCount+1)
                lineRen.positionCount = lineIndex+1;
            
            
            //raycast new point
            Vector3 newForward = Vector3.Reflect(forward, normal * -1);

            RaycastHit rayHit;
            if (Physics.Raycast(point, newForward, out rayHit, maxLength))
            {
                if (rayHit.collider.tag.Equals("LaserTarget"))
                {
                    rayHit.transform.GetComponent<LaserTarget>().powered = true;
                    lineRen.SetPosition(lineIndex, transform.InverseTransformPoint(rayHit.point)); //set point
                    return;
                }
                else if (rayHit.collider.tag.Equals("MirrorCube"))
                {
                    LineUpdateRecursive(rayHit.point, rayHit.normal, newForward);
                }
                else
                {
                    lineRen.SetPosition(lineIndex, transform.InverseTransformPoint(rayHit.point)); //set point
                    return;
                }
            }
            else
            {
                lineRen.SetPosition(lineIndex, transform.InverseTransformPoint(point + newForward * maxLength)); //set point
                return;
            }
        }
        else
            return;//break recursion
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < lineRen.positionCount; i++)
            Gizmos.DrawSphere(transform.TransformPoint(lineRen.GetPosition(i)), 0.25f);
    }
}
