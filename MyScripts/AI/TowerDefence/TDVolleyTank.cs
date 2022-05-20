using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDVolleyTank : MonoBehaviour
{
    [System.Serializable]
    public struct BarrelSet
    {
        public Transform[] bulletSpawns;
        //add firing vfx
        public AudioSource shotSFX;
    }

    public BarrelSet[] volleys;
    public GameObject projectile;
    [Tooltip("the x and z range it can move from its spawn point")]
    public Vector2 moveRangeMin = Vector2.one;
    public Vector2 moveRangeMax = Vector2.one; //maybe have a min/max separate
    [Space]
    public float shotDelay = 0.333f;
    public float volleyDelay = 1f;
    public float volleyReset;

    private NavMeshAgent navAgent;
    private Vector3 startPos;
    private bool firing;
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        startPos = transform.position;
        NewDest();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!firing)
        {
            if (Vector3.Distance(navAgent.destination, transform.position) < 1f)
            {
                navAgent.destination = transform.position;
                transform.LookAt(TDPlayerBase.instance.buildings[0].building.position);//look at base building - maybe make it look at player or something idk
                StartCoroutine(FireVolley());
            }
        }
    }

    private void NewDest()
    {
        Vector3 dest = new Vector3(startPos.x + (Random.Range(moveRangeMin.x, moveRangeMax.x)), startPos.y, startPos.z + (Random.Range(moveRangeMin.y, moveRangeMax.y)));//probably a better way would be to add the random range multiplied by forward direction
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(dest, out navHit, 3f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(navHit.position);
        }
        else
        {
            Debug.Log("Failed to find move pos (VolleyTank)");
            if (NavMesh.SamplePosition(dest, out navHit, 6f, NavMesh.AllAreas)) //double the range
            {
                navAgent.SetDestination(navHit.position);
            }
            else
            {
                Debug.Log("Bruh");
            }

        }
    }

    private IEnumerator FireVolley()
    {
        firing = true;
        /*
        for(int i=0; i<volleys.Length; i++)
        {
            //fire set of barrels
            for(int j=0; j < volleys[i].bulletSpawns.Length; j++)
            {
                GameObject g = Instantiate(projectile, volleys[i].bulletSpawns[j].position, volleys[i].bulletSpawns[j].rotation);
                yield return new WaitForSeconds(shotDelay);
            }
            volleys[i].shotSFX.Play();
            yield return new WaitForSeconds(volleyDelay); //delay next set of barrels
        }
        */
        yield return new WaitForSeconds(volleyReset);
        NewDest();
        firing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 p1 = new Vector3(transform.position.x + moveRangeMin.x, transform.position.y, transform.position.z + moveRangeMin.y);
        Vector3 p2 = new Vector3(transform.position.x + moveRangeMax.x, transform.position.y, transform.position.z + moveRangeMin.y);
        Vector3 p3 = new Vector3(transform.position.x + moveRangeMin.x, transform.position.y, transform.position.z + moveRangeMax.y);
        Vector3 p4 = new Vector3(transform.position.x + moveRangeMax.x, transform.position.y, transform.position.z + moveRangeMax.y);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p1, p3);
        Gizmos.DrawLine(p2, p4);
        Gizmos.DrawLine(p3, p4);
    }
}
