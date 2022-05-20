using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StalkerScript : MonoBehaviour
{
    private NavMeshAgent navAgent;
    public GameObject player;
    public bool follow = false;
    public float followDist = 2f;
    private Renderer ren;
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        ren = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (follow && Vector3.Distance(player.transform.position, transform.position) > followDist && !ren.isVisible)
        {
            navAgent.destination = player.transform.position; //put condition so only move when player isnt looking
        }
        else
            navAgent.destination = transform.position;


        //Debug.Log("Is Seen? " + ren.isVisible);
    }
}
