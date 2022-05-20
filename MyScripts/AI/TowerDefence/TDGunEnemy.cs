using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(UniversalStats))]
public class TDGunEnemy : MonoBehaviour
{
    // *********************************** Add a sight check so that enemy only shoots at base when it can see it 
    public BotShooting shootScript;
    public float range = 50f;
    public Transform gunTransform;
    public Transform sightTransform;
    public LayerMask rayCastIgnore;
    [Space]
    [Tooltip("Minimum amount of time spent walking around without a target before switching to target the base")]
    public float baseTargetingTime = 120f;
    [Space]
    public bool useTestPriority;
    public LayerMask sightLayers;
    protected UniversalStats[] nearTargets;

    protected bool attackingBase;
    protected NavMeshAgent navmeshAgent;
    //protected Vector3 prevPlayerPos;
    //protected bool surpressing;
    //protected bool inRange;
    protected bool seePlayer;
    protected UniversalStats unistats;
    protected Transform target;
    protected UniversalStats tarStats;

    protected bool seeTarget;
    protected bool inRange;

    protected int baseIndex;

    protected enum TargetOrder {Close, Far, Weak, Strong, Player, Base };

    protected TargetOrder targetingOrder;

    protected bool wandering;
    protected bool prevWanderStatus;

    //wandering stuff
    protected Vector3 currentDest;
    protected Bounds bounds;

    protected void Start()
    {
        navmeshAgent = GetComponent<NavMeshAgent>();
        unistats = GetComponent<UniversalStats>();

        baseIndex = TDPlayerBase.instance.GetClosestBase(transform.position);


        int randomTargetingSelect = Random.Range(0, 5);
        switch (randomTargetingSelect)
        {
            case 0:
                targetingOrder = TargetOrder.Close;
                break;
            case 1:
                targetingOrder = TargetOrder.Far;
                break;
            case 2:
                targetingOrder = TargetOrder.Weak;
                break;
            case 3:
                targetingOrder = TargetOrder.Strong;
                break;
            case 4:
                targetingOrder = TargetOrder.Player;
                break;
                //dont set one for base - should switch to base targeting after time
        }

        UpdateTarget();
        //target = TowerDefenceWaveManager.instance.GetClosestPlayerTarget(transform.position);
        //tarStats = target.GetComponent<UniversalStats>();
        StartCoroutine(DestinationUpdate());

        bounds = TowerDefenceWaveManager.instance.bomberZone.bounds;
    }

    protected void FixedUpdate()
    {
        if(unistats.health>0)
            DetermineBehaviour();

        prevWanderStatus = wandering;
    }

