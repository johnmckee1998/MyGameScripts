using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PickupObjective : ObjectiveBase
{
    private AudioSource pickupSound;

    private void Start()
    {
        pickupSound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            AudioSource.PlayClipAtPoint(pickupSound.clip, transform.position, pickupSound.volume);
            ObjectiveComplete();
        }
    }
}
