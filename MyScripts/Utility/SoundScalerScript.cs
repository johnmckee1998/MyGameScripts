using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundScalerScript : MonoBehaviour
{
    public GameObject player;
    private AudioSource sound;
    public bool prnt = false;
    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(prnt)
            Debug.Log(gameObject.name + " " +  Mathf.Abs(transform.position.x - player.transform.position.x));
        if (Mathf.Abs(transform.position.x - player.transform.position.x) <= 30f)
        {
            float soundVol = Mathf.Abs((Mathf.Abs(transform.position.x - player.transform.position.x) / 30) - 1);
            soundVol = Mathf.Clamp01(soundVol);
            //Debug.Log(soundVol);
            sound.volume = soundVol;
        }
    }
}
