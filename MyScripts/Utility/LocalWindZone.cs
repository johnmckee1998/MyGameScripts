using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalWindZone : MonoBehaviour
{
    [Tooltip("The windforce applied when enetering trigger - > note: requires a globalstats object to be present")]
    public Vector3 windForce;

    //Note BIG ISSUE - this relies on the player entering the area, 
    //if you shoot from one windzone to another the wind from the area that you are in is applied, 
    //a better way would be fore the bullet to detect changes in wind so that it cn adapt to different zones
    private Vector3 prevWindForce;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterControllerScript>() != null)
            if (GlobalStats.instance != null)
            {
                prevWindForce = GlobalStats.instance.windforce;
                GlobalStats.instance.windforce = windForce;
            }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterControllerScript>() != null)
            if (GlobalStats.instance != null)
                GlobalStats.instance.windforce = prevWindForce;
            
    }
}
