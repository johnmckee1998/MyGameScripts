using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UniversalStats))]
public class SniperAI : MonoBehaviour
{
    public GameObject player;
    public float health = 100;
    public float hitDamage = 50f;
    public float ShootDist = 500f;

    private float prevHealth;
    //private bool inShotRange = false;

    private AudioSource sounds;
    public AudioClip hitSound;
    public AudioClip fleeSound;
    public AudioClip pHitSound;

    //private NavMeshAgent navmesh;

    private Vector3 lookDir;
    
    private float realDetect;

    private bool nearPlayer = false;
    private bool seePlayer = false;
    private bool isDead = false;

    private float DistFromP;

    /*
    private Vector3 centerPos;
    private Vector3 wanderDest;
    public float wanderRangeX = 20.0f;
    public float wanderRangeZ = 20.0f;
    public float wanderRangeALL = 20.0f;
    */
    private UniversalStats uniStats;

    private CharacterControllerScript pScript;

    private BotShooting shootScript;

    private Vector3 playerTravelDir;
    private CharacterController pControl;

    private Transform gunTransform;

    //Used when ai is hit - flees to new location
    private bool hiding;

    //[Tooltip("The navmesh area layer that corresponds to cover")] //NOTE: is bitmask, so pos 4 is actually (1 << 4) or 1 shifted 4 to the left in binary, so 10000 which is 16 in decimal
    private readonly int coverAreaMask = 16;
    private Vector3 rotationVector; //random vector used when looking around
    public bool useFov;

    private bool watchingLastPos;

    // Start is called before the first frame update
    void Start()
    {
        //navmesh = GetComponent<NavMeshAgent>();
        
        if (player == null)
            player = GameObject.FindWithTag("Player");

        

        realDetect = ShootDist;

        uniStats = GetComponent<UniversalStats>();

        pScript = player.GetComponent<CharacterControllerScript>();

        shootScript = GetComponentInChildren<BotShooting>();

        pControl = player.GetComponent<CharacterController>();

        sounds = GetComponent<AudioSource>();

        gunTransform = shootScript.transform;

        prevHealth = health;


        StartCoroutine(RandomiseRotationVector());
        //coverAreaMask = NavMesh.GetAreaFromName("User 4");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Note - this is currently not applied - it is commented out later on
        playerTravelDir = pControl.velocity / 5; //used to offset the look position so the turret shoots where the player will be, so you cant just walk in a circle to dodge bullets
                                                 //the /5 bit is coz using the velocity directly is too strong of an effect, /5 seemed to work well though it is based off of nothing in particular


        sounds.pitch = Time.timeScale;

        if (uniStats.health != health)//update health if damage is done to unistats
            health = uniStats.health;

        //Death check
        if (health <= 0 && !isDead)
        {
            //Debug.Log("DEAD");
            //gameObject.GetComponent<NavMeshAgent>().enabled = false;
            this.enabled = false;
            gameObject.GetComponent<Rigidbody>().freezeRotation = false;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            isDead = true;
            shootScript.shoot = false;
            gameObject.layer = 16;
        }


        if (health < prevHealth-30 && !hiding && !isDead)
            StartCoroutine(TakeCover());
        //run away -> wander

        if (!hiding && !isDead)
        {

            if (pScript.getCrouch())
                realDetect = ShootDist / 2;
            else
                realDetect = ShootDist;


            if (!useFov)
            { 
                CheckSee(); //Replace this with a fov sensor
                CheckNear();
            }

            //shootScript.shoot = false;

            if (nearPlayer && !isDead && seePlayer)
            {

                //Shoot at player
                //transform.LookAt(player.transform.position);//+ playerTravelDir
                Quaternion lookDir = Quaternion.LookRotation((CharacterControllerScript.instance.pCam.transform.position - transform.position).normalized);
                lookDir.x = 0;
                lookDir.z = 0;
                transform.rotation = lookDir;

                gunTransform.LookAt(CharacterControllerScript.instance.pCam.transform.position);
                //gunTransform.localEulerAngles += Vector3.up*180;
                //gunTransform.localEulerAngles = new Vector3(gunTransform.localEulerAngles.x * -1, gunTransform.localEulerAngles.y + 180, gunTransform.localEulerAngles.z);
                shootScript.shoot = true;//shoot

            }
            else if (!isDead && !hiding) //cannot see player but is not hiding
            {
                //look around
                LookAround();
                shootScript.shoot = false;
            }
        }

        else if (!isDead)
        {
            shootScript.shoot = false;
        }

        prevHealth = health;
        //Debug.Log(hiding + " Flee");
    }

    

    public bool getDead()
    {
        return isDead;
    }

    private void CheckSee()
    {
        RaycastHit sight;
        Vector3 pDir = player.transform.position - transform.position;
        if (pScript.getCrouch())
            pDir = (player.transform.position + Vector3.up) - transform.position;
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
                seePlayer = false;
        }
        else
        {
            //Debug.DrawRay(transform.position, pDir, Color.white);
            //Debug.Log("Did not Hit");
        }
    }

    private void CheckNear()
    {
        DistFromP = Vector3.Distance(transform.position, player.transform.position);
        if (Vector3.Distance(transform.position, player.transform.position) <= realDetect && !isDead)
            nearPlayer = true;
        else
            nearPlayer = false;
    }

    private void LookAround()
    {
        transform.eulerAngles += rotationVector * Time.fixedDeltaTime;

        /*
        if (transform.eulerAngles == lookDir || lookDir == null || lookDir == Vector3.zero)
        {
            lookDir = new Vector3(Random.Range(-45f, 45f), Random.Range(0f, 360f), 0);
            //if (lookDir.x < 0) //if x is negative, loop it round so it uses a positive value
            //    lookDir = new Vector3(lookDir.x + 360, lookDir.y, 0f);
            //Debug.Log("New look dir " + lookDir);
        }
        else
        {
            transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, lookDir, 2f, 1f);
            gunTransform.eulerAngles = Vector3.RotateTowards(gunTransform.eulerAngles + Vector3.up*180, lookDir, 2f, 1f);
            //Debug.Log("Rotating...");
        }*/
    }

    public void CanSeePlayer(bool see)
    {
        if (seePlayer && !see)
            LostSight(); //could previously see player and now cant
        seePlayer = see;
        nearPlayer = see;
        //playerInfront = see;
    }


    IEnumerator RandomiseRotationVector()
    {
        while (!isDead)
        {
            if(!watchingLastPos)
                rotationVector = new Vector3(0f, Random.Range(-20f, 20f), 0f);
            yield return new WaitForSeconds(Random.Range(6f, 8f));
        }
    }

    IEnumerator LostSight()
    {
        float time = 3f;
        watchingLastPos = true;
        while(time>0 && !seePlayer)
        {
            time -= Time.fixedDeltaTime;
            rotationVector = Vector3.zero; //dont rotate
            yield return new WaitForFixedUpdate();
        }

        watchingLastPos = false;
    }

    IEnumerator TakeCover()
    {
        hiding = true;
        Vector3 originalScale = transform.localScale;
        Vector3 hideScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.6f, transform.localScale.z);
        while (transform.localScale.y > hideScale.y)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, hideScale, 0.5f*Time.deltaTime);
            yield return null;
        }
        //after scaling down, hide for a bit
        yield return new WaitForSeconds(Random.Range(2f, 8f));
        //then scale back up and stop hiding
        while (transform.localScale.y < originalScale.y)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale, 0.5f * Time.deltaTime);
            yield return null;
        }
        hiding = false;
    }
}
