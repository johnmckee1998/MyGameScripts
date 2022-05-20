using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour
{
    public GameObject player;
    public float health = 100;
    public float hitDamage = 20;
    public float ShootDist = 35f;
    [Tooltip("Aiming rotation lerp speed -> is divided by 360 and timesd by deltatime")]
    public float rotSpeed = 720;

    private bool inShotRange = false;

    private AudioSource hitSound;
    private AudioSource activeSound;
    private AudioSource idleSound;
    private AudioSource pHitSound;

    private bool hitPlayer = false;
    private float counting = 0;

    private bool nearPlayer = false;
    private bool seePlayer = false;
    private bool isDead = false;
    private bool idle = true;

    private Vector3 centerPos;

    private UniversalStats uniStats;

    private CharacterControllerScript pScript;

    private BotShooting shootScript;

    private bool turnLeft = true;

    private Vector3 playerTravelDir;
    private CharacterController pControl;

    public GameObject turretParent;

    public float minRotation = -60f;
    public float maxRotation = 60f;
    [Tooltip("Idle rotation speed in magnitude")]
    public float rotationSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource[] aSources = GetComponents<AudioSource>();
        //hitSound = aSources[0];
        activeSound = aSources[0];
        idleSound = aSources[1];
        //pHitSound = aSources[3];

        activeSound.Stop();
        idleSound.Play();


        if (player == null)
            player = GameObject.FindWithTag("Player");

        centerPos = transform.position;

        uniStats = GetComponentInParent<UniversalStats>();

        pScript = player.GetComponent<CharacterControllerScript>();

        shootScript = GetComponentInChildren<BotShooting>();

        pControl = player.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerTravelDir = pControl.velocity/5; //used to offset the look position so the turret shoots where the player will be, so you cant just walk in a circle to dodge bullets
                                                //the /5 bit is coz using the velocity directly is too strong of an effect, /5 seemed to work well though it is based off of nothing in particular
        
        //hitSound.pitch = Time.timeScale;
        activeSound.pitch = Time.timeScale;
        idleSound.pitch = Time.timeScale;

        if (uniStats.health < health)//update health if damage is done to unistats
            health = uniStats.health;

        if (health <= 0)//die
        {
            gameObject.GetComponent<TurretAI>().enabled = false;
            gameObject.GetComponent<Rigidbody>().freezeRotation = false;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            isDead = true;
            idleSound.Stop();
            activeSound.Stop();

            gameObject.layer = 16;
        }

        //if (player.GetComponent<CharacterControllerScript>().getCrouch())
        //    realDetect = detectDistance / 2;
        //else
        //    realDetect = detectDistance;


        CheckSee();

        float DistFromP = Vector3.Distance(transform.position, player.transform.position);
        //if (DistFromP <= ShootDist)
        //    inShotRange = true;
        //else
        //    inShotRange = false;


        if (inShotRange && seePlayer)
        {//if the player is in range and is visible, then the turret is no longer idle
            if (idle)//switch sounds (only on change in state)
            {
                idleSound.Stop();
                activeSound.Play();
            }


            idle = false;
        }
        else
        {
            if (!idle)//Switch sounds
            {
                idleSound.Play();
                activeSound.Stop();
            }
            idle = true;

            //wander sound ---------------------------------- already done elsewhere
            //stop reee sound
        }

        

        shootScript.shoot = false;
        if (!isDead && !idle ) //Shoot da bich
        {

            Vector3 Ppos = player.transform.position;
            if (pScript.getCrouch())
                Ppos.y += 0.5f; //Was -0.5f when crouching, no change when not - cahnge to alter this coz of weird behaviour witht he new coruching system
            else
                Ppos.y += 1.0f;

            //transform.LookAt(Ppos);
            //Smooth rotation
            Vector3 dirToPlayer = Ppos - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(dirToPlayer + playerTravelDir);

            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * (rotSpeed / 360f));

            shootScript.shoot = true;//shoot


            //idleSound.Stop();
            //activeSound.Play();

            //Debug.Log("SEE");
            //play reee sound-------------------
            //stop wander sound


        }
        else if (!isDead && idle) //idle behaviour
        {
            float angle = transform.localEulerAngles.y;
            angle = (angle > 180) ? angle - 360 : angle; //account for negative rotation
            if (angle >= maxRotation)
                turnLeft = false;
            else if (angle <= minRotation)
                turnLeft = true;

            if (turnLeft)
                transform.localEulerAngles += transform.up * Time.deltaTime * rotationSpeed;
            else
                transform.localEulerAngles -= transform.up * Time.deltaTime * rotationSpeed;
            //spin or some shit
            //transform.localEulerAngles += transform.up;
            //idleSound.Play();
            //activeSound.Stop();
        }

     
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ShootDist);
        Gizmos.color = Color.green;
        if (player != null)
        {
            Vector3 pDir = player.transform.position - transform.position;
            pDir.y += 1f;
            Gizmos.DrawRay(transform.position, pDir);
        }
        //Gizmos.color = Color.gray;
        //Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y-0.5f, transform.position.z), 2.0f);
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
            pDir.y += 0.5f; //Was -0.5f when crouching, no change when not - cahnge to alter this coz of weird behaviour witht he new coruching system
        else
            pDir.y += 1.0f;

        if (Physics.Raycast(transform.position, pDir, out sight, Mathf.Infinity))
        {
            if (sight.collider.tag == "Player")
                seePlayer = true;
            
            else
                seePlayer = false;
        }
    }

    public void TargetPlayer(bool b)
    {
        inShotRange = b;
    }
}
