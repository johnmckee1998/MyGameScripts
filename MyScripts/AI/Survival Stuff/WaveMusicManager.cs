using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMusicManager : MonoBehaviour
{
    public AudioSource waveStart;
    public AudioSource waveMusic;
    public AudioSource waveEnd;
    public AudioSource intermission;

    public float waveStartLength;
    

    public void BeginWave()
    {
        StartCoroutine(WaveBegin());
    }

    private IEnumerator WaveBegin()
    {
        waveStart.Play();
        waveEnd.Stop();
        if(intermission!=null)
            intermission.Stop();
        yield return new WaitForSeconds(waveStartLength);
        waveStart.Stop();
        waveMusic.Play();
    }

    public void EndWave()
    {
        waveEnd.Play();
        waveMusic.Stop();
    }

    
}
