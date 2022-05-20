using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTargetMover : MonoBehaviour
{
    public Transform connectedBody;
    [Space]
    public Transform limbTarget; //the target that the limb actually follows
    public Transform targetDestinationForward; //the point that the target will move to 
    public Transform targetDestinationBackward; //the point that the target will move to 
    [Tooltip("The sort of centre point (alongside the body) that the limb would be at rest at, too be used for x movement")] //when moving along x, if z velocity is low enough then move here instead? or maybe find a point inbetween either forward/backward and destorigin depending on how fast along x?
    public Transform targetDestOrigin;

    [Space]
    public float moveSpeedModifier = 1.5f;
    public float minMoveSpeed = 5f;
    public float moveDist = 1f;
    public float stepHeight;
    private float worldSpaceStepHeight;
    private bool movingUp;

    private bool moving;



    private float forwardDist;
    private float backwardDist;
    //todo - use racasts to reposition the targetdest so it is always on the ground - could either raycast from the limb root or somewhere above the target dest
    //Also - need to test x and y pos? turning might be odd
    
    private bool movingForward;

    private Vector3 prevPos;
    Vector3 localVelocity;

    private float realMoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        prevPos = connectedBody.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        localVelocity = connectedBody.localPosition - prevPos;

        movingForward = (localVelocity.z > 0f);

        

        forwardDist = Vector3.Distance(limbTarget.position, targetDestinationForward.position);
        backwardDist = Vector3.Distance(limbTarget.position, targetDestinationBackward.position);
        if (((forwardDist >= moveDist && movingForward) || (backwardDist >= moveDist && !movingForward)) && !moving) //the combo of checking forward dist>movedist and checking backdist isnt 0 is because when the limb moves up to a dest, it can then in turn be far enough 
            StartCoroutine(MoveTarget());                                                                               //away from the other dest that it triggers the move in the oposite direction. This check measn that if it is on top of a dest it wont move
                                                                                                                        //BETTER WAY - base the movement off of current movement direction/speed
        prevPos = connectedBody.localPosition;
    }


    IEnumerator MoveTarget()
    {
        moving = true;
        Debug.Log("Moving!");
        Vector3 finalDest;

        //calculate this first so that prevmovespeed isnt based off of incorrect movespeed
        realMoveSpeed = Mathf.Abs(localVelocity.z / Time.fixedDeltaTime) * moveSpeedModifier;

        if (realMoveSpeed < minMoveSpeed)
            realMoveSpeed = minMoveSpeed;

        //float distForward = Vector3.Distance(limbTarget.position, targetDestinationForward.position);
        //float distBackward = Vector3.Distance(limbTarget.position, targetDestinationBackward.position);

        //bool forward = (distForward > distBackward);
        Debug.Log("Forwards: " + movingForward);
        if (movingForward)
            finalDest = targetDestinationForward.position + new Vector3(0, 0, localVelocity.z*moveDist); //  no longer relevant but still a useful thing to know -> //the reason it is /deltatime is because the velocity is only the difference between an update - basically its like its already velocity*deltatime, so dividing it gives me the velocity in m/s
        else
            finalDest = targetDestinationBackward.position + new Vector3(0,0, localVelocity.z*moveDist);
        movingUp = true;
        worldSpaceStepHeight = limbTarget.position.y + stepHeight;

        Vector3 midpoint = (finalDest + limbTarget.position) / 2f;
        midpoint.y = worldSpaceStepHeight;


        bool wasforwad = movingForward; //backup of moving forward
        while(limbTarget.position != finalDest)
        {

            //recalculate this each update so that if the move start when body is slow, the target doesnt lag behind
            float prevMoveSpeed = realMoveSpeed; //make a backup coz if body slows down it looks weird to have fst moving then slow moving legs

            realMoveSpeed = Mathf.Abs(localVelocity.z / Time.fixedDeltaTime) * moveSpeedModifier;

            if (realMoveSpeed < minMoveSpeed)
                realMoveSpeed = minMoveSpeed;

            if (realMoveSpeed < prevMoveSpeed)
                realMoveSpeed = prevMoveSpeed;

            if (limbTarget.position.y >= worldSpaceStepHeight)
                movingUp = false;

            if (wasforwad)
                finalDest = targetDestinationForward.position; //  no longer relevant but still a useful thing to know -> //the reason it is /deltatime is because the velocity is only the difference between an update - basically its like its already velocity*deltatime, so dividing it gives me the velocity in m/s
            else
                finalDest = targetDestinationBackward.position;


            if (movingUp)
                limbTarget.position = Vector3.MoveTowards(limbTarget.position, midpoint, realMoveSpeed*Time.fixedDeltaTime);
            else
                limbTarget.position = Vector3.MoveTowards(limbTarget.position, finalDest, realMoveSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Finished!");
        moving = false;
    }

    private void OnDrawGizmos()
    {
        if (limbTarget != null)
            Gizmos.DrawWireSphere(limbTarget.position, 0.25f);
        if (targetDestinationForward != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetDestinationForward.position, 0.25f);
        }
        if (targetDestinationBackward != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetDestinationBackward.position, 0.25f);
        }
    }
}
