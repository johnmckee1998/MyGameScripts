using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class SmartAI : MonoBehaviour
{
    [Header("Detection Symbols")]
    public GameObject investiagte;
    public GameObject chase;

    [Header("Basic Values")]
    
    public float health = 100;
    [Tooltip("Damage Delt to Player")]
    public float hitDamage = 20;

    //change this to 1 audiosource with multiple clips
    protected AudioSource hitSound;
    protected AudioSource chaseSound;
    protected AudioSource wanderSound;
    protected AudioSource pHitSound;

    //Components of current object
    protected NavMeshAgent navmesh;
    protected Rigidbody navRigid;

    [Tooltip("How far away the enemy will detect the player from")]
    public float detectDistance = 10;
    protected float realDetect;

    protected bool hitPlayer = false;
    protected bool resetting = false;
    protected float counting = 0;

    protected bool nearPlayer = false;
    protected bool seePlayer = false;
    protected bool playerInfront = false;
    protected bool isDead = false;
    protected bool wandering = true;

    //used for special event wandering - gunshot, explosion etc
    protected int eventOccured;

    //Starting position, center of patrol area
    protected Vector3 centerPos;

    [Header("Wander Values")]

    //Randomised destination to wander to while patroling 
    protected Vector3 wanderDest;
    //Area contraints for patrolling 
    public float wanderRangeX = 20.0f;
    public float wanderRangeZ = 20.0f;

    //[Tooltip("Not used - old method")]
    //public float wanderRangeALL = 20.0f;

    [Tooltip("Time to wait once a destination is reached")] 
    public float waitTime = 2.0f;

    //Dynamic value used the record how long is left to wait
    //private float timeToWait;
    //Used for waiting
    protected bool isWaiting = false;
    //To help prevent multiple coroutines from running
    protected bool startedCo = false;

    //Used to record how long an ai is staying still, to detect if they are stuck
    protected float counterStuck = 0;

    [Header("Ai Type Values")]
    public bool isGuard = false; //Couldnt guard just be a patrol with only 1 point?
    public bool isFixedPatrol = false;
    public bool getRandomPath;
    public bool scramblePath;
    [Tooltip("Points to patrol if type is a fixed patrol")]
    public Transform[] points; //Make sure to consider rotation when putting in tranform locations
    protected int destPoint = 0;
    protected Quaternion targetRot;

    //Used for detection
    protected float detection;
    [Space]
    public float detectRate = 40f;
    public Transform sightPos;
    protected float realDetectRate;

    public GameObject blood;

    protected UniversalStats uniStats;

    //public float attackSpeed = 10;
    protected float startSpeed;
    //private float lastDetect;

    public enum AiState {wandering, waiting, chasing, fleeing} //only really for debugging?
    public AiState currentState;

    [Space]
    public bool useFov;
    private bool firstPoint;
    // Start is called before the first frame update
    void Start()
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

        if(chaseSound!=null)
            chaseSound.Stop();
        if (wanderSound != null)
            wanderSound.Play();

        //hitSound.volume = 0;
        

        centerPos = transform.position;
        wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));
        wanderDest = GetValidPoint(wanderDest);
        realDetect = detectDistance;

        //timeToWait = waitTime;
        eventOccured = 0;

        GotoNextPoint();

        detection = 0;

        investiagte.SetActive(false);
        chase.SetActive(false);
        
        navRigid = transform.GetComponent<Rigidbody>();
        //lastDetect = 0;
        uniStats = GetComponent<UniversalStats>();

        StartCoroutine(AIFunction());
    }

    // Maybe make coroutine (didnt work last time i tried) 
    protected virtual IEnumerator AIFunction()
    {
        yield return new WaitForSeconds(1f);//wait a second for all start functions to finish
        while (!isDead)
        {
            if (isWaiting)
            {
                Debug.Log("Waiting");
                currentState = AiState.waiting;
            }
            else if (wandering)
            {
                navmesh.speed = startSpeed*0.5f;
                currentState = AiState.wandering;
            }
            else if (chase)
            {
                navmesh.speed = startSpeed;
                currentState = AiState.chasing;
            }


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

            if (!useFov)
            {
                CheckSee();

                CheckInFront();

                CheckIsNear();
            }

            SetDetect();

            CheckWander();

            CheckChase();

            WanderFunction();

            AttackReset();
            
            //Debug.Log("Health " + health);
            if(Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position) < 1.2f) //why?
            {
                AttackPlayer();
            }

            //Wait
            //Debug.Log(gameObject.name + " Yee");
            if (wandering && eventOccured<=0)
                yield return new WaitForSecondsRealtime(0.06f);
            else
                yield return new WaitForSecondsRealtime(0.03f); //run faster when player is spotted/investigating
        }
    }

    /*
    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("e");
        if(collision.gameObject.tag == "Player")
        {
            
        }
        else if(collision.gameObject.tag == "Bullet")
        {
            //AudioSource.PlayClipAtPoint(hitSound.clip, transform.position);

            //hitSound.Play();
        }
    }
    */

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(transform.position, wanderRangeALL);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(2*wanderRangeX,1,2*wanderRangeZ));
        //Gizmos.color = Color.gray;
        //Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y-0.5f, transform.position.z), 2.0f);

        if(wanderDest != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(wanderDest, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(centerPos, 0.5f);
        }

        if (CharacterControllerScript.instance != null)
        {
            //Vector3 pDir = CharacterControllerScript.instance.pCam.transform.position - transform.position;
            Gizmos.DrawLine(transform.position, CharacterControllerScript.instance.pCam.transform.position);
            if (sightPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(sightPos.position, CharacterControllerScript.instance.pCam.transform.position);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(new Ray(sightPos.position, CharacterControllerScript.instance.pCam.transform.position - sightPos.position));
            }
        }

    }

    protected virtual void AttackPlayer()
    {
        if (!hitPlayer && !isDead)
        {
            hitPlayer = true;
            CharacterControllerScript.instance.health -= hitDamage;

            navRigid.constraints = RigidbodyConstraints.None;
            navRigid.freezeRotation = true;

            //pHitSound.Play();
            //Debug.Log("player hit");
        }
    }

    public bool getDead()
    {
        return isDead;
    }

    //Used for waiting at a destination
    protected IEnumerator Wait(float waitTimeR)
    {
        //isWaiting = false;
        float randWait = 2f;
        if (!isFixedPatrol || eventOccured > 0)
            randWait = Random.Range(0.5f, waitTimeR);
        else if (isFixedPatrol && wandering && eventOccured <= 0) //Wait for a more consistent time if patrolling fixed points
            randWait = waitTimeR;

        if(wanderSound!=null)
            wanderSound.Stop();
        yield return new WaitForSeconds(randWait);
        //print("WaitAndPrint " + Time.time);
        if(!isDead && wanderSound!=null)
            wanderSound.Play();
        isWaiting = false;
        startedCo = false;
        //StopAllCoroutines();
    }

    public void setWanderDest(Vector3 des, int eventNum)
    {
        if (wandering)
        {
            wanderDest = des;
            navmesh.speed = startSpeed;
            eventOccured = eventNum;
            investiagte.SetActive(true);
        }
        //walk faster to event
        //navmesh.speed = 8;
        //eventOccured = eventNum;
    }

    protected void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        if (!firstPoint) //to start off, assign random point in the cycle, adds more randomness, maybe make this optional
        {
            destPoint = Random.Range(0, points.Length);
            firstPoint = true;
        }

        // Set the agent to go to the currently selected destination.
        wanderDest = points[destPoint].position;
        targetRot = points[destPoint].rotation;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
    }

    public float getDetect()
    {
        if (!isDead)
            return detection;
        else
            return 0;
    }

    public void playHitSound()
    {
        hitSound.Play();
    }


    public void showBlood(Vector3 Bpos, Quaternion Brot)
    {
        GameObject b = Instantiate(blood, Bpos, Brot);
        b.GetComponent<ParticleSystem>().Play();
    }

    protected void Death()
    {

        //Debug.Log("DEAD");
        navmesh.enabled = false;
        navmesh.height = 0;
        navmesh.baseOffset = 0;
        //gameObject.GetComponentInChildren<VisualEffect>().Stop();
        navRigid.freezeRotation = false;
        navRigid.constraints = RigidbodyConstraints.None;
        isDead = true;
        if (wanderSound != null)
            wanderSound.Stop();
        if (chaseSound != null)
            chaseSound.Stop();

        gameObject.layer = 16;

        //Disolve all parts
        

        gameObject.GetComponent<SmartAI>().enabled = false;

        Destroy(gameObject);

    }

    protected void CheckStuck()
    {
        if (navmesh.velocity == new Vector3(0f, 0f, 0f) && points.Length != 1)
        {
            counterStuck += Time.deltaTime;
            if (counterStuck >= waitTime * 3)
            {
                Debug.Log("Character stuck");
                navmesh.enabled = false;
                navmesh.enabled = true;
                /*wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));
                NavMeshHit destPos;
                NavMesh.SamplePosition(wanderDest, out destPos, 3f, NavMesh.AllAreas);
                wanderDest = destPos.position;*/
                wanderDest = transform.position;
                Debug.Log("navmesh re enabled");
                counterStuck = 0;
            }
            // navmesh agent will start moving again
        }
        else
            counterStuck = 0;
    }

    protected void SetDetectRateRange()
    {
        //Half Detection range if player is crouching
        
        if (CharacterControllerScript.instance.getCrouch())
        {
            realDetect = detectDistance / 1.5f;
            realDetectRate = 0.75f*detectRate;
        }
        else
        {
            realDetect = detectDistance;
            realDetectRate = detectRate;
        }
        if (CharacterControllerScript.instance.GetInLight()) //Increase Detection if player is in a light
        {
            realDetect *= 2f;
            realDetectRate *= 2f;
        }
        
    }

    protected virtual void CheckSee()
    {
        RaycastHit sight;
        Vector3 pDir;
        if (sightPos == null)
            pDir = CharacterControllerScript.instance.pCam.transform.position - transform.position;
        else
            pDir = (CharacterControllerScript.instance.pCam.transform.position-(Vector3.up/4f)) - sightPos.position;


        //offset raycast slighlty when crouching as colider is too short for normal detection
        // if (CharacterControllerScript.instance.getCrouch())
        //     pDir.y += 0.5f; //Was -0.5f when crouching, no change when not - cahnge to alter this coz of weird behaviour witht he new coruching system
        // else
        //     pDir.y += 1.0f;
        pDir = pDir.normalized;
        Vector3 rayorigin;
        if (sightPos != null)
            rayorigin = sightPos.position;
        else
            rayorigin = transform.position;
        if (Physics.Raycast(rayorigin, pDir, out sight, detectDistance))
        {
            //Debug.DrawRay(transform.position, pDir, Color.green);
            if (sight.collider.CompareTag("Player"))
            {
                //Debug.Log("see Player");
                seePlayer = true;
            }
            else
            {
                //Debug.Log("Cant see " + sight.transform.gameObject.name);
                //if the Ai could previously see the player and was chasing them but now cannot, search area around last sighting  //Does this work? -> not with smartgunai coz its navmesh dest will be whereever its standing
                if (seePlayer && !wandering)
                {
                    LostSight();
                    //detection =49f;
                    //investiagte.SetActive(true);
                    //Debug.Log("Normal investigate");
                }
                seePlayer = false;
            }
        }
        else
            seePlayer = false;
    }

    protected virtual void CheckInFront()
    {
        Vector3 relativePoint = transform.InverseTransformPoint(CharacterControllerScript.instance.transform.position);
        if (relativePoint.z < 0.0)
            playerInfront = false;
        else if (relativePoint.z > 0.0)
            playerInfront = true;

        //if (playerInfront)
        //    Debug.Log("P in front");
    }

    protected virtual void SetDetect()
    {
        //If the Ai can see the player, is near them and they are in front, then detect
        if (nearPlayer && seePlayer && playerInfront)
        {

            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < 7) //instant detect range - change to variable
                detection = 100;
            else
                detection += realDetectRate / (realDetect/ Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position)); //Make this distance based

            //player.GetComponent<CharacterControllerScript>().setDetect(detection);
        }
        else
            detection -= 2;
        detection = Mathf.Clamp(detection, 0f, 100f); //stop the detection from going below 0 and above 100
    }

    protected virtual void CheckIsNear()
    {
        if (Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position) <= realDetect && !isDead)
            nearPlayer = true;
        else
            nearPlayer = false;
    }

    protected virtual void CheckWander() //If the AI can see the player, is near the player, and the player is in front, then stop wandering
    {
        if (nearPlayer && seePlayer && playerInfront && detection >= 100)
            wandering = false;
        else
        {
            wandering = true;

            //wander sound ---------------------------------- already done elsewhere
            //stop reee sound
        }
    }

    protected virtual void CheckChase()
    {
        //If the enemy is not resetting from a hit, is near the player and can see the player then chase 
        if (!resetting && nearPlayer && !isDead && seePlayer && !wandering)
        {
            //Chase Player
            chase.SetActive(true);
            navmesh.speed = startSpeed;
            //if (Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position) > 3f)
            //    navmesh.speed = startSpeed;
            //else
            //    navmesh.speed = 0.6f*startSpeed;
            transform.LookAt(CharacterControllerScript.instance.transform.position);
            navmesh.destination = CharacterControllerScript.instance.transform.position;

            if (chaseSound != null)
            {
                if (!chaseSound.isPlaying && wanderSound.isPlaying)
                {
                    wanderSound.Stop();
                    chaseSound.Play();
                }
            }
            //play reee sound-------------------
            //stop wander sound
        }
    }

    protected virtual void WanderFunction()
    {
        //Investigate an area if detection is >50 but not yet chasing
        if (detection > 50 && seePlayer && detection < 100 && nearPlayer)
        {
            setWanderDest(CharacterControllerScript.instance.transform.position, 3);
            //Debug.Log("Investigating " + player.transform.position);
        }
        //lastDetect = detection;
        //if (lastDetect > detection)
        //{
        //    setWanderDest(player.transform.position, 3);
        //    lastDetect = detection;
        //}



        //if the enemy is wandering/patrolling
        else if (!isDead && wandering)
        {
            chase.SetActive(false);

            // if no wander point is set, make one  -- also has check to see if the agent gets close to the point (to help with points in walls) 
            if ((transform.position.x, transform.position.z) == (wanderDest.x, wanderDest.z) || Vector3.Distance(transform.position, wanderDest) < navmesh.height/1.5f)
            {
                if (isFixedPatrol)
                    transform.rotation = targetRot;


                //Start waiting start coroutine
                if (isWaiting && !startedCo)
                {
                    StartCoroutine(Wait(waitTime));
                    startedCo = true;
                    //Debug.Log("yeyeyey " + transform.position + " " + wanderDest);
                }
                //If not waiting, wait next update and generate next point
                if (!isWaiting)
                {
                    
                    //StopCoroutine(Wait(waitTime));
                    isWaiting = true;

                    //Pick random point within wander area - normal area if not event occured, else check given number of points near event
                    if (eventOccured <= 0)
                    {
                        investiagte.SetActive(false);
                        if (!isFixedPatrol)
                            wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));
                        else if (isFixedPatrol)
                            GotoNextPoint();

                        wanderDest = GetValidPoint(wanderDest);
                    }
                    else
                    {
                        investiagte.SetActive(true);
                        wanderDest = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                        eventOccured -= 1;

                        wanderDest = GetValidPoint(wanderDest);
                    }

                    //Debug.Log(gameObject.name + " WanderDest " + wanderDest);

                    //Check if the point is on the navmesh and find the nearest point if it isnt
                    

                    //update destination 

                    //wanderDest = destPos.position;



                    //transform.LookAt(wanderDest);

                    //set destination
                    if(!navmesh.destination.Equals(wanderDest))
                        navmesh.destination = wanderDest;

                    //Debug.Log("nanananan " + transform.position + " " + wanderDest);
                }


                //navmesh.destination = destPos.position;
                //navmesh.speed = 4;
                navmesh.speed = startSpeed;
                if (chaseSound != null)
                {
                    if (!wanderSound.isPlaying && chaseSound.isPlaying)
                    {
                        wanderSound.Play();
                        chaseSound.Stop();
                    }
                }

            }
            else //if a wander point is set, go to it
            {

                //transform.LookAt(wanderDest);
                if(!navmesh.destination.Equals(wanderDest)) //only set if not already set, to prevent excessive path calc
                    navmesh.destination = wanderDest;
            }

        }

    }

    protected virtual void AttackReset()
    {
        if (hitPlayer && !isDead && !resetting)
        {
            //hitPlayer = false;
            resetting = true;
            counting = 0.5f;
        }
        if (resetting && !isDead)
        {
            navmesh.destination = transform.position;
            counting -= Time.deltaTime;
            //Debug.Log
            if (counting <= 0)
            {
                resetting = false;
                hitPlayer = false;

                navRigid.freezeRotation = true;
                navRigid.constraints = RigidbodyConstraints.FreezePosition;

            }
        }
    }


    public void CanSeePlayer(bool see)
    {
        if (seePlayer && !see)
            LostSight(); //could previously see player and now cant
        seePlayer = see;
        playerInfront = see;
    }

    protected Vector3 GetValidPoint(Vector3 destToCheck)
    {
        //Debug.Log("getting valid point");
        NavMeshHit destPos;
        int loopcount = 0;
        while (!NavMesh.SamplePosition(destToCheck, out destPos, 5f, navmesh.areaMask))//if samplepos returns true then a valid position is found, in this case a loop runs so long as no point is found
        {
            //Debug.Log(loopcount + " Failed: " + destToCheck);

            if (eventOccured<=0)
                destToCheck = centerPos +new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));
            else
                destToCheck = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));

            loopcount++;

            if (loopcount >= 9)
            { //after 9 loops give up and wait by setting dest pos to transform pos
                destPos.position = transform.position;
                break;
            }
            

        }
        //bool validPath = false;
        //loopcount = 0;
        //while (!validPath)
        //{
            NavMeshPath navPath = new NavMeshPath();
            navmesh.CalculatePath(destPos.position, navPath);
        if (navPath.status == NavMeshPathStatus.PathInvalid || navPath.status == NavMeshPathStatus.PathPartial)
        {
            destPos.position = transform.position;
            //Debug.Log("Bad Path: " + navPath.status);
        }
        //    if (navPath.status == NavMeshPathStatus.PathComplete)
        //    {
        //        validPath = true;
        //        break;
        //    }
        //
        //    if (eventOccured <= 0)
        //        destToCheck = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));
        //    else
        //        destToCheck = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        //
        //    if (loopcount >= 9)
        //    { //after 9 loops give up and wait by setting dest pos to transform pos
        //       destPos.position = transform.position;
        //        break;
        //    }
        //}
        

        return destPos.position;
    }

    protected virtual void LostSight()
    {
        wandering = true;
        setWanderDest(navmesh.destination, (int)Random.Range(1, 4)); //tried player pos and current navmesh destination but both caused them to chase where the player was not where the player was last seen, especially causing issues in switcher scene
    }
}
