using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDTankScript : MonoBehaviour
{
    public float shootDist = 5f;
    public float rotSpeed = 1f;
    private NavMeshAgent navAgent;
    private int targetIndex;
    TDPlayerBase playerBase;
    // Start is called before the first frame update
    void Start()
    {
        playerBase = TDPlayerBase.instance;

        navAgent = GetComponent<NavMeshAgent>();

        targetIndex = playerBase.GetClosestBase(transform.position);

        navAgent.SetDestination(playerBase.buildings[targetIndex].building.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (playerBase.buildings[targetIndex].buildingHealth <= 0)
        {//current target dead, update
            targetIndex = playerBase.GetClosestBase(transform.position);
            navAgent.SetDestination(playerBase.buildings[targetIndex].building.position);
        }

        if (Vector3.Distance(transform.position, playerBase.buildings[targetIndex].building.position) <= shootDist) //if in range of dest
        {
            if(navAgent.destination!=transform.position)
                navAgent.SetDestination(transform.position);
            /*
            //rotate to look at dest
            Vector3 dirToTarget = transform.position - playerBase.buildings[targetIndex].building.position;
            dirToTarget.Normalize();
            Vector3 lookRot = Quaternion.LookRotation(dirToTarget, transform.up).eulerAngles;
            transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, lookRot, rotSpeed*Time.fixedDeltaTime,0);
            */
        }
    }
}
