using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDAreaBomber : MonoBehaviour
{

    [Header("DropStats")]
    public int dropAmount = 10;
    public float dropRatePS = 5f;

    [Tooltip("Make Sure this is an arty shell, not a raycast bullet")]
    public GameObject dropObj;
    public Transform dropSpawn;
    public float dropRadius;
    [Space]
    public float moveSpeed = 10f;


    private UniversalStats unistats;
    private double lastShotTime;
    //private int destIndex;
    private Vector3 dest;
    private bool reachedDest;
    private Transform tempDest; //Used to make ship move to another position before moving to firing position - e.g. make it fly up from its spawn point rather than flying directly to destination
    private bool reachedTempDest;
    private Vector3 spawnPos;
    private bool returningToSpawn;
    //private enum TDArtyState { Walking, Firing, Waiting, Dead };
    //private TDArtyState state;

    void Start()
    {
        unistats = GetComponent<UniversalStats>();

        //destIndex = TowerDefenceWaveManager.instance.GetRandomMissileZone();
        //if (destIndex >= 0) //random missile zones returns -1 when no zone is found
        //    dest = TowerDefenceWaveManager.instance.missileZones[destIndex];

        spawnPos = transform.position;


        dest = transform.position + transform.forward * 250f + transform.up*10f;
    }

    private void OnDestroy()
    {
        //if (destIndex >= 0)
        //    TowerDefenceWaveManager.instance.FreeMZ(destIndex);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateState();
        Behaviour();
        
    }

    private void UpdateState()
    {
        if (unistats.health > 0)//not dead
        {

            if (!reachedDest && dest != null && Vector3.Distance(transform.position, dest) < 0.5f)
                reachedDest = true;

            if (!reachedTempDest && tempDest != null && Vector3.Distance(transform.position, tempDest.position) < 0.5f)
                reachedTempDest = true;

        }
    }

    private void Behaviour()
    {
        if (unistats.health > 0)
        {
            //move to temp dest
            if (tempDest != null && !reachedTempDest)
            {
                transform.position = Vector3.MoveTowards(transform.position, tempDest.position, moveSpeed * Time.fixedDeltaTime);
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, tempDest.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

                transform.rotation = Quaternion.LookRotation(newDirection);
            }
            else if (!reachedDest)
            {
                transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.fixedDeltaTime);
                //Vector3 newDirection = Vector3.RotateTowards(transform.forward, dest.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

                //transform.rotation = Quaternion.LookRotation(newDirection);

                if (TowerDefenceWaveManager.instance.bomberZone.bounds.Contains(transform.position)) //reached dest is true
                    Fire();


            }
            else 
                Destroy(gameObject);
            
        }
    }

    private void Fire()
    {
        if (dropAmount>0 && (lastShotTime + (1f / dropRatePS) < Time.timeAsDouble))
        {
            GameObject proj = Instantiate(dropObj, dropSpawn.position + new Vector3(Random.Range(-dropRadius,dropRadius),0f, Random.Range(-dropRadius, dropRadius)), dropSpawn.rotation, TowerDefenceWaveManager.instance.transform);
            

            lastShotTime = Time.timeAsDouble;

            
            dropAmount--;
        }
    }

    public void FlyToDest(Transform point) //temp dest -assigned by wave spawner
    {
        tempDest = point;
    }

    private void ReturnToSpawn()
    {
        if (!returningToSpawn)
        { //reset temp dest bool so i can reuse it
            reachedTempDest = false;
            returningToSpawn = true;
        }

        if (tempDest != null && !reachedTempDest)
        {
            transform.position = Vector3.MoveTowards(transform.position, tempDest.position, moveSpeed * Time.fixedDeltaTime);
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, (transform.position - tempDest.position).normalized, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            //move to dest
            transform.position = Vector3.MoveTowards(transform.position, spawnPos, moveSpeed * Time.fixedDeltaTime);
            //dont need to rotate
            //Vector3 newDirection = Vector3.RotateTowards(transform.forward, spawnPos.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

            //transform.rotation = Quaternion.LookRotation(newDirection);
        }

        if (Vector3.Distance(transform.position, spawnPos) < 0.5f) //reached spawn - destory
            Destroy(gameObject);

        if (!reachedTempDest && tempDest != null && Vector3.Distance(transform.position, tempDest.position) < 0.5f) //reach temp dest - go back to spawn
            reachedTempDest = true;

    }
}
