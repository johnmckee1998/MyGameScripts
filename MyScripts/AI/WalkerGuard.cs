using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkerGuard : MonoBehaviour
{
    //public GameObject player;
    [HideInInspector]
    public float health = 100;
    public float hitDamage = 20;
    public float hitBuffer = 1.2f;
    public AudioClip bodyHit;
    public AudioClip headshot;
    private AudioSource hitSound;
    private AudioSource chaseSound;
    private AudioSource wanderSound;
    private AudioSource pHitSound;

    private NavMeshAgent navmesh;

    [Tooltip("How far away the enemy will detect the player from")]
    public float detectDistance = 10;
    private float realDetect;

    private bool hitPlayer = false;
    private bool resetting = false;
    private float counting = 0;

    private bool nearPlayer = false;
    private bool seePlayer = false;
    private bool isDead = false;
    private bool wandering = true;

    private Vector3 centerPos;
    private Vector3 wanderDest;
    public float wanderRangeX = 20.0f;
    public float wanderRangeZ = 20.0f;
    public float wanderRangeALL = 20.0f;

    private UniversalStats uniStats;
    //private float volume;
    private float counterStuck = 0;

    private float seeTimer = 0;

    public float idleSpeed = 6;
    public float chaseSpeed = 12;
    public float accel = 100;
    public float angularSpeed = 360;


    private Animator guardAnim;

    private float chasePitch;

    private bool chasingPlayer;

    private bool waiting;

    private Rigidbody rb;

    private float destUpdateTime;

    public int deathlayer;
    // Start is called before the first frame update
    void Start()
    {
        navmesh = GetComponent<NavMeshAgent>();
        navmesh.speed = idleSpeed;
        navmesh.acceleration = accel;
        navmesh.angularSpeed = angularSpeed;
        //Debug.Log("Speed " + navmesh.speed);
        //Debug.Log("Accel " + navmesh.acceleration);
        //Debug.Log("AngSpeed " + navmesh.angularSpeed);
        AudioSource[] aSources = GetComponents<AudioSource>();
        if (aSources.Length >= 4)
        {
            hitSound = aSources[0];
            chaseSound = aSources[1];
            wanderSound = aSources[2];
            pHitSound = aSources[3];
            chaseSound.Stop();
            wanderSound.Play();
        }



        //hitSound.volume = 0;
        //if (player == null)
        //    player = GameObject.FindWithTag("Player");

        centerPos = transform.position;
        wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));

        NavMeshHit hit;
        if (NavMesh.SamplePosition(wanderDest, out hit, 3f, NavMesh.AllAreas))
            wanderDest = hit.position;
        else
            wanderDest = transform.position;

        realDetect = detectDistance;

        uniStats = GetComponent<UniversalStats>();

        if (chaseSound != null)
            chasePitch = chaseSound.pitch;

        guardAnim = GetComponent<Animator>();

        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hitSound != null)
            hitSound.pitch = Time.timeScale;
        if (chaseSound != null)
            chaseSound.pitch = chasePitch * Time.timeScale;
        if (wanderSound != null)
            wanderSound.pitch = Time.timeScale;
        if (pHitSound != null)
            pHitSound.pitch = Time.timeScale;

        if (uniStats.health < health)
        {//update health if damage is done to unistats
            health = uniStats.health;
            //hitSound.Play();
        }

        if (health <= 0)
        {
            //Debug.Log("DEAD");
            OnDeath();
        }

        CheckStuck();



        /*if(Vector3.Distance(transform.position, player.transform.position) < 1.5)
        {
            if (!hitPlayer && !isDead)
            {
                hitPlayer = true;
                player.GetComponent<CharacterControllerScript>().health -= hitDamage;
                //pHitSound.Play();
                //Debug.Log("player hit");
            }
        }*/

        if (CharacterControllerScript.instance.getCrouch())
            realDetect = detectDistance / 2;
        else
            realDetect = detectDistance;


        CheckSight();

        float DistToPlayer = Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position);
        if (DistToPlayer <= realDetect && !isDead)
            nearPlayer = true;
        else
            nearPlayer = false;

        if (nearPlayer && seePlayer)
            wandering = false;
        else
        {
            wandering = true;

            //wander sound ---------------------------------- already done elsewhere
            //stop reee sound
        }


        if (!resetting && nearPlayer && !isDead && seePlayer && !wandering)
        {
            //Chase Player
            chasingPlayer = true;

            navmesh.speed = chaseSpeed;
            transform.LookAt(new Vector3(CharacterControllerScript.instance.transform.position.x, transform.position.y, CharacterControllerScript.instance.transform.position.z));
            if (destUpdateTime < Time.time + 0.5f || true) //only update dest evey half second - should reduce cpu load with lots of bots
            {
                if (CheckInFront(CharacterControllerScript.instance.transform.position))
                    navmesh.destination = CharacterControllerScript.instance.transform.position;
                else
                    TurnToTarget(CharacterControllerScript.instance.transform.transform.position);
                destUpdateTime = Time.time;
            }

            if (chaseSound != null && wanderSound != null)
            {
                if (!chaseSound.isPlaying && wanderSound.isPlaying)
                {
                    wanderSound.Stop();
                    chaseSound.Play();
                }
            }
            //play reee sound-------------------
            //stop wander sound

            if (DistToPlayer < 0.5f)
            {
                navmesh.speed = 0.01f;
            }
        }
        else if (!isDead && wandering)
        {
            chasingPlayer = false;

            // if no wander point is set, make one  -- also has check to see if the agent gets close to the point (to help with points in walls)
            if ((transform.position.x, transform.position.z) == (wanderDest.x, wanderDest.z) || Vector3.Distance(transform.position, wanderDest) < 1f)
            {
                if (!waiting)
                {
                    waiting = true;
                    StartCoroutine(WaitSetWander());
                }
                //SetWander();

            }
            else //if a wander point is set, go to it
            {
                //transform.LookAt(wanderDest);
                if (CheckInFront(wanderDest))
                    navmesh.destination = wanderDest;
                else
                    TurnToTarget(wanderDest);
                navmesh.speed = idleSpeed;
            }

            if (DistToPlayer < 0.5f)
            {
                navmesh.speed = 0.01f;
            }

        }

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

                rb.freezeRotation = true;
                rb.constraints = RigidbodyConstraints.FreezePosition;

            }
        }

        if (DistToPlayer < hitBuffer) //attack player when very close 
        {
            AttackPlayer();
        }




        //Debug.Log("Health " + health);
    }

    /*
    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("e");
        if(collision.gameObject.tag == "Player")
        {
            //AttackPlayer();
        }
        else if(collision.gameObject.tag == "Bullet")
        {
            //if (hitSound.volume == 0)
            //hitSound.volume = 100;
            //hitSound.outputAudioMixerGroup.audioMixer.GetFloat("Volume", out volume);
            //Debug.Log("Volume " + volume);
            //AudioSource.PlayClipAtPoint(hitSound.clip, transform.position);
            //audioS.Play();
            //Debug.Log("YEEEE");

            //hitSound.Play();

            //Debug.Log("Speed " + navmesh.speed);
        }
    }
    */

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRangeALL);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(2 * wanderRangeX, 1, 2 * wanderRangeZ));
        //Gizmos.color = Color.gray;
        //Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y-0.5f, transform.position.z), 2.0f);

        if (wanderDest != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(wanderDest, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(centerPos, 0.5f);
        }



    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 pDir = player.transform.position - transform.position;
        pDir.y += 1f;
        Gizmos.DrawRay(transform.position, pDir);
    }*/

    public bool getDead()
    {
        return isDead;
    }

    private void CheckStuck()
    {
        if (navmesh.velocity == new Vector3(0f, 0f, 0f))
        {
            counterStuck += Time.deltaTime;
            if (counterStuck >= 20f)
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
                SetWander();
            }
            // navmesh agent will start moving again
        }
        else
            counterStuck = 0;
    }

    private void SetWander()
    {
        //Pick random point within wander area
        wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));

        //Debug.Log("WanderDest " + wanderDest);

        //Check if the point is on the navmesh and find the nearest point if it isnt
        NavMeshHit destPos;
        NavMesh.SamplePosition(wanderDest, out destPos, 2.5f, NavMesh.AllAreas);

        //update destination 
        wanderDest = new Vector3(destPos.position.x, wanderDest.y, destPos.position.z);

        //Debug.Log("DestPos " + destPos.position);
        //transform.LookAt(wanderDest);

        //set destination
        if (CheckInFront(wanderDest))
            navmesh.destination = wanderDest;
        else
            TurnToTarget(wanderDest);
        //navmesh.destination = destPos.position;
        navmesh.speed = idleSpeed;

        if (chaseSound != null && wanderSound != null)
        {
            if (!wanderSound.isPlaying && chaseSound.isPlaying)
            {
                wanderSound.Play();
                chaseSound.Stop();
            }
        }
    }

    private void AttackPlayer()
    {
        if (!hitPlayer && !isDead)
        {
            hitPlayer = true;
            CharacterControllerScript.instance.health -= hitDamage;

            rb.constraints = RigidbodyConstraints.None;
            rb.freezeRotation = true;

            //pHitSound.Play();
            //Debug.Log("player hit");
        }
    }

    public bool IsChasing()
    {
        return chasingPlayer;
    }

    private IEnumerator WaitSetWander()
    {
        yield return new WaitForSeconds(Random.Range(1f, 5f));
        if (!isDead)//prevent situation where it stops, calls this coroutine but then dies, meaning that is then tries to set destination on a non active agent
            SetWander();
        waiting = false;
    }

    private void OnDeath()
    {
        navmesh.enabled = false;
        enabled = false;

        rb.freezeRotation = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddTorque(transform.right * 5f);
        isDead = true;
        if (chaseSound != null && wanderSound != null)
        {
            wanderSound.Stop();
            chaseSound.Stop();
        }

        if (guardAnim != null)
            guardAnim.enabled = false;

        gameObject.layer = deathlayer;
    }

    private void CheckSight()
    {
        RaycastHit sight;
        Vector3 pDir = (CharacterControllerScript.instance.transform.position + Vector3.up) - transform.position;
        if (CharacterControllerScript.instance.getCrouch())//if crouching
            pDir = (CharacterControllerScript.instance.transform.position + Vector3.up / 2) - transform.position;
        //pDir.y += 1f;
        if (Physics.Raycast(transform.position, pDir, out sight, Mathf.Infinity))
        {
            //Debug.DrawRay(transform.position, pDir, Color.yellow);
            //Debug.Log("Hit " + sight.collider.gameObject.tag);
            if (sight.collider.tag == "Player")
            {
                //Debug.Log("Hit Player");
                seePlayer = true;
            }
            else
            {
                if (seePlayer && seeTimer < 2.5f)//if previously saw player and now cant
                {
                    seeTimer += Time.deltaTime;//pretend to see player for another 1 second so that if sight is  lost for a split second the guard doesnt just abandon the attack

                }
                else
                {
                    seePlayer = false;
                    seeTimer = 0; //reset timer
                }
            }
        }
        else
        {
            //Debug.DrawRay(transform.position, pDir, Color.white);
            //Debug.Log("Did not Hit");
        }
    }


    private void TurnToTarget(Vector3 target)
    {
        Vector3 targetDir = target - transform.position;
        targetDir.Normalize();

        transform.eulerAngles = Vector3.MoveTowards(transform.eulerAngles, Quaternion.LookRotation(targetDir).eulerAngles, 50f*Time.fixedDeltaTime);
    }

    private bool CheckInFront(Vector3 Pos) //Checks if given position is within +- 10 degrees of tranform forward
    {
        Vector3 posDir = new Vector3( Pos.x, transform.position.y, Pos.z )- transform.position;
        float angleToPos = Vector3.Angle(posDir.normalized, transform.forward);
        Debug.Log("Angle: " + angleToPos);
        if (angleToPos <= 10 && angleToPos >= -10)
            return true;



        return false;
    }

}
