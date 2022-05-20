using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDEnemy : MonoBehaviour
{
    [Tooltip("Layers to ignore when doing sight check")]
    public LayerMask raycastIgnore;


    protected CharacterControllerScript player;

    /*
     * This Class includes a series of basic functions used by many Tower Defence enemies
     * 
     * 
     * */


    protected bool CheckSightPlayer()
    {
        if (player == null)
            player = CharacterControllerScript.instance;

        Vector3 pDir = player.pCam.transform.position - transform.position;
        RaycastHit rHit;
        if (Physics.Raycast(transform.position, pDir.normalized, out rHit, 50f, ~raycastIgnore))
        {
            if (rHit.transform.tag.Equals("Player"))
                return true;
            else
                return false;
        }


        return false;
    }
}
