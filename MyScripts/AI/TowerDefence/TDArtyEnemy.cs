using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UniversalStats))]
[RequireComponent(typeof(NavMeshAgent))]
public class TDArtyEnemy : MonoBehaviour
{
    //[Header("Basic AI")]
    private NavMeshAgent navAgent;
    private int targetIndex;

    [Header("ArtilleryStuff")]
    public float range = 50f;
    public float fireRate = 10f;
    public bool useBurst;
    public int burstAmount = 5;
    private int burstCounter;
    public float burstCooldown = 5f;
    private bool coolingDown;
    [Tooltip("Make Sure this is an arty shell, not a raycast bullet")]
    public GameObject projectile;
    public Transform bulletSpawn;
    private Transform target;

    private float fireRatePerSecond;
    private UniversalStats unistats;
    private double lastShotTime;
    private enum TDArtyState {Walking, Firing, Waiting, Dead };
    private TDArtyState state;
    // Start is called before the first frame update
    void Start()
    {
        unistats = GetComponent<UniversalStats>();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.SetDestination(TowerDefenceWaveManager.instance.destination.position);
        targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
        //target = TDPlayerBase.instance.buildings[targetIndex].target;
        target = TDPlayerBase.instance.shootTarget;
        fireRatePerSecond = fireRate / 60f;
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
            if (TDPlayerBase.instance.buildings[targetIndex].buildingHealth <= 0)
            {//current target dead, update it
                targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
                //target = TDPlayerBase.instance.buildings[targetIndex].target;
            }

            if (Vector3.Distance(transform.position, TDPlayerBase.instance.buildings[targetIndex].building.position) > range) //out of range
                state = TDArtyState.Walking;
            else
                state = TDArtyState.Firing;
            
        }
        else
            state = TDArtyState.Dead;
    }

    private void Behaviour()
    {
        if(state != TDArtyState.Dead)
        {
            if(state == TDArtyState.Walking)
            {
                if (navAgent.destination != TDPlayerBase.instance.buildings[targetIndex].building.position)
                    navAgent.SetDestination(TDPlayerBase.instance.buildings[targetIndex].building.position);
            }
            else if(state == TDArtyState.Firing)
            {
                if (navAgent.destination != transform.position)
                    navAgent.SetDestination(transform.position);
                if (!useBurst)
                    Fire();
                else if (!coolingDown)
                    StartCoroutine(BurstFire());
            }
        }
    }

    private void Fire()
    {
        if (lastShotTime + (1f / fireRatePerSecond) < Time.timeAsDouble || useBurst)
        {
            GameObject proj = Instantiate(projectile, bulletSpawn.position, bulletSpawn.rotation);
            //set target
            proj.GetComponent<TDArtyShot>().target = target.position;

            lastShotTime = Time.timeAsDouble;
        }
    }

    private IEnumerator BurstFire()
    {
        if (!coolingDown)
        {
            coolingDown = true;
            burstCounter = burstAmount;

            while (burstCounter >= 0)
            {
                Fire();
                burstCounter--;
                yield return new WaitForSeconds(1f / fireRatePerSecond);
            }
            yield return new WaitForSeconds(burstCooldown);
            coolingDown = false;
        }
    }
}
