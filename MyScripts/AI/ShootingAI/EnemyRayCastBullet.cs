using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRayCastBullet : MonoBehaviour
{
    //this is just used for bullets pass sounds, use raycastbullet for the main bullet shit
    private AudioSource asource;
    public AudioClip[] bulletPass;
    // Start is called before the first frame update
    void Start()
    {
        asource = GetComponent<AudioSource>();
    }

    

    public void PlayPassSound()
    {
        int ran = Random.Range(0, bulletPass.Length);
        asource.clip = bulletPass[ran];
        AudioSource.PlayClipAtPoint(bulletPass[ran], transform.position, asource.volume);
    }
}
