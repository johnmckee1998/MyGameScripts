using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    public float modifier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            CharacterControllerScript.instance.SetSpeedModifier(modifier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterControllerScript.instance.ResetSpeedModifier();
        }
    }
}
