using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SImpleSuicideDroneAI : MonoBehaviour
{
    //maybe add sight check? or dont update player pos? idk
    public GameObject explosiveObj;
    private Vector3 playerPos;

    public float moveSpeed =10f;
    public float spinSpeed = 10f;

    private Rigidbody rb;

    //to  add 
    //Flying sound
    //engine cutoff sound
    //maybe some randomised torque to make it turn when falling 
    // Make it so that when it spawns if flys down a bit before flying to destination to make it look better
    private bool reachedDest;
    private bool hit;
    public LayerMask raycastIgnore;

    private bool useSpecialDest;
    private Vector3 specialDest;
    private Vector3 specialMoveRef;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerPos = CharacterControllerScript.instance.transform.position;
        playerPos.y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (useSpecialDest)
        {
            if (Vector3.Distance(transform.position, specialDest) < 0.1f)
                useSpecialDest = false;
            else
                transform.position = Vector3.SmoothDamp(transform.position, specialDest, ref specialMoveRef, 1f, moveSpeed, Time.deltaTime);
        }
        else
        {
            if (CheckSight()) //if can see player, go to player, otherwise position is not updated so it will go to last know position
            {
                playerPos.x = CharacterControllerScript.instance.transform.position.x;
                playerPos.z = CharacterControllerScript.instance.transform.position.z;
            }
            if (Vector3.Distance(transform.position, playerPos) > 0.25f && !reachedDest)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerPos, moveSpeed * Time.deltaTime);
                //Vector3 rot = Quaternion.LookRotation((playerPos - transform.position).normalized).eulerAngles;
                //transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, rot, spinSpeed*Time.fixedDeltaTime);
                
            }
            else if (!reachedDest)
            {
                reachedDest = true;
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
        transform.eulerAngles += Vector3.up * spinSpeed * Time.deltaTime;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (reachedDest && !hit)
        {
            if (!collision.transform.tag.Equals("Drone")) //doesnt hit another drone
            {
                hit = true;
                OnDeath(collision.GetContact(0).normal);
            }
        }
    }

    private void OnDeath(Vector3 normal)
    {
        GameObject exp = Instantiate(explosiveObj, transform.position, transform.rotation);
        exp.transform.up = normal;
        Destroy(gameObject, 0.02f);
    }

    private bool CheckSight()
    {
        Vector3 pDir = CharacterControllerScript.instance.pCam.transform.position - transform.position;
        RaycastHit rHit;
        if(Physics.Raycast(transform.position, pDir.normalized, out rHit, 150f, ~raycastIgnore))
        {
            if (rHit.transform.tag.Equals("Player"))
                return true;
            else
                return false;
        }


        return false;
    }

    public void GoToDest(Vector3 dest)
    {
        useSpecialDest = true;
        specialDest = dest;
        playerPos.y = dest.y;
    }

}
