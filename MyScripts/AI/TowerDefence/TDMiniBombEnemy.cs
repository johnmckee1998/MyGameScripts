using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDMiniBombEnemy : TDEnemy
{
    public float maxAliveTime = 60f;

    public GameObject explosionObj;
    private Transform target;
    private NavMeshAgent navAgent;

    private bool idle;
    // Start is called before the first frame update
    void Start()
    {
        player = CharacterControllerScript.instance;
        navAgent = GetComponent<NavMeshAgent>();

        GetTarget();
    }

    // Update is called once per frame
    void Update()
    {
        maxAliveTime -= Time.deltaTime;
        if (maxAliveTime <= 0)
            BlowUp();

        
        
        if (!idle && target!=null)
            if(navAgent.destination != target.position)
                navAgent.SetDestination(target.position);

        if (!idle && Vector3.Distance(transform.position, navAgent.destination) < 0.75f)
            BlowUp();

    }

    private void FixedUpdate()
    {
        GetTarget();
    }

    private void GetTarget()
    {
        target = TowerDefenceWaveManager.instance.GetClosestPlayerTarget(transform.position);
        if (target == null)
        {
            idle = true;
            target = transform;
        }
        else
            idle = false;
    }

    private void BlowUp()
    {
        Instantiate(explosionObj, transform.position, transform.rotation);
        Destroy(gameObject);
        //Debug.Log("Boom!");
    }
}
