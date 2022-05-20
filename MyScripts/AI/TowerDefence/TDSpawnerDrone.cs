using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDSpawnerDrone : MonoBehaviour
{
    public float speed = 10f;
    public GameObject objToSpawn;
    public TowerDefenceWaveManager.EnemyAimType objType;
    public Transform spawnPos;
    public int amountToSpawn = 10;
    public float spawnRatePerSecond = 1;
    private Transform dest;
    private int destIndex=-2;
    private Vector3 startPoint;
    private float deployTimer;
    private Vector3 realSpawnPos; //spawn pos that is actually used - is on navmesh
    private float checkRadius = 1f;
    private bool validSpawn;
    //state bools
    private bool arrived; //once arrived above dest, this si true and triggers decent
    private bool deploy; //landed, now delpoy
    private bool completedDeployment; //once all objs are deployed, this triggers accension
    private bool acended; //once accended after deploying, this triggers return to start

    private Vector3 travelDir = Vector3.zero;


    //temp dest stuff - assigned by spawner to make ship fly away from spawn
    private Transform tempDest;
    private bool tempDestReached;
    
    void Start()
    {
        //transform.parent = null; //prevents this being targetted by turrets - maybe dont do this, idk testing
        startPoint = transform.position;
        StartCoroutine(GetDest());
    }

    
    void Update()
    {
        if (dest != null &&(tempDest==null || tempDestReached))
        {
            travelDir = Vector3.zero;
            if (acended)
            {
                //return to start
                TravelToStart();
            }
            else if (completedDeployment)
            {
                //accend 
                Accend();
            }
            else if (deploy)
            {
                DeployObj();
            }
            else if (arrived)
            {
                //decend
                TravelToDest(true);
            }
            else
            {
                //travel to dest
                TravelToDest(false);
            }
        }
        else //temp dest behaviour
        {
            if (!tempDestReached && Vector3.Distance(tempDest.position, transform.position) < 0.25f)
                tempDestReached = true;

            if (tempDestReached && dest == null) //temp dest reached but dest still not set
                travelDir = Vector3.zero;
        }
        
        transform.Translate(travelDir * speed*Time.deltaTime, Space.World);
    }

    private IEnumerator GetDest()
    {
        while (dest == null)
        {
            destIndex = TowerDefenceWaveManager.instance.GetRandomLandingZone();
            if (destIndex >= 0)//valid point given
            {
                dest = TowerDefenceWaveManager.instance.landingZones[destIndex];
                break;
            }
            yield return new WaitForSeconds(1f); //not valid point found, wait a second and try again
        }
    }

    private void TravelToDest(bool decend)
    {
        travelDir = dest.position - transform.position;
        
        if(!decend)
            travelDir.y = 0;//dont decend at 
        else //while decending rotate to dest rot
        {
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dest.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        travelDir.Normalize();
        Vector3 tdest = dest.position;
        tdest.y = transform.position.y;
        if (!arrived && Vector3.Distance(transform.position, tdest) < 0.25f) //check if travel dir is so small as to basically be nothing - arrived above dst so decend
        {
            arrived = true;
            return;
        }

        if(transform.position.y<= dest.position.y)
        {
            deploy = true;
        }
    }

    private void DeployObj()
    {
        if (!validSpawn)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(spawnPos.position, out navHit, checkRadius, NavMesh.AllAreas))
            {
                realSpawnPos = navHit.position;
                validSpawn = true;
            }
            else
            {
                Debug.Log("Increasing Radius");
                checkRadius++; //increase radius
            }
        }
        else
        {
            if (deployTimer <= 0 && amountToSpawn > 0)
            {
                //spawn obj
                if (objType == TowerDefenceWaveManager.EnemyAimType.Normal)
                    Instantiate(objToSpawn, realSpawnPos, spawnPos.rotation, TowerDefenceWaveManager.instance.transform);
                else if (objType == TowerDefenceWaveManager.EnemyAimType.Air)
                    Instantiate(objToSpawn, realSpawnPos, spawnPos.rotation, TowerDefenceWaveManager.instance.airEnemyParent);
                amountToSpawn--;
                deployTimer = 1f / spawnRatePerSecond;
            }
            else if (amountToSpawn <= 0)
                completedDeployment = true;
            else
                deployTimer -= Time.deltaTime;
        }
    }

    private void Accend()
    {
        if (tempDest == null)
        {
            if (transform.position.y < startPoint.y)
                travelDir = Vector3.up;
            else
                acended = true;
        }
        else
        {
            if (transform.position.y < tempDest.position.y)
                travelDir = Vector3.up;
            else
                acended = true;
        }
    }

    private void TravelToStart()
    {
        if (tempDest == null) //if no temp dest, use start
        {
            if (Vector3.Distance(startPoint, transform.position) > 0.5f)
                travelDir = (startPoint - transform.position).normalized;
            else
            {
                TowerDefenceWaveManager.instance.FreeLZ(destIndex);
                Destroy(gameObject);
            }
        }
        else
        {
            //otherwise, use temp dest coz it has the better height
            if (Vector3.Distance(tempDest.position, transform.position) > 0.5f)
                travelDir = (tempDest.position - transform.position).normalized;
            else
            {
                TowerDefenceWaveManager.instance.FreeLZ(destIndex);
                Destroy(gameObject);
            }
        }
    }

    public void FlyToDest(Transform point) //temp dest -assigned by wave spawner
    {
        acended = false;
        tempDest = point;
        travelDir = tempDest.position - transform.position;
        travelDir.Normalize();
        //startPoint = tempDest.position;
        //dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 5f;
        //travelDir = dest - point.position;
        //travelDir.Normalize();
    }

    private void OnDestroy()
    {
        if(destIndex>=0)
            TowerDefenceWaveManager.instance.FreeLZ(destIndex);
    }

}
