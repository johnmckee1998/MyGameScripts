using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorTrigger : MonoBehaviour
{
    public bool UseReverb = false;
    public bool IsSixShooter = true;

    public AudioReverbPreset ReverbType;
    private AudioReverbPreset PrevReverb;

    AudioReverbFilter A;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(IsSixShooter)
                other.GetComponentInChildren<SixShooterScript>().isInside = true;

            if (UseReverb)
            {
                A = other.GetComponentInChildren<AudioReverbFilter>();
                PrevReverb = A.reverbPreset;
                A.reverbPreset = ReverbType;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (IsSixShooter)
                other.GetComponentInChildren<SixShooterScript>().isInside = false;

            //causes issue when exiting one reverb and entering another
            //if (UseReverb)
            //    A.reverbPreset = PrevReverb;
            
        }
    }
}
