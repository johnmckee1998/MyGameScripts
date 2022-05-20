using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralPickupPlacement : MonoBehaviour
{
    public float XMaxlimit;
    public float ZMaxlimit;
    public int AmountToSpawn;

    public GameObject[] pickups;

    public void PlacePickups()
    {
        for (int i = 0; i < AmountToSpawn; i++)
        {
            int rand = Random.Range(0, pickups.Length);
            float randX = Random.Range(transform.position.x, transform.position.x + XMaxlimit);
            float randZ = Random.Range(transform.position.z, transform.position.z + ZMaxlimit);
            Vector3 point = new Vector3(randX, transform.position.y + 1f, randZ);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, 3f, NavMesh.AllAreas))//if a point is found, spawn enemy
            {
                Instantiate(pickups[rand], new Vector3(hit.position.x, hit.position.y + 1f, hit.position.z), transform.rotation); //THIS IS THE DIFFERENCE TO ENEMY PLACER - does not set pickups as child objects - enemies need to be children so enemy checker can work
            }
            else
            {
                //Debug.Log("No Point Found");
                i--;
            }

        }
    }

    public void PickupsSpawnRate(string p)
    {
        if (int.TryParse(p, out AmountToSpawn))
            Debug.Log("Success Pickup: " + AmountToSpawn);
        else
            Debug.Log("Fail Pickup");
    }

    public void SetSize(float bSize, float x, float z)
    {
        XMaxlimit = bSize * x;
        ZMaxlimit = bSize * z;
    }
}
