using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDMeleeEnemy : MonoBehaviour
{
    protected NavMeshAgent navAgent;

    [Header("General Attributes")]
    public float health = 100f;
    public bool alive;
    protected bool hit = false;

    public float MoveSpeed = 5f;
    public float hitDamage = 10f;
    public float hitTime = 0.5f;
    [Tooltip("How close does the player need to be to register a hit")]
    public float hitBuffer = 1.2f;


    [Header("Audio")]
    
    public AudioSource hitSound;

    protected UniversalStats uniStats;
    //protected bool usesUniStats = false;

    private Transform target;
    private Vector3 dest;
    private UniversalStats tarStats;
    private bool tarIsPlayer; //tracks if target is player or bot
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        alive = true;
        StartCoroutine(Chase());
        navAgent.speed = MoveSpeed;
        //navAgent.angularSpeed = 360;


        if (GetComponent<UniversalStats>() != false)
        {
            //usesUniStats = true;
            uniStats = GetComponent<UniversalStats>();
        }

        SetNewTargetAndDest();
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
            if ((tarIsPlayer && CharacterControllerScript.instance.health <= 0) || (!tarIsPlayer && tarStats.health <= 0)) //target/player is dead, find new one
                SetNewTargetAndDest();

            if (hit)
                navAgent.SetDestination(transform.position);
            else
                navAgent.SetDestination(target.position);

            //yield return new WaitForSeconds(0.05f);

            if (!hit && Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < hitBuffer)
                PlayerHit();


            yield return new WaitForFixedUpdate();
        }
    }

    protected virtual void OnDeath()
    {
        navAgent.enabled = false;
        gameObject.layer = 16;
        enabled = false;

        //navRigid.AddForce(Random.insideUnitCircle.normalized * 20f); //add random force on death so they dont stay standing

        Destroy(gameObject, 10);
    }

    protected virtual void PlayerHit()
    {
        hit = true;

        if (tarIsPlayer)
            CharacterControllerScript.instance.health -= hitDamage;
        else
            tarStats.health -= hitDamage;
        hitSound.PlayOneShot(hitSound.clip, 0.15f);

        StartCoroutine(HitReset());
        //pHitSound.Play();
        //Debug.Log("player hit");
    }

    protected virtual IEnumerator HitReset()
    {
        //Debug.Log("WAIT");
        yield return new WaitForSeconds(hitTime);
        hit = false;
    }

    private void SetNewTargetAndDest()
    {
        target = TowerDefenceWaveManager.instance.GetClosestPlayerTarget(transform.position);

        if (transform == CharacterControllerScript.instance.transform) //check if target is player
            tarIsPlayer = true;
        else
        {
            tarIsPlayer = false;
            tarStats = target.GetComponent<UniversalStats>();
        }

        navAgent.SetDestination(dest);
    }
}
