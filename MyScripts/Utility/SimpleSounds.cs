using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleSounds : MonoBehaviour
{
    private AudioSource audSource;
    public AudioClip[] sounds;


    private void Start()
    {
        audSource = GetComponent<AudioSource>();
    }

    public void PlaySound(int i)
    {
        if (i < sounds.Length)
            audSource.PlayOneShot(sounds[i], audSource.volume);
    }
    
    public AudioClip GetSound(int i)
    {
        if (i < sounds.Length)
            return sounds[i];
        else
            return null;
    }

}
