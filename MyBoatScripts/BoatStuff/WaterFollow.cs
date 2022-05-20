using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFollow : MonoBehaviour
{
    public Transform player;
    public float tileSize = 500f;
    [Tooltip("How many tileSizes to jump - 1 means once you are tileSize away it moves tileSize in length")]
    public float tilesDistToJump = 2;


    public float scrollDist = 500f;
    [Tooltip("Used to properly offset it in relation to other tiles")]
    public float scrollModifier = 2f;

    public bool debugLog;

    private void Start()
    {
        if(player == null)
            player = CharacterControllerScript.instance.transform;
    }


    // Update is called once per frame
    void Update()
    {
        if (debugLog)
            Debug.Log("X Dist: " + (transform.position.x - player.position.x) + " Z Dist: " + (transform.position.z - player.position.z));

        //Z checks
        if(transform.position.z - player.position.z >scrollDist)
        {
            transform.position -= new Vector3(0, 0, scrollDist*scrollModifier);
        }
        else if(transform.position.z - player.position.z < -scrollDist)
        {
            transform.position += new Vector3(0, 0, scrollDist*scrollModifier);
        }

        //X checks
        if(transform.position.x - player.position.x > scrollDist)
        {
            transform.position -= new Vector3(scrollDist*scrollModifier, 0, 0);
        }
        else if(transform.position.x - player.position.x < -scrollDist)
        {
            transform.position += new Vector3(scrollDist*scrollModifier, 0, 0);
        }
    }
    
}
