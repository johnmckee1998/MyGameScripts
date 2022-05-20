using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryScript : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            try {
                other.gameObject.GetComponent<CharacterControllerScript>().health = 0;
            }
            catch
            {
                other.gameObject.GetComponent<CharacterControllerScriptRigidBody>().health = 0;
            }
            Debug.Log(other.tag + " Hit boundary");
            }
    }


}
