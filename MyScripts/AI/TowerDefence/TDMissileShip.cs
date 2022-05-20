using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniversalStats))]
public class TDMissileShip : MonoBehaviour
{
    private int targetIndex;

    [Header("ArtilleryStuff")]
    public float speed = 10f;
    public float fireRate = 10f;
    public bool useBurst;
    public int burstAmount = 5;

    private int burstCounter;

    public float burstCooldown = 5f;

    private bool coolingDown;

    public bool limitedAmmo;
    public int ammoPool = 10;
    [Tooltip("Make Sure this is an arty shell, not a raycast bullet")]
    public GameObject projectile;
    public Transform bulletSpawn;
    private Transform target;

    private float fireRatePerSecond;
    private UniversalStats unistats;
    private double lastShotTime;
    private int destIndex;
    private Transform dest;
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
        targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
        //target = TDPlayerBase.instance.buildings[targetIndex].target;
        target = TDPlayerBase.instance.shootTarget;
        fireRatePerSecond = fireRate / 60f;
        destIndex = TowerDefenceWaveManager.instance.GetRandomMissileZone();
        if(destIndex>=0) //random missile zones returns -1 when no zone is found
            dest = TowerDefenceWaveManager.instance.missileZones[destIndex];
        
        spawnPos = transform.position;
    }

    private void OnDestroy()
    {
        if(destIndex>=0)
            TowerDefenceWaveManager.instance.FreeMZ(destIndex);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (limitedAmmo && ammoPool <= 0)
            ReturnToSpawn();
        else
        {
            UpdateState();
            Behaviour();
        }
    }

    private void UpdateState()
    {
        if (unistats.health > 0)//not dead
        {
            if (TDPlayerBase.instance.buildings[targetIndex].buildingHealth <= 0)
            {//current target dead, update it //NOT USED
                targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
                //target = TDPlayerBase.instance.buildings[targetIndex].target;
            }

            if (!reachedDest && dest!=null && Vector3.Distance(transform.position, dest.position)<0.5f && destIndex>=0) 
                reachedDest = true;

            if (!reachedTempDest && tempDest!=null && Vector3.Distance(transform.position, tempDest.position) < 0.5f) 
                reachedTempDest = true;

        }
    }

    private void Behaviour()
    {
        if (unistats.health>0)
        {
            //Update Target
            if (CharacterControllerScript.instance.health > 0)//when player is alive, target them
                target = CharacterControllerScript.instance.transform;
            else
                target = TDPlayerBase.instance.shootTarget;

            //move to temp dest
            if (tempDest!=null && !reachedTempDest)
            {
                transform.position = Vector3.MoveTowards(transform.position, tempDest.position, speed * Time.fixedDeltaTime);
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, tempDest.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

                transform.rotation = Quaternion.LookRotation(newDirection);
            }
            else if (!reachedDest)
            {
                //move to dest
                if (destIndex >= 0) 
                {
                    transform.position = Vector3.MoveTowards(transform.position, dest.position, speed * Time.fixedDeltaTime);
                    Vector3 newDirection = Vector3.RotateTowards(transform.forward, dest.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

                    transform.rotation = Quaternion.LookRotation(newDirection);
                }
                else //If no destination has been found, keep trying
                {
                    destIndex = TowerDefenceWaveManager.instance.GetRandomMissileZone();
                    if (destIndex >= 0) //random missile zones returns -1 when no zone is found
                        dest = TowerDefenceWaveManager.instance.missileZones[destIndex];
                }

            }
            else 
            {
                //stay still
                if (!useBurst)
                    Fire();
                else if (!coolingDown)
                    StartCoroutine(BurstFire());
            }
        }
    }

    private void Fire()
    {
        if ((!limitedAmmo || ammoPool>0) && (lastShotTime + (1f / fireRatePerSecond) < Time.timeAsDouble || useBurst))
        {
            GameObject proj = Instantiate(projectile, bulletSpawn.position, bulletSpawn.rotation);
            //set target
            proj.GetComponent<TDArtyShot>().target = target.position;

            lastShotTime = Time.timeAsDouble;

            if (limitedAmmo)
                ammoPool--;
        }
    }

    private IEnumerator BurstFire()
    {
        if (!coolingDown)
        {
            coolingDown = true;
            burstCounter = burstAmount;

            while (burstCounter > 0)
            {
                Fire();
                burstCounter--;
                yield return new WaitForSeconds(1f / fireRatePerSecond);
            }
            yield return new WaitForSeconds(burstCooldown);
            coolingDown = false;
        }
    }

    public void FlyToDest(Transform point) //temp dest -assigned by wave spawner
    {
        tempDest = point;
        
        //startPoint = tempDest.position;
        //dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 5f;
        //travelDir = dest - point.position;
        //travelDir.Normalize();
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
            transform.position = Vector3.MoveTowards(transform.position, tempDest.position, speed * Time.fixedDeltaTime);
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, (transform.position-tempDest.position).normalized, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            //move to dest
            transform.position = Vector3.MoveTowards(transform.position, spawnPos, speed * Time.fixedDeltaTime);
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
