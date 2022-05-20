using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoundaryScript : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        UniversalStats unistat = other.GetComponent<UniversalStats>();

        if (unistat!=null)
        {
            unistat.health = 0;
            
        }
    }
}
