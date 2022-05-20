using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleTDEnemy : MonoBehaviour
{
    public float damagePerSec =10f;
    public float damageRange = 2f;
    public LineRenderer laserLine;
    private NavMeshAgent navAgent;
    private int targetIndex;
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);

        navAgent.SetDestination(TDPlayerBase.instance.buildings[targetIndex].building.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (TDPlayerBase.instance.buildings[targetIndex].buildingHealth <= 0)
        {//current target dead, update
            targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
            navAgent.SetDestination(TDPlayerBase.instance.buildings[targetIndex].building.position);
        }

        Vector3 targetPoint = TDPlayerBase.instance.buildings[targetIndex].building.position;

        if (Vector3.Distance(transform.position, targetPoint) <= damageRange) //if in range of dest
        {

            TDPlayerBase.instance.DamageBase(damagePerSec * Time.fixedDeltaTime, targetIndex);

            if (laserLine != null)
            {
                laserLine.gameObject.SetActive(true);
                laserLine.SetPosition(1, laserLine.transform.InverseTransformPoint(new Vector3(targetPoint.x, transform.position.y, targetPoint.z)));
            }
        }
        else if (laserLine != null)
            laserLine.gameObject.SetActive(false); //disable laser when not in range
    }
}