    protected void UpdateTarget()
    {
        //targetting shouldnt use get player target all the time, it should have a collider that reports what targets are nearby and chooses one of them, if none avaliable then use get random or nearest target
        //could have a collider that sends a message when the player or a bot enters it 

        //Targetting idea: have different modes -> player/bots/base, or more like turrets strong/weak/close/far/base, and then pick a targeting type at random. 
        //Could also have it so that if a bot is shot at by the player, then target is overrided are now the player is targetted 

        if(targetingOrder == TargetOrder.Player)
        {
            if (CharacterControllerScript.instance.health > 0)
            {
                wandering = false;
                target = CharacterControllerScript.instance.transform;
                return;
            }
            target = null;
            wandering = true;
            return;
        }

        if(targetingOrder == TargetOrder.Base)
        {
            //$$$ TODO
            baseIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
            //add base health check
            target = TDPlayerBase.instance.buildings[baseIndex].building;
            //tarStats = TDPlayerBase.instance.buildings[baseIndex]
            return;
        }


        if ((tarStats != null && tarStats.health > 0)) //target is still valid, dont bother updating
            return;

        Transform targetParent = TowerDefenceWaveManager.instance.friendlyAIParent;

        if (targetParent.childCount > 0)//if targets are avaliable
        {
            float closestDist = float.MaxValue;
            float farthestDistance = -range * 2f - 1f;
            float highestHealth = 0f;
            float lowestHealth = float.MaxValue;
            bool foundTarget = false;
            int targetIndex = 0;
            UniversalStats tempStats;
            for (int i = 0; i < targetParent.childCount; i++)
            {
                tempStats = targetParent.GetChild(i).GetComponent<UniversalStats>();
                if (tempStats.health > 0) //only consider if it is alive
                {
                    //Debug.Log("InBounds");

                    float curIDist = Vector3.Distance(transform.position, targetParent.GetChild(i).position);

                    if (!useTestPriority || curIDist<=range) //only consider it if within range
                    {
                        if (targetingOrder == TargetOrder.Close)
                        {
                            if (closestDist > curIDist)
                            {
                                foundTarget = true;
                                targetIndex = i;
                                closestDist = curIDist;
                            }
                        }
                        else if (targetingOrder == TargetOrder.Far)
                        {
                            if (farthestDistance < curIDist)
                            {
                                foundTarget = true;
                                targetIndex = i;
                                farthestDistance = curIDist;
                            }
                        }
                        else if(targetingOrder == TargetOrder.Weak)
                        {
                            //UniversalStats uni = targetParent.GetChild(i).GetComponent<UniversalStats>();
                            if (tempStats.health < lowestHealth)
                            {
                                foundTarget = true;
                                targetIndex = i;
                                lowestHealth = tempStats.health;
                            }
                        }
                        else //Must be strong targetting if reaches this point
                        {
                            //UniversalStats uni = targetParent.GetChild(i).GetComponent<UniversalStats>();
                            if (tempStats.health > highestHealth)
                            {
                                foundTarget = true;
                                targetIndex = i;
                                highestHealth = tempStats.health;
                            }
                        }
                    }

                }
            }
            if (!foundTarget) //no point is found within range, so dont target anyone
            {
                target = null;
                wandering = true;
            }
            else
            {
                wandering = false;
                target = targetParent.GetChild(targetIndex);
                tarStats = target.GetComponent<UniversalStats>();
            }

        }
        else
            target = null;
        


        
    }


    protected void DetermineBehaviour() 
    {
        
        UpdateTarget();

        if(transform!=null && target!=null)
            if(Vector3.Distance(transform.position, target.position) <= range)
            {
                inRange = true;
            }

        CheckSight();

        if (target!=null && target.transform == CharacterControllerScript.instance.transform) //target is player
        {
            
            if (PlayerRespawnManager.instance.IsRespawning())//if respawning
            {
                //target = TowerDefenceWaveManager.instance.GetRandomPlayerTarget();
                seeTarget = false; //dont see, dont shoot
                shootScript.shoot = false;
                //player isnt alive
            }
        }

        if (inRange && seeTarget)
            ShootTarget();
        else
            shootScript.shoot = false;

        //Debug.Log("Sight: " + seeTarget + " Range: " + inRange);

        /* OLD CODE
        if (TDPlayerBase.instance.buildings[targetIndex].buildingHealth <= 0)//current target dead
            targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);

        if (PlayerRespawnManager.instance.IsRespawning())
        {
            seePlayer = false;
            shootScript.shoot = false;
            //player isnt alive
        }
        //if (surpressing)
        //{
        //    navmeshAgent.destination = transform.position;
        //    ShootPlayer(prevPlayerPos);
        //}


        if (!seePlayer && Vector3.Distance(transform.position, TDPlayerBase.instance.buildings[targetIndex].building.position) < range)
        {
            SetDest(transform.position);
            ShootTarget(TDPlayerBase.instance.buildings[targetIndex].target.position); //not the best name for this function, it really just shoots given target
        }
        else if (!seePlayer || Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position)>range) //if not in range, dont affect wander dest 
        {
            //transform.LookAt(new Vector3(CharacterControllerScript.instance.transform.position.x, transform.position.y, CharacterControllerScript.instance.transform.position.z) );//+ playerTravelDir
            //navmeshAgent.destination = CharacterControllerScript.instance.transform.position;
            //not in range, keep walking to base
            shootScript.shoot = false;
            SetDest(TDPlayerBase.instance.buildings[targetIndex].building.position);
        }
        else
        {//In shoot range - change wanderdest and shoot

            SetDest(transform.position);
            ShootTarget(CharacterControllerScript.instance.pCam.transform.position);
        }
            
        */
    }

