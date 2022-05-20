using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SmartGunAI : SmartAI
{
    [Header("Shooting Settings")]
    protected bool inShotRange = false;
    protected BotShooting shootScript;
    protected Vector3 playerTravelDir;
    protected Transform gunTransform;
    protected float prevHealth;
    public float ShootDist = 35f;
    protected bool fleeing = false;

    public int coverAreaMask = 16; //is a bitfield for whatever fucking reason
    protected Vector3 prevPlayerPos;
    protected bool surpressing;

    
    private void Start()
    {
        StartFunction();
    }

    protected virtual void StartFunction()
    {
        navmesh = GetComponent<NavMeshAgent>();
        startSpeed = navmesh.speed;
        //navmesh.speed = 12;
        //navmesh.acceleration = 50;
        //navmesh.angularSpeed = 180;

        AudioSource[] aSources = GetComponents<AudioSource>();
        if (aSources.Length >= 4)
        {
            hitSound = aSources[0];
            chaseSound = aSources[1];
            wanderSound = aSources[2];
            pHitSound = aSources[3];
        }

        if (chaseSound != null)
            chaseSound.Stop();
        if (wanderSound != null)
            wanderSound.Play();

        centerPos = transform.position;
        wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));
        wanderDest = GetValidPoint(wanderDest);

        realDetect = detectDistance;
        eventOccured = 0;

        if (getRandomPath)
        {
            points = (Transform[])AIPathManager.instance.GetRandomPath(AIPath.AIPathtype.Guard, transform).Clone();
            if (points != null && points.Length > 0)
                isFixedPatrol = true; //only use the patrol is a path is actually recieved
            else
                isFixedPatrol = false;
        }
        if (scramblePath)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] = points[Random.Range(0, points.Length)]; //very simple scramble
        }

        GotoNextPoint();
        detection = 0;
        investiagte.SetActive(false);
        chase.SetActive(false);

        navRigid = transform.GetComponent<Rigidbody>();
        uniStats = GetComponent<UniversalStats>();

        StartCoroutine(AIFunction());

        shootScript = GetComponentInChildren<BotShooting>();
        gunTransform = shootScript.transform;

        prevHealth = health;

        StartCoroutine(HealthCheck());
    }

    protected override IEnumerator AIFunction()
    {
        yield return new WaitForSeconds(1f);//wait a second for all start functions to finish
        while (!isDead)
        {
            if (uniStats.health <= 0)
            {
                isDead = true;
                shootScript.shoot = false;
                break;
            }

            if (startedCo)
            {
                currentState = AiState.waiting;
            }
            else if (wandering)
            {
                currentState = AiState.wandering;
            }
            else if (chase)
            {
                currentState = AiState.chasing;
            }
            if (fleeing)
                currentState = AiState.fleeing;


            //Scale Sounds with timescale - for slowmo/fast time
            if (hitSound != null)
                hitSound.pitch = Time.timeScale;
            if (chaseSound != null)
                chaseSound.pitch = Time.timeScale;
            if (wanderSound != null)
                wanderSound.pitch = Time.timeScale;

            if (uniStats.health < health)//if universal stats has taken damage, update health
                health = uniStats.health;
            if (health <= 0) //Check if should be dead
            {

                StopCoroutine(AIFunction());
                Death();
            }

            CheckStuck();

            SetDetectRateRange();

            if (!fleeing) //only set see and near properties if not already fleeing
            {
                if (!useFov)
                {
                    CheckSee();
                    CheckInFront();
                }

                CheckIsNear();

                SetDetect();
            }
            else //pretend to not see player when fleeing
            {
                seePlayer = false;
                nearPlayer = false;
                Flee();
            }

            

            CheckWander();

            //CheckChase();

            WanderFunction();

            if (shootScript.shoot && seePlayer && nearPlayer && !fleeing) //if shooting dont keep charging
            {
                wanderDest = transform.position;
            }

            if (!seePlayer || !nearPlayer || fleeing)
                shootScript.shoot = false;

            inShotRange = (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) <= ShootDist && !fleeing);
            

            AttackReset();

            DetermineBehaviour();
            //Wait
            //Debug.Log(gameObject.name + " Yee");
            if(seePlayer)
                prevPlayerPos = CharacterControllerScript.instance.transform.position;
            if (wandering && eventOccured <= 0)
                yield return new WaitForSecondsRealtime(0.06f);
            else
                yield return new WaitForSecondsRealtime(0.03f); //run faster when player is spotted/investigating
        }
    }

    protected void ShootPlayer(Vector3 target)
    {
        //transform.LookAt(CharacterControllerScript.instance.transform.position);
        Quaternion lookDir = Quaternion.LookRotation((target - transform.position).normalized);
        lookDir.x = 0;
        lookDir.z = 0;
        transform.rotation = lookDir;
        //point gun 
        gunTransform.LookAt(target);
        //gunTransform.localEulerAngles = new Vector3(gunTransform.localEulerAngles.x * -1, gunTransform.localEulerAngles.y + 180, gunTransform.localEulerAngles.z);

        shootScript.shoot = true;//shoot
    }

    protected virtual void DetermineBehaviour()
    {
        if (surpressing)
        {
            navmesh.destination = transform.position;
            ShootPlayer(prevPlayerPos);
        }

        else if (detection >= 100 && !fleeing)
        {
            if (!inShotRange) //if not in range, dont affect wander dest 
            {
                //Chase Player
                navmesh.speed = startSpeed;
                transform.LookAt(new Vector3(CharacterControllerScript.instance.transform.position.x, transform.position.y, CharacterControllerScript.instance.transform.position.z) /*+ playerTravelDir*/);
                navmesh.destination = CharacterControllerScript.instance.transform.position;

                if (wanderSound != null)
                {
                    if (!chaseSound.isPlaying && wanderSound.isPlaying)
                    {
                        wanderSound.Stop();
                        chaseSound.Play();
                    }
                }
            }
            else
            {//In shoot range - change wanderdest and shoot

                navmesh.destination = transform.position;
                ShootPlayer(CharacterControllerScript.instance.pCam.transform.position);
            }
        }
    }

    protected IEnumerator HealthCheck()
    {
        while (!isDead)
        {
            //Debug.Log("Damage done: " + prevHealth + " -> " + health); 
            if (false && prevHealth > health+30 && Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position)>15f) //if more than 30 damage is done and player is more than 15m away ************************ DISABLED
            {//damage done
                fleeing = true;
                SetFleePoint();
            }
            prevHealth = health;
            yield return new WaitForSeconds(1f);//wait for 1s rather than a frame so multiple hits are included in health update
        }
    }

    protected void SetFleePoint()
    {
        fleeing = true;

        //Pick random point within wander area
        //New method - isnt confined the original area, instead runs away from current location - also goes a minimum of range/2
        int posNeg = Random.Range(0, 2);
        if (posNeg == 0) //positive
            wanderDest = transform.position + new Vector3(Random.Range(wanderRangeX / 2, wanderRangeX), 0, Random.Range(wanderRangeZ / 2, wanderRangeZ));
        else
            wanderDest = transform.position - new Vector3(Random.Range(wanderRangeX / 2, wanderRangeX), 0, Random.Range(wanderRangeZ / 2, wanderRangeZ));

        //shorter random dest used for finding cover randomly - if transform.pos was used then the same cover would be found over and over
        Vector3 shortDest;
        if (posNeg == 0)
            shortDest = transform.position + new Vector3(Random.Range(wanderRangeX / 6, wanderRangeX / 3), 0, Random.Range(wanderRangeZ / 6, wanderRangeZ / 3));
        else
            shortDest = transform.position - new Vector3(Random.Range(wanderRangeX / 6, wanderRangeX / 3), 0, Random.Range(wanderRangeZ / 6, wanderRangeZ / 3));
        

        //Check if the point is on the navmesh and find the nearest point if it isnt
        NavMeshHit destPos;

        if (NavMesh.SamplePosition(shortDest, out destPos, 6f, coverAreaMask))
        {
            //found area in cover
            //Debug.Log("Found cover area " + destPos.position);
        }
        else //find a normal spot
            wanderDest = GetValidPoint( wanderDest);
        //update destination 
        wanderDest = destPos.position;

        //set destination
        navmesh.destination = wanderDest;
    }

    protected void Flee()
    {
        
        if (Vector3.Distance(transform.position, wanderDest) < 1.5f) //if near the point
            fleeing = false; //stop fleeing
        else if(!isDead)
            navmesh.destination = wanderDest;
    }

    protected override void LostSight()
    {
        //Debug.Log("Lost Sight");
        StartCoroutine(LostSightAttack());
    }

    protected IEnumerator LostSightAttack()
    {
        yield return null;
        float attacking = 3f;
        while (attacking>0 && !seePlayer && !isDead && navmesh.enabled) //while attacking keep shoting and slowly advance
        {
            //Debug.Log("Surpressing!");
            surpressing = true;
            //shootScript.shoot = true;
            navmesh.destination = GetValidPoint(transform.position + transform.forward);
            navmesh.speed = startSpeed/5f;

            attacking -= Time.deltaTime;
            yield return null;
        }
        surpressing = false;
        if(!seePlayer && !isDead)
            setWanderDest(prevPlayerPos, Random.Range(1,3)); //after shooting, go investigate
    }
}
