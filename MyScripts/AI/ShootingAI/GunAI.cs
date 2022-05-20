using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GunAI : MonoBehaviour
{
    public GameObject player;
    public float health = 100;
    public float hitDamage = 20;
    public float ShootDist = 35f;

    private bool inShotRange = false;
     
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

    //private CharacterControllerScript pScript;

    private BotShooting shootScript;

    private Vector3 playerTravelDir;
    //private CharacterController pControl;

    private Transform gunTransform;

    private float prevHealth;
    // Start is called before the first frame update
    void Start()
    {
        navmesh = GetComponent<NavMeshAgent>();
        navmesh.speed = 12;
        navmesh.acceleration = 100;
        navmesh.angularSpeed = 360;
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

        
        if (player == null)
            player = GameObject.FindWithTag("Player");

        centerPos = transform.position;
        wanderDest = centerPos + new Vector3(Random.Range(-wanderRangeX, wanderRangeX), 0, Random.Range(-wanderRangeZ, wanderRangeZ));

        realDetect = detectDistance;

        uniStats = GetComponent<UniversalStats>();

        //pScript = player.GetComponent<CharacterControllerScript>();

        shootScript = GetComponentInChildren<BotShooting>();

        //pControl = player.GetComponent<CharacterController>();

        gunTransform = shootScript.transform;

        prevHealth = health;
    }

    //POSSIBLE CHANGE - look into NavMeshAgent.updateRotation, as it could be used to set wanderdest without rotating, so things like flanking would be possible

    // Update is called once per frame
    void FixedUpdate()
    {
        //Note - this is currently not applied - it is commented out later on
        playerTravelDir = CharacterControllerScript.characterController.velocity / 5; //used to offset the look position so the turret shoots where the player will be, so you cant just walk in a circle to dodge bullets
                                                                                      //the /5 bit is coz using the velocity directly is too strong of an effect, /5 seemed to work well though it is based off of nothing in particular


        if (hitSound != null)
            hitSound.pitch = Time.timeScale;
        if (chaseSound != null)
            chaseSound.pitch = Time.timeScale;
        if (wanderSound != null)
            wanderSound.pitch = Time.timeScale;

        if (uniStats.health < health)//update health if damage is done to unistats
            health = uniStats.health;

        if (health <= 0)
        {
            //Debug.Log("DEAD");
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            gameObject.GetComponent<GunAI>().enabled = false;
            gameObject.GetComponent<Rigidbody>().freezeRotation = false;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            isDead = true;
            wanderSound.Stop();
            chaseSound.Stop();

            gameObject.layer = 16;
        }

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
            else if (seePlayer)
            {//if previously saw player
                wanderDest = player.transform.position; //investigate last know position
                seePlayer = false;
            }
            else
                seePlayer = false;
        }
        else
        {
            //Debug.DrawRay(transform.position, pDir, Color.white);
            //Debug.Log("Did not Hit");
        }

        float DistFromP = Vector3.Distance(transform.position, player.transform.position);
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

        if (DistFromP <= ShootDist)
            inShotRange = true;
        else
            inShotRange = false;

        shootScript.shoot = false;
        if (!resetting && nearPlayer && !isDead && seePlayer && !wandering)
        {
            if (!inShotRange)
            {
                //Chase Player
                navmesh.speed = 12;
                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z) /*+ playerTravelDir*/);
                navmesh.destination = player.transform.position;

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
            {//In shoot range
                transform.LookAt(player.transform.position);
                navmesh.destination = transform.position;
                //point gun 
                gunTransform.LookAt(player.transform.position - Vector3.up / 2);
                gunTransform.localEulerAngles = new Vector3(gunTransform.localEulerAngles.x * -1, gunTransform.localEulerAngles.y + 180, gunTransform.localEulerAngles.z);

                shootScript.shoot = true;//shoot
            }
            //play reee sound-------------------
            //stop wander sound


        }
        else if (!isDead && wandering)
        {
            // if no wander point is set, make one  -- also has check to see if the agent gets close to the point (to help with points in walls)
            if ((transform.position.x, transform.position.z) == (wanderDest.x, wanderDest.z) || Vector3.Distance(transform.position, wanderDest) < 0.5f)
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
                navmesh.speed = 6;

                if (wanderSound != null)
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
                navmesh.destination = wanderDest;
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

    /*
    private void CheckSee()
    {
        RaycastHit sight;
        Vector3 pDir = player.transform.position - transform.position;
        //offset raycast slighlty when crouching as colider is too short for normal detection
        if (pScript.getCrouch())
            pDir.y += 0.5f; //Was -0.5f when crouching, no change when not - cahnge to alter this coz of weird behaviour witht he new coruching system
        else
            pDir.y += 1.0f;
        if (Physics.Raycast(transform.position, pDir, out sight, Mathf.Infinity))
        {
            if (sight.collider.tag != "Player" || Vector3.Distance(transform.position, player.transform.position) > ShootDist) //if the enemy cannot see the player or is far away then move towards player
            {
                navmesh.destination = player.transform.position;
            }
            else
                navmesh.destination = transform.position;
        }
    }
    */
    private IEnumerator HeathCheck()
    {
        while (isDead)
        {
            if (prevHealth < health)
            {//damage done
            }
            prevHealth = health;
            yield return new WaitForSeconds(0.25f);//wait for 0.25s rather than a frame so multiple hits are included in healt update
        }
    }
}