    protected void SetDest(Vector3 pos)
    {
        if (navmeshAgent.destination != pos)//prvents repeatedly setting same dest and calculating route
            navmeshAgent.destination = pos;
    }

    public void PlayerSight(bool b) // depreciated - was used when i tried out the senor tookit fov collider
    {
        seePlayer = b;
    }

    protected virtual void ShootTarget()
    {
        Vector3 shootPos;
        if (tarStats != null && tarStats.targetPos != null)
            shootPos = tarStats.targetPos.position;
        else if (targetingOrder == TargetOrder.Player)//player
            shootPos = CharacterControllerScript.instance.pCam.transform.position;
        else //assume its the base at this point
            shootPos = TDPlayerBase.instance.buildings[baseIndex].target.position;
        Quaternion lookDir = Quaternion.LookRotation((shootPos - transform.position).normalized);
        lookDir.x = 0;
        lookDir.z = 0;
        transform.rotation = lookDir;
        //point gun 
        shootScript.useAimAssist = true;
        shootScript.aimAssistTarget = shootPos;

        shootScript.shoot = true;//shoot
    }

    protected IEnumerator DestinationUpdate()
    {
        while (unistats.health > 0)
        {
            if ((!seeTarget || !inRange) && target != null)
                SetDest(target.position);
            else if (target != null)
                SetDest(transform.position); //stand still when inrange and seetarget and valid target
            else //no valid target
                Wander();
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    protected void CheckSight()
    {
        if (target != null && transform!=null)
        {
            RaycastHit rHit;
            Vector3 tarPos; //temp position used for targetting - as player needs to use cam pos
            if (target.transform.IsChildOf(CharacterControllerScript.instance.transform))
                tarPos = CharacterControllerScript.instance.pCam.transform.position - Vector3.up / 2f;
            else
            {
                UniversalStats uStats = target.GetComponent<UniversalStats>();
                if (uStats != null && uStats.targetPos != null)
                    tarPos = uStats.targetPos.position;
                else
                    tarPos = target.position;
            }
            if (Physics.Raycast(sightTransform.position, (tarPos - sightTransform.position).normalized, out rHit, range, ~rayCastIgnore))
            {
                if (rHit.transform.IsChildOf(target))//hit target
                    seeTarget = true;
                else
                    seeTarget = false;

            }
            else
            {
                seeTarget = false;
            }
        }
        else
            seeTarget = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(target.position, 1f);
        }
    }

    protected void Wander()
    {
        baseTargetingTime -= Time.deltaTime * Time.timeScale; //reduce targeting time while wandering, when 0 target base
        if (baseTargetingTime <= 0)
        {
            targetingOrder = TargetOrder.Base;
            return;
        }

        //if still time left, find a wander point
        if (navmeshAgent.remainingDistance > 0.5f && wandering && prevWanderStatus)
            return; //already got a dest, dont bother with a new one (also checks if it was previously already wandering. if prev is false that measn it has just switched, so get a new dest)

        NewWanderDest(); //if remaining dist is less than 0.5, get a new dest
    }

    protected void NewWanderDest()
    {
        currentDest = new Vector3(
        Random.Range(bounds.min.x, bounds.max.x),
        transform.position.y,
        Random.Range(bounds.min.z, bounds.max.z));
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(currentDest, out navHit, 3f, NavMesh.AllAreas))
        {
            currentDest = navHit.position;
            SetDest(currentDest);
            return;
        }
    }

    protected void UpdateTargetList()
    {
        //this is sort of done by the range check in update target - if it only considers enemies in range, then it is basically using a list of enemies nearby 
        //overlap shpere with radius range
        //store bots within range and use them in targeting check
        Collider[] tempEnemies;

        tempEnemies = Physics.OverlapSphere(transform.position, range, sightLayers);
        if(tempEnemies.Length > 0)
        {
            nearTargets = new UniversalStats[tempEnemies.Length];
            for(int i=0; i < tempEnemies.Length; i++)
            {
                //nearTargets[i] = tempEnemies[i].transform.get
            }
        }

    }


}
