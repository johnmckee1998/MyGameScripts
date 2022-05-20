using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDRollingTank : MonoBehaviour
{
    //public float moveSpeed = 10f;
    public float radius = 2f;
    private NavMeshAgent navAgent;

    private Vector3 currentDest;
    private Bounds bounds;
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        bounds = TowerDefenceWaveManager.instance.bomberZone.bounds;
        UpdateDest();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(navAgent.destination, transform.position) <= radius*2f)
            UpdateDest();
    }

    private void UpdateDest()
    {
        Debug.Log("Update");
        currentDest = new Vector3(
        Random.Range(bounds.min.x, bounds.max.x),
        transform.position.y,
        Random.Range(bounds.min.z, bounds.max.z));
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(currentDest, out navHit, radius + 1f, NavMesh.AllAreas))
        {
            currentDest = navHit.position;
            navAgent.SetDestination(currentDest);
            return;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if(navAgent!=null)
            Gizmos.DrawSphere(navAgent.destination, 1f);
    }
}
