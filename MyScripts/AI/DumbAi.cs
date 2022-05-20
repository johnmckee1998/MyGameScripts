using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DumbAi : MonoBehaviour
{
    protected NavMeshAgent navAgent;
    protected Rigidbody navRigid;

    [Header("General Attributes")]
    public float health = 100f;
    public bool alive;
    protected bool hitPlayer = false;

    public float MoveSpeed = 5f;
    public float hitDamage = 10f;
    public float hitTime = 0.5f;
    [Tooltip("How close does the player need to be to register a hit")]
    public float hitBuffer = 1.2f;


    [Header("Audio Files")]
    public AudioSource idleSound;
    public AudioSource hitSound;

    protected UniversalStats uniStats;
    //protected bool usesUniStats = false;
    public Collider[] enableColliders;
    public Collider[] disableColliders;
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navRigid = GetComponent<Rigidbody>();
        alive = true;
        StartCoroutine(Chase());
        navAgent.speed = MoveSpeed;
        //navAgent.angularSpeed = 360;
        

        if(GetComponent<UniversalStats>() != false)
        {
            //usesUniStats = true;
            uniStats = GetComponent<UniversalStats>();
        }
    }


    private void Update()
    {
        health = uniStats.health;
        if (health <= 0)
        {
            alive = false;
            OnDeath();
        }
    }


    // Update is called once per frame
    protected virtual IEnumerator Chase()
    {
        yield return new WaitForFixedUpdate();
        while (alive)
        {
            if (hitPlayer)
                navAgent.destination = transform.position;
            else 
                navAgent.destination = CharacterControllerScript.instance.transform.position;

            //yield return new WaitForSeconds(0.05f);

            if (!hitPlayer && Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < hitBuffer)
                PlayerHit();


            yield return null;
        }
    }

    protected virtual void OnDeath()
    {
        navAgent.enabled = false;
        navRigid.freezeRotation = false;
        navRigid.constraints = RigidbodyConstraints.None;
        transform.GetComponent<DumbAi>().enabled = false;
        gameObject.layer = 16;

        WaveManagerScript.KillCount++;
        idleSound.Stop();

        if (PickupManager.instance != null)
        {
            GameObject pickup = PickupManager.instance.PickupFunction();
            if (pickup != null)
                Instantiate(pickup, transform.position, transform.rotation);
        }
                
        
        for (int i = 0; i < disableColliders.Length; i++)
            disableColliders[i].enabled = false;
        for (int i = 0; i < enableColliders.Length; i++)
            enableColliders[i].enabled = true;

        //navRigid.AddForce(Random.insideUnitCircle.normalized * 20f); //add random force on death so they dont stay standing

        Destroy(gameObject, 10);
    }

    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("e");
        if (collision.gameObject.tag == "Player")
        {
            if (!hitPlayer && alive)
            {
                PlayerHit();
            }
        }
    }

    protected virtual void PlayerHit()
    {
        hitPlayer = true;
        CharacterControllerScript.instance.health -= hitDamage;

        navRigid.constraints = RigidbodyConstraints.None;
        navRigid.freezeRotation = true;
        hitSound.PlayOneShot(hitSound.clip, 0.15f);

        StartCoroutine(HitReset());
        //pHitSound.Play();
        //Debug.Log("player hit");
    }

    protected virtual IEnumerator HitReset()
    {
        //Debug.Log("WAIT");
        yield return new WaitForSeconds(hitTime);
        hitPlayer = false;
        navRigid.constraints = RigidbodyConstraints.FreezePosition;
    }


}
