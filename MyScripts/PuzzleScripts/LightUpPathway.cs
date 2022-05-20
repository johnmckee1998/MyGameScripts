using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightUpPathway : MonoBehaviour
{
    /* this is not the most efficient method - requires evey tile to have a script 
     * A better way would be for all tiles to be in an array and have just 1 script 
     * The script would simply check player position and highlight tiles near it
     * 
     * 
     * 
     */


    private Renderer[] tiles;
    private Material[] onMats;

    public Material offMat;

    private bool playerInside;

    public float tileSize;
    // Start is called before the first frame update
    void Start()
    {
        tiles = GetComponentsInChildren<Renderer>();
        onMats = new Material[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            onMats[i] = tiles[i].material; // record their on material
            tiles[i].material = offMat; //turn off material
        }


    }

    private void FixedUpdate()
    {
        if (playerInside)
        {
            Vector3 playerPos = CharacterControllerScript.instance.transform.position;
            //playerPos = transform.InverseTransformPoint(playerPos); //get player pos in localSpace

            for(int i =0; i<tiles.Length; i++)
            {
                if (Vector3.Distance(tiles[i].transform.position, playerPos) <= tileSize)
                    tiles[i].material = onMats[i];
                else
                    tiles[i].material = offMat;
            }
        }
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            playerInside = false;

            for (int i = 0; i < tiles.Length; i++)
                tiles[i].material = offMat;
        }
    }
}
