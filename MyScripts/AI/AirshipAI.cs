using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirshipAI : MonoBehaviour
{
    public enum Behaviour {FixedPatrol, RandomPatrol, SearchAndDestroy }
    //fixed patrol - patrol back and forth between fixed points,
    //random patrol - patrol randomly within area,
    //search and destroy - patrol randomly (maybe react to player shots?) and chase player once spotted
    public Behaviour behaviourType;


    public float forwardMoveSpeed = 1f;
    public float turnRate = 1f;

    [Header("RandomPatrolStuff")]
    public Vector3 patrolCenter;
    public float xPatrolRadius = 1f;
    public float zPatrolRadius = 1f;
    //public Vector3 patrolMinXYZ;
    //public Vector3 patrolMaxXYZ;

    private Vector3 destination;

    private bool destInFront;
    // Start is called before the first frame update
    void Start()
    {
        RandomiseDestination();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (behaviourType == Behaviour.FixedPatrol)
            FixedPatrol();
        else if (behaviourType == Behaviour.RandomPatrol)
            RandomPatrol();
        else if (behaviourType == Behaviour.SearchAndDestroy)
            SearchAndDestroy();
    }

    private void FixedPatrol()
    {

    }

    private void RandomPatrol()
    {
        if (Vector3.Distance(transform.position, destination) < 5f) //at destination
            RandomiseDestination();


        UpdateMovement();
    }

    private void SearchAndDestroy()
    {

    }

    private void UpdateMovement() //improvements - make it slow down if needed to turn sharply, smooth turning not constant rate
    {
        Quaternion targetRotation = Quaternion.LookRotation(destination - transform.position);

        if(destInFront)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate*Time.fixedDeltaTime);
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate*2f * Time.fixedDeltaTime);

        if (destInFront)
            transform.Translate(transform.forward * forwardMoveSpeed * Time.fixedDeltaTime, Space.World);
        else
            transform.Translate(transform.forward * forwardMoveSpeed/2f * Time.fixedDeltaTime, Space.World);
    }

    private void RandomiseDestination()
    {
        destination = patrolCenter + new Vector3(Random.Range(-xPatrolRadius, xPatrolRadius), 0, Random.Range(-zPatrolRadius, zPatrolRadius) );
        if (Vector3.Distance(transform.position, destination) < xPatrolRadius / 2 || Vector3.Distance(transform.position, destination) < zPatrolRadius / 2) //if less than radius/2 away get a new point
            RandomiseDestination();
    }

    private void OnDrawGizmosSelected()
    {
        if(behaviourType == Behaviour.RandomPatrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(patrolCenter, new Vector3(xPatrolRadius*2f, 1f, zPatrolRadius*2f));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position,transform.position+ transform.forward);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(destination, 1);
        }
    }

    private void CheckInFront()
    {
        Vector3 relativePoint = transform.InverseTransformPoint(destination);
        if (relativePoint.z < 0.0)
            destInFront = false;
        else if (relativePoint.z > 0.0)
            destInFront = true;
    }

    private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) //determines if target is left (positive), right (negative), or forward (0) of fwd vector
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

}
