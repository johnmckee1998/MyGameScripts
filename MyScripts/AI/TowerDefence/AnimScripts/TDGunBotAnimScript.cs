using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDGunBotAnimScript : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public BotShooting gunScript;
    public Animator anim;
    //these bools prevent constantly trying to change anim bools - bit of performance saving i think
    private bool running;
    private bool shooting;
    private bool idle;

    private Vector3 prevPos;
    private float speed;
    // Start is called before the first frame update
    void Start()
    {
        //if (navAgent == null)
        //    navAgent = GetComponent<NavMeshAgent>();
        if (gunScript == null)
            gunScript = GetComponentInChildren<BotShooting>();
        if (anim == null)
            anim = GetComponent<Animator>();
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.enabled)
        {
            speed = Vector3.Distance(transform.position, prevPos) / Time.deltaTime;


            if (speed > 0.1f) //using speed rather than navagent.isstopped is more reliable as someimes the navagent will keep running even when reached dest
                RunAnim();
            else if (gunScript.shoot && !gunScript.IsReloading())
                ShootAnim();
            else if ((running || shooting) && !idle)
                IdleAnim();


            prevPos = transform.position;
            if (running)
                gunScript.shoot = false;
        }
    }

    private void RunAnim()
    {
        
        if (!running)
        {
            anim.SetBool("Run", true);
            anim.SetBool("Shoot", false);
            anim.SetBool("Idle", false);
            running = true;
            shooting = false;
            idle = false;
            //Debug.Log("Run");
        }
    }

    private void IdleAnim()
    {
        anim.SetBool("Run", false);
        anim.SetBool("Shoot", false);
        anim.SetBool("Idle", true);
        running = false;
        shooting = false;
        idle = true;
        //Debug.Log("Idle");
    }

    private void ShootAnim()
    {
        if (!shooting)
        {
            anim.SetBool("Run", false);
            anim.SetBool("Shoot", true);
            anim.SetBool("Idle", false);
            running = false;
            shooting = true;
            idle = false;
            //Debug.Log("Shoot");
        }
    }

    //others (using animation rigging?):
    // crouch?
    // ads?
    // reload
    // die
}
