using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEnabledScript : MonoBehaviour
{
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > 120f)
        {
            transform.GetComponent<SmartAI>().enabled = false;
        }
        else
        {
            transform.GetComponent<SmartAI>().enabled = false;
        }
    }
}
