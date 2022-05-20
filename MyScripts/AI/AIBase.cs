using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBase : MonoBehaviour
{   /* TODO - turn this into a base class for all ai - have it store basics ;ike waner and detect range, find random wander point, detect death etc.
    public GameObject player;
    [HideInInspector]
    public float health = 100;
    public float hitDamage = 20;

    public AudioClip bodyHit;
    public AudioClip headshot;
    public AudioClip hitSound;
    public AudioClip chaseSound;
    public AudioClip wanderSound;
    public AudioClip pHitSound;
    public AudioSource sounds;

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

    private UniversalStats uniStats;
    //private float volume;
    private float counterStuck = 0;

    private float seeTimer = 0;

    public float idleSpeed = 6;
    public float chaseSpeed = 12;
    public float accel = 100;
    public float angularSpeed = 360;
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
        hitSound = aSources[0];
        chaseSound = aSources[1];
        wanderSound = aSources[2];
        pHitSound = aSources[2];

        chaseSound.Stop();
        wanderSound.Play();

        //hitSound.volume = 0;
        if (player == null)
            player = GameObject.FindWithTag("Player");

        centerPos = transform.position;
        wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));

        NavMeshHit hit;
        if (NavMesh.SamplePosition(wanderDest, out hit, 3f, NavMesh.AllAreas))
            wanderDest = hit.position;
        else
            wanderDest = transform.position;

        realDetect = detectDistance;

        uniStats = GetComponent<UniversalStats>();
    }

    // Update is called once per frame
    void Update()
    {
        hitSound.pitch = Time.timeScale;
        chaseSound.pitch = Time.timeScale;
        wanderSound.pitch = Time.timeScale;
        pHitSound.pitch = Time.timeScale;

        if (uniStats.health < health)
        {//update health if damage is done to unistats
            health = uniStats.health;
            //hitSound.Play();
        }

        if (health <= 0)
        {
            //Debug.Log("DEAD");
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            gameObject.GetComponent<Guard>().enabled = false;
            gameObject.GetComponent<Rigidbody>().freezeRotation = false;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            gameObject.GetComponent<Rigidbody>().AddTorque(transform.right * 5f);
            isDead = true;
            wanderSound.Stop();
            chaseSound.Stop();


            gameObject.layer = 16;
        }

        CheckStuck();

        if (player.GetComponent<CharacterControllerScript>().getCrouch())
            realDetect = detectDistance / 2;
        else
            realDetect = detectDistance;


        RaycastHit sight;
        Vector3 pDir = player.transform.position - transform.position;
        pDir.y += 1f;
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


        if (Vector3.Distance(transform.position, player.transform.position) <= realDetect && !isDead)
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
            navmesh.speed = chaseSpeed;
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
            navmesh.destination = player.transform.position;

            if (!chaseSound.isPlaying && wanderSound.isPlaying)
            {
                wanderSound.Stop();
                chaseSound.Play();
            }
            //play reee sound-------------------
            //stop wander sound


        }
        else if (!isDead && wandering)
        {
            // if no wander point is set, make one  -- also has check to see if the agent gets close to the point (to help with points in walls)
            if ((transform.position.x, transform.position.z) == (wanderDest.x, wanderDest.z) || Vector3.Distance(transform.position, wanderDest) < 1f)
            {
                SetWander();

            }
            else //if a wander point is set, go to it
            {
                //transform.LookAt(wanderDest);
                navmesh.destination = wanderDest;
                navmesh.speed = idleSpeed;
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

                gameObject.GetComponent<Rigidbody>().freezeRotation = true;
                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

            }
        }
        //Debug.Log("Health " + health);
    }

    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("e");
        if (collision.gameObject.tag == "Player")
        {
            if (!hitPlayer && !isDead)
            {
                hitPlayer = true;
                collision.gameObject.GetComponent<CharacterControllerScript>().health -= hitDamage;

                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                gameObject.GetComponent<Rigidbody>().freezeRotation = true;

                //pHitSound.Play();
                //Debug.Log("player hit");
            }
        }
        else if (collision.gameObject.tag == "Bullet")
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

    public bool getDead()
    {
        return isDead;
    }

    private void CheckStuck()
    {
        if (navmesh.velocity == new Vector3(0f, 0f, 0f))
        {
            counterStuck += Time.deltaTime;
            if (counterStuck >= 2f)
            {
                Debug.Log("Character stuck");
                navmesh.enabled = false;
                navmesh.enabled = true;
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
        navmesh.destination = wanderDest;
        //navmesh.destination = destPos.position;
        navmesh.speed = idleSpeed;

        if (!wanderSound.isPlaying && chaseSound.isPlaying)
        {
            wanderSound.Play();
            chaseSound.Stop();
        }
    }
    */
}
