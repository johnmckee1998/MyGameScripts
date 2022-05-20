using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDArtyShot : MonoBehaviour
{
    public Vector3 target;
    public float speed = 200f;
    public float rotSpeed = 180f;
    public float directhitDamage = 100f;
    public GameObject explosionObj;
    public LayerMask raycastIgnore;
    public Transform trail;
    private float incrementDist;
    private bool hitSomething;
    private Vector3 prevRot;
    public float deviation;
    private float deviationTimer;
    private Vector3 deviationVector;
    private float alt = 100f;
    private bool reachedAlt;
    private Vector3 startPos;
    //private Vector3 midpoint;
    //private Vector3 alt;
    //private float a; //the a value of y= -a(x-c)^2 +b
    //private Vector3 incrementPoint;
    // Start is called before the first frame update
    void Start()
    {
        prevRot = transform.eulerAngles;
        startPos = transform.position;
        //midpoint = (startPos + target) / 2f;
        //alt = midpoint + Vector3.up*50f; //assume always having alt of 50
        //a = alt.y / Mathf.Pow((Vector3.Distance(startPos, target)),2f);
    }

    // Update is called once per frame
    void Update()
    {
        Increment();
    }

    private void Increment()
    {
        incrementDist = speed * Time.deltaTime;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, incrementDist, ~raycastIgnore)) //Hit
        {
            try
            {
                hit.collider.gameObject.SendMessage("HitByBullet", directhitDamage, SendMessageOptions.DontRequireReceiver);
            }
            catch
            {

            }
            Instantiate(explosionObj, transform.position, Quaternion.LookRotation(hit.transform.forward, hit.normal));
            if (trail != null)
            {
                trail.parent = null;
                Destroy(trail.gameObject, 2f);
            }
            Destroy(gameObject, 0.01f);
        }
        else //Nothing Hit
        {
            if (transform.position.y >= alt + startPos.y)
                reachedAlt = true;

            if (reachedAlt)
                UpdatePosition();
            else
                UpdatePosition2();
        }
    }

    private void UpdatePosition()
    {
        //Vector3 destRot = Quaternion.LookRotation((target - transform.position).normalized).eulerAngles;
        Vector3 targetDir = ((target + RandomDeviation()) - transform.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDir, rotSpeed*Mathf.Deg2Rad*Time.deltaTime, 0.0f);

        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.position += incrementDist * transform.forward;
        //transform.eulerAngles = destRot; //assign to get worldspace rotation
        //Convert to Negative
        /*
        if (destRot.x > 180)
            destRot.x -= 360f;
        if (destRot.y > 180)
            destRot.y -= 360f;
        if (destRot.z > 180)
            destRot.z -= 360f;
        Vector3 newRot = new Vector3(prevRot.x, prevRot.y, prevRot.z) //aparently i can delcare vectors like this? idk it just suggested it
        {
            x = Mathf.MoveTowards(prevRot.x, destRot.x, rotSpeed * Time.deltaTime),
            y = Mathf.MoveTowards(prevRot.y, destRot.y, rotSpeed * Time.deltaTime),
            z = Mathf.MoveTowards(prevRot.z, destRot.z, rotSpeed * Time.deltaTime)
        };*/
        //Vector3.RotateTowards
        /*
        transform.eulerAngles = destRot;
        float tempPitch = transform.eulerAngles.x;
        float tempYaw = transform.eulerAngles.y;
        if (tempPitch > 120) //shouldnt realistically pass 90 degree rotation unless going negative
            tempPitch -= 360f;
        if (tempYaw > 120) //shouldnt realistically pass 120 degree rotation unless going negative
            tempYaw -= 360f;


        prevRot.x = Mathf.MoveTowards(prevRot.x, tempPitch, rotSpeed * Time.deltaTime); //use localspace rotation
        prevRot.y = Mathf.MoveTowards(prevRot.y, tempYaw, rotSpeed * Time.deltaTime);

        






        transform.eulerAngles = prevRot;
        //prevRot = newRot;
        transform.position += incrementDist * transform.forward;
        */

        

    }

    private Vector3 RandomDeviation()
    {
        if (deviation != 0)
        {
            if (deviationTimer <= 0) {
                deviationTimer = 0.25f;
                deviationVector = new Vector3(Random.Range(0, deviation), Random.Range(0, deviation), Random.Range(0, deviation));
                //randomly flip them to positive or negative
                deviationVector.x *= Random.Range(0 , 2)*2 - 1;
                deviationVector.y *= Random.Range(0 , 2)*2 - 1;
                deviationVector.z *= Random.Range(0 , 2)*2 - 1;
            }
            return deviationVector;
        }
        else
        {
            deviationTimer -= Time.deltaTime;
            return Vector3.zero;
        }
    }

    private void UpdatePosition2()
    {
        Vector3 targetDir = ((startPos + Vector3.up*alt + RandomDeviation()/2f) - transform.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDir, rotSpeed * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.position += incrementDist * transform.forward;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position+((target-transform.position).normalized));
    }
}
