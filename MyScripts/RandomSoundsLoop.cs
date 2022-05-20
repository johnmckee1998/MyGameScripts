using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSoundsLoop : MonoBehaviour
{
    public AudioSource[] sounds;
    private bool soundIsPlaying = false;
    private int soundSelect;
    private int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        soundSelect = Random.Range(0, sounds.Length - 1);

        sounds[soundSelect].Play();
        soundIsPlaying = true;

        StartCoroutine(PlayRanSound());
    }

    // Update is called once per frame
    IEnumerator PlayRanSound()
    {
        while (true)
        {
            if(counter == sounds.Length - 1) //Randomise
            {
                for (int i = 0; i < sounds.Length - 1; i++)
                {
                    int j = Random.Range(i, sounds.Length-1);
                    AudioSource temp = sounds[i];
                    sounds[i] = sounds[j];
                    sounds[j] = temp;
                }
                counter = 0;
            }


            soundIsPlaying = false;
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].isPlaying)
                    soundIsPlaying = true;
            }


            if (!soundIsPlaying)
            {
                sounds[counter].Play();
                counter++;
            }

            soundSelect = Random.Range(1, 3);
            yield return new WaitForSeconds(soundSelect);



            /*
            soundIsPlaying = false;
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].isPlaying)
                    soundIsPlaying = true;
            }

            soundSelect = Random.Range(1, 4);
            yield return new WaitForSeconds(soundSelect);

            if (!soundIsPlaying)
            {
                soundSelect = Random.Range(0, sounds.Length - 1);

                sounds[soundSelect].Play();
            }*/
        }
        
    }
}
