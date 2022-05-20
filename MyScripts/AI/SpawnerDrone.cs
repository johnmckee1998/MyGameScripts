using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerDrone : MonoBehaviour
{
    //SHOULD MAKE THIS AND LARGE DRONE INHERIT FROM SAME PARENT SCRIPT - like a droneBaseAi os smthing
    //Todo - add investiagting areas (used for investigating sound or high detection)
    //add a particle effect when spawning drone?
    // add sound to drone spawn

    [Header("Shooting Settings")]
    //private bool inShotRange = false;
    //private BotShooting shootScript;
    private Vector3 playerTravelDir;
    //private Transform gunTransform;
    private float prevHealth;
    //public float ShootDist = 35f;

    private bool gunDestroyed = false;
    private Vector3 prevPlayerPos;
    private Vector3 lastSeenPlayerPos;

    [Header("Ai Settings")]
    public Vector2 minMaxX;
    public Vector2 minMaxZ;
    public Vector2 minMaxPatrolDist;
    public float moveSpeed = 10f;
    public float detectRate = 50f;
    public float detectDecay = 25f;
    public Vector2 waitTime;

    [Header("Spawner Settings")]
    public Transform spawnObject;
    public Transform spawnPoint;
    public Transform destPos;
    public float timeBetweenSpawns = 1f;
    public int spawnCount = 4;
    private bool spawning;

    private Vector3 patrolPoint;
    private Vector3 patrolCenter;

    private float detection;
    private bool investigating;
    private bool waiting;

    private Vector3 patrolRef;
    private Vector3 rotationRef;

    private bool seePlayer;

    private UniversalStats uniStats;
    private void Start()
    {


        patrolCenter = new Vector3((minMaxX.x + minMaxX.y) / 2, transform.position.y, (minMaxZ.x + minMaxZ.y) / 2);


        transform.position = patrolCenter + new Vector3(Random.Range(-minMaxPatrolDist.y, minMaxPatrolDist.y), 0f, Random.Range(-minMaxPatrolDist.y, minMaxPatrolDist.y));
        transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);


        uniStats = GetComponent<UniversalStats>();

        NewPatrolPoint();
    }


    private void FixedUpdate()
    {
        if (uniStats.health>0)
        {
            if (seePlayer)
                detection += detectRate * Time.fixedDeltaTime;

            if (detection >= 20)
                Spawn(); //improvement - make a spawn amount variable - rather than constantly spawning when detected, make it so that on detect it spawns x amount, and then after a while spawn more
            if (!investigating)
                Patrol();


            detection -= detectDecay * Time.fixedDeltaTime;
            detection = Mathf.Clamp(detection, 0f, 100f);
        }
    }


    public void GunDestroyed(bool b)
    {
        gunDestroyed = b;
    }

    public void DetectPlayer(bool b)
    {
        seePlayer = b;
    }

    private void Patrol()
    {
        if (!waiting)
        {
            if (Vector3.Distance(transform.position, patrolPoint) < 0.5f)
                StartCoroutine(WaitPatrol());

            //rotate to patrol point
            if (!CheckInFront(patrolPoint))
                RotateToTarget(patrolPoint);
            //if looking at point, start moving there (unless actively spawning)
            else if(!spawning)
                transform.position = Vector3.SmoothDamp(transform.position, patrolPoint, ref patrolRef, Vector3.Distance(transform.position, patrolPoint) / moveSpeed, moveSpeed, Time.fixedDeltaTime);
        }
    }


    public void InvestigateArea(Vector3 area)
    {

    }

    private void RotateToTarget(Vector3 target)
    {
        //Debug.Log("Rotating");
        Vector3 targetDir = target - transform.position;
        targetDir.Normalize();

        transform.eulerAngles = Vector3.SmoothDamp(transform.eulerAngles, Quaternion.LookRotation(targetDir).eulerAngles, ref rotationRef, 2.5f, 50f, Time.fixedDeltaTime);
    }

    private bool CheckInFront(Vector3 Pos) //Checks if given position is within +- 1 degrees of tranform forward
    {
        Vector3 posDir = new Vector3(Pos.x, transform.position.y, Pos.z) - transform.position;
        float angleToPos = Vector3.Angle(posDir.normalized, transform.forward);
        if (angleToPos <= 2 && angleToPos >= -2)
            return true;
        return false;
    }

    private void NewPatrolPoint()
    {
        float xPos = Random.Range(minMaxPatrolDist.x, minMaxPatrolDist.y);
        float zPos = Random.Range(minMaxPatrolDist.x, minMaxPatrolDist.y);
        xPos *= Random.Range(0, 2) * 2 - 1; //randomly set pos or neg
        zPos *= Random.Range(0, 2) * 2 - 1;

        patrolPoint = transform.position + new Vector3(xPos, 0f, zPos);

        patrolPoint.x = Mathf.Clamp(patrolPoint.x, minMaxX.x, minMaxX.y); //make sure they are not outside boundaries
        patrolPoint.z = Mathf.Clamp(patrolPoint.z, minMaxZ.x, minMaxZ.y);
    }

    IEnumerator WaitPatrol()
    {
        waiting = true;

        yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));

        NewPatrolPoint();

        waiting = false;
    }


    private void OnDrawGizmosSelected()
    {
        Vector3 min = new Vector3(minMaxX.x, transform.position.y, minMaxZ.x);
        Vector3 max = new Vector3(minMaxX.y, transform.position.y, minMaxZ.y);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(min, max);
        Gizmos.DrawLine(min, new Vector3(minMaxX.x, transform.position.y, minMaxZ.y)); //00 -> 01
        Gizmos.DrawLine(min, new Vector3(minMaxX.y, transform.position.y, minMaxZ.x)); //00 -> 10
        Gizmos.DrawLine(new Vector3(minMaxX.y, transform.position.y, minMaxZ.x), max); //10 -> 11
        Gizmos.DrawLine(new Vector3(minMaxX.x, transform.position.y, minMaxZ.y), max); //01 -> 11

        patrolCenter = new Vector3((minMaxX.x + minMaxX.y) / 2, transform.position.y, (minMaxZ.x + minMaxZ.y) / 2);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(patrolCenter, 1f);
    }

    public float GetDetection()
    {
        return detection;
    }

    private void Spawn()
    {
        if(!spawning)
        {
            StartCoroutine(SpawnObjects());
            //Instantiate(spawnObject, spawnPoint.position, spawnPoint.rotation);
        }
    }

    IEnumerator SpawnObjects()
    {
        spawning = true;
        int leftToSpawn = spawnCount;
        while (leftToSpawn > 0)
        {
            Transform spawned = Instantiate(spawnObject, spawnPoint.position, spawnPoint.rotation);
            try
            {
                spawned.gameObject.SendMessage("GoToDest", destPos.position, SendMessageOptions.DontRequireReceiver);
            }
            catch
            {

            }
            leftToSpawn--;
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        spawning = false;
    }
}
