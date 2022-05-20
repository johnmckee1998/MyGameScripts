using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionScript : MonoBehaviour
{
    public Transform[] cameraPositions;
    public Transform centerPos;
    private Vector3 camPosRef;

    private int posCount;

    private Transform curPos;
    private int curPosIndex;

    private Vector3 startPos;
    private Quaternion startRot;
    // Start is called before the first frame update
    void Start()
    {
        posCount = cameraPositions.Length;
        curPos = cameraPositions[0];
        curPosIndex = 0;

        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.position = Vector3.SmoothDamp(transform.position, curPos.position, ref camPosRef, 0.01f); //move to current pos
        //transform.rotation = Quaternion.Lerp(transform.rotation, curPos.rotation, 0.5f);// move to current rot

        //transform.position = curPos.position;
        

        

    }

    private void Update()
    {
        
        if (Input.GetKeyDown("c") && posCount > 1 && Time.timeScale > 0)
        {
            if (curPosIndex < posCount - 1) //cycle to next pos
            {
                curPosIndex++;
                curPos = cameraPositions[curPosIndex];

                //transform.rotation = curPos.rotation;
            }

            else // loop back to first pos
            {
                curPosIndex = 0;
                curPos = cameraPositions[0];

                //transform.rotation = curPos.rotation;
            }
        }
        
        if (Input.GetMouseButton(0) && Time.timeScale > 0 && curPosIndex==0)//if left mouse click
        {
            //transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + (Input.GetAxis("Mouse X")*2f), transform.localEulerAngles.z); //rotate camera
            transform.RotateAround(centerPos.position, centerPos.up, (Input.GetAxis("Mouse X") * 2f));
            transform.RotateAround(centerPos.position, centerPos.right, (Input.GetAxis("Mouse Y") * -2f));

            //transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, centerPos.rotation.z, transform.rotation.w);
        }
        else if(Time.timeScale > 0)
        {
            //transform.localRotation = Quaternion.Lerp(transform.localRotation, startRot, 0.01f);// move to current rot
            //transform.localPosition = Vector3.Lerp(transform.localPosition, curPos.localPosition, 0.01f);// move to current rot
            transform.position = curPos.position;
        }
        if (curPosIndex == 0) //only look at center pos when in default 3rd person view
            transform.LookAt(centerPos);
        else //else just look forward
            transform.LookAt(curPos.position + curPos.forward);
        

    }
}
