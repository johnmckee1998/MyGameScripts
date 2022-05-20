using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundChanges : MonoBehaviour
{
    private AudioSource[] ambientSounds;
    private SixShooterScript playerShoot;
    private bool wasInside = false;
    // Start is called before the first frame update
    void Start()
    {
        ambientSounds = GetComponents<AudioSource>();
        playerShoot = FindObjectOfType<SixShooterScript>();

        wasInside = playerShoot.isInside;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerShoot.isInside)
        {
            foreach (AudioSource aud in ambientSounds)
                aud.volume = 0.5f;

            wasInside = true;
        }
        else if (wasInside)
        {
            wasInside = false;

            foreach (AudioSource audi in ambientSounds)
                audi.volume = 1.0f;
        }

    }
}
