using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDDroneEnemy : MonoBehaviour
{
    public float moveSpeed = 10f;
    
    [Tooltip("UNUSED: If true, uses custom path, otherwise flies direct to base")]
    public bool usePath;
    Vector3 travelDir;
    Vector3 dest;
    private int targetIndex;
    private Vector3 acendPoint;
    private Vector3 acendDir;
    private bool acended = true;

    [Tooltip("Simple buffer to spread out drone spawns")]
    public Vector3 offset;
    //suicide drone stuff

    public GameObject explosiveObj;
    private Vector3 playerPos;
    
    //public float spinSpeed = 10f;

    private Rigidbody rb;

    //to  add 
    //Flying sound
    //engine cutoff sound
    //maybe some randomised torque to make it turn when falling 
    // Make it so that when it spawns if flys down a bit before flying to destination to make it look better
    private bool reachedDest;
    private bool hit;
    public LayerMask raycastIgnore;
    public LayerMask targetingMask;
    public float targetingRange;

    [Header("Audio")]
    public AudioSource dropSound;

    private CharacterControllerScript player;
    // Start is called before the first frame update
    void Start()
    {
        targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
        dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 15f;
        travelDir = dest - transform.position;
        travelDir.Normalize();

        rb = GetComponent<Rigidbody>();

        player = CharacterControllerScript.instance;
    }

    private void FixedUpdate()
    {
        if (!reachedDest)
        {
            if (TDPlayerBase.instance.buildings[targetIndex].buildingHealth <= 0)//current target dead
                targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
            

            if (player.health > 0)//target player
            {
                dest = player.transform.position;
                dest.y = transform.position.y; //essentially get player pos at current altitude
                if (Vector3.Distance(transform.position, dest) < 10f) //when 10m away from player, check sight
                    if (!CheckSight()) //if cant see then target bot or base
                    {
                        //put something here to target bots
                        dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 20f;
                    }
            }
            else //player dead target base building
                dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 20f;
            
            travelDir = dest - transform.position;
            travelDir.Normalize();
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        if (!reachedDest)
        {
            if (!acended)
            {
                if (Vector3.Distance(transform.position, acendPoint) > 0.5f)
                    transform.Translate(acendDir * moveSpeed * Time.deltaTime);
                else
                {
                    acended = true;
                    travelDir = (dest - transform.position).normalized;
                }
            }
            else if (Vector3.Distance(transform.position, dest) > 0.5f)
            {
                transform.position += (travelDir * moveSpeed * Time.deltaTime);
            }
            else
            {
                //Drop
                reachedDest = true;
                rb.isKinematic = false;
                rb.useGravity = true;
                if (dropSound != null)
                    dropSound.Play();
                //TDPlayerBase.instance.DamageBase(dps * Time.deltaTime, targetIndex);
            }
        }
    }

    public void FlyToDest(Transform point)
    {
        acended = false;
        acendPoint = point.position + new Vector3(Random.Range(-offset.x, offset.x), Random.Range(-offset.y, offset.y), Random.Range(-offset.z, offset.z));
        acendDir = acendPoint - transform.position;
        acendDir.Normalize();
        //dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 5f;
        //travelDir = dest - point.position;
        //travelDir.Normalize();
    }

    private void OnDrawGizmosSelected()
    {
        if (dest != null)
        {
            Gizmos.DrawSphere(dest, 1f);
            Gizmos.DrawLine(transform.position, transform.position + travelDir*Vector3.Distance(transform.position, dest));
            Vector3 testDir = dest - transform.position;
            Gizmos.DrawLine(transform.position, transform.position + testDir);
        }
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
        Vector3 pDir = player.pCam.transform.position - transform.position;
        RaycastHit rHit;
        if (Physics.Raycast(transform.position, pDir.normalized, out rHit, 50f, ~raycastIgnore))
        {
            if (rHit.transform.tag.Equals("Player"))
                return true;
            else
                return false;
        }


        return false;
    }
    
    /*
    private void FindBestTarget(bool checkPlayer)
    {
        float distToP;
        if (checkPlayer && CheckSight(player))
            distToP = Vector3.Distance(transform.position, player.position);
        else
            distToP = float.MaxValue;
        float minDist = float.MaxValue;
        int minDistIndex = -1;

        //update potential targets
        Collider[] tarCols = Physics.OverlapSphere(transform.position, range, physicsOverlayMask);
        potentialTargets = new Transform[tarCols.Length];
        for (int i = 0; i < tarCols.Length; i++)
            potentialTargets[i] = tarCols[i].transform;
        for (int i = 0; i < potentialTargets.Length; i++)
        {
            float checkDist = Vector3.Distance(transform.position, potentialTargets[i].position);
            if (checkDist < minDist)
            {
                if (CheckSight(potentialTargets[i]))
                {
                    minDist = checkDist;
                    minDistIndex = i;
                }
            }
        }
        if (checkPlayer)
        {
            if (minDist < distToP && minDist <= targetingRange)
                target = potentialTargets[minDistIndex];
            else if (distToP <= targetingRange && distToP <= minDist)
                target = player;
            else if (distToP > minDist)
                target = potentialTargets[minDistIndex];
            else
                target = null;
        }
        else
        {
            if (minDistIndex < 0) //no valid found
                target = null;
            else if (minDist <= targetingRange)
                target = potentialTargets[minDistIndex];
            else
                target = null;
        }

    }*/

}
