using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPositioning : MonoBehaviour
{
    public Transform connectedObj;
    private Camera pCam;
    private CameraMove pCamScript;
    private RectTransform rTransform;
    private Canvas can;

    private Vector3 calcVec = new Vector3(1f, 0f, 1f);
    // Start is called before the first frame update
    void Start()
    {
        rTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pCam == null)
        {
            pCam = CameraMove.instance.GetComponent<Camera>();
            pCamScript = CameraMove.instance;
            can = CanvasScript.instance.GetComponent<Canvas>();
        }

        //make this so it only runs when in front, otherwise put it at the side or something
        UpdatePosition();
    }

    public void UpdatePosition()
    {

        //Mathf.Clamp(newPos.x, -Screen.width/2, Screen.width);  -> doesnt work because as soon as you rotate too much it bugs out because clammping does not fix the problem when you are facing away
        

        //rTransform.anchoredPosition3D = newPos;
        Vector3 newPos = Vector3.zero;

        /*hmmm
        Vector3 reticuleWorldPos = CalculateWorldPosition( connectedObj.position);
        Vector3 myPositionOnScreen = pCam.WorldToScreenPoint(connectedObj.position);
        float scaleFactor = can.scaleFactor;
        newPos = new Vector3(myPositionOnScreen.x - (Screen.width / 2f), myPositionOnScreen.y - (Screen.height / 2), myPositionOnScreen.z) / scaleFactor;
        */


        //Clamp the position to the canvas bondaries - used in cases where the connected object is not in the cameras field of view
        Vector3 pForward = new Vector3(calcVec.x* pCamScript.transform.forward.x, calcVec.y* pCamScript.transform.forward.y, calcVec.z* pCamScript.transform.forward.z);
        Vector3 dir = connectedObj.position - pCamScript.transform.position;
        dir.x *= calcVec.x;
        dir.y *= calcVec.y;
        dir.z *= calcVec.z;
        float angle =  Vector3.SignedAngle(pForward,  dir, Vector3.up); // Calculated angle around y axis - negative means to the right, positive measn to the left
        
        //rotation around x axis - used to calculate y pos
        //Vector3 pX = new Vector3(0f, pCamScript.transform.forward.y, 0f);
        Vector3 xDir = connectedObj.position - pCamScript.transform.position;
        float mag = xDir.magnitude;
        xDir.x = 0f;
        xDir.z = 0f;
        //float xAngle = Vector3.SignedAngle(pX, xDir, Vector3.right);
        //possible alt - calculate signed angle then rotate unmodified xdir to look forward
        Vector3 yPos = CharacterControllerScript.instance.transform.position + xDir + (CharacterControllerScript.instance.transform.forward*mag);


        //newPos.z =0; //clamp z

        if (angle < -(0.5f*pCam.fieldOfView +15f)) //too far left //|| newPos.x<-Screen.width/2f   *********-> the equation y=0.5x+15 (where x is fov and y is angle) represents the point where the image goes off screen
        {
            newPos.x = -(Screen.width/2f)*1.1f;

            Vector3 myPositionOnScreen = pCam.WorldToScreenPoint(yPos);
            float scaleFactor = can.scaleFactor;
            newPos.y = myPositionOnScreen.y - (Screen.height / 2);
            
            //newPos.y = 0f; //the y goes fucky, this is my shitty fix
        }
        else if (angle > (0.5f * pCam.fieldOfView + 15f))// too far right//|| newPos.x > Screen.width / 2f
        {
            newPos.x = (Screen.width / 2f) * 1.1f;

            Vector3 myPositionOnScreen = pCam.WorldToScreenPoint(yPos);
            float scaleFactor = can.scaleFactor;
            newPos.y = myPositionOnScreen.y - (Screen.height / 2);
            
            //newPos.y = 0f;
        }
        else //all good - do normal calcs
        {
            Vector3 reticuleWorldPos = connectedObj.position;
            Vector3 myPositionOnScreen = pCam.WorldToScreenPoint(connectedObj.position);
            float scaleFactor = can.scaleFactor;
            newPos = new Vector3(myPositionOnScreen.x - (Screen.width / 2f), myPositionOnScreen.y - (Screen.height / 2), myPositionOnScreen.z) / scaleFactor;

            
        }


        //Just some clamps to stop any weird behaviour that may arise
        newPos.x = Mathf.Clamp(newPos.x, -(Screen.width / 2f)*1.1f, (Screen.width / 2)*1.1f);
        newPos.y = Mathf.Clamp(newPos.y, -(Screen.height / 2f)*1.1f, (Screen.height / 2)*1.1f);

        //Debug.Log(newPos + " Pos " + Screen.width + " Angle: " + angle);
        rTransform.anchoredPosition3D = newPos;
    }

    private Vector3 CalculateWorldPosition(Vector3 pos)
    {
        Vector3 camNorm = pCam.transform.forward;
        Vector3 vecFromCam = pos - pCam.transform.position;
        float camNormDot = Vector3.Dot(camNorm, vecFromCam.normalized);
        if (camNormDot <= 0f) //behind cam
        {
            //project pos to cam plane
            float camDot = Vector3.Dot(camNorm, vecFromCam);
            Vector3 projection = (camNorm * camDot * 1.01f);
            pos = pCam.transform.position + (vecFromCam - projection);
        }
        return pos;
    }
}
