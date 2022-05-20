using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedToggle : MonoBehaviour
{
    public GameObject[] set1;
    public GameObject[] set2;
    [Space]
    public GameObject[] preview1;
    public GameObject[] preview2;

    [Space]

    [Header("Switch Settings")]
    public float switchTime = 5f;
    [Tooltip("After switching, how long before the sound is played. Should end up like this: switchSoundTime + sound length = switchTime")]
    public float switchSoundTime = 2f;
    public AudioSource switchSound;

    private bool toggle = true;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < set1.Length; i++) //enable all of set 1
            set1[i].SetActive(true);

        for (int i = 0; i < set2.Length; i++) //disable all of set 2
            set2[i].SetActive(false);

        for (int i = 0; i < preview1.Length; i++) //disable all of preview set 1
            preview1[i].SetActive(false);

        for (int i = 0; i < preview2.Length; i++) //enable all of preview set 2
            preview2[i].SetActive(true);

        StartCoroutine(ToggleSets());

        if(switchSoundTime > switchTime)
        {
            Debug.Log("You set the wrong switch time retard " + gameObject.name);
            switchSoundTime = Time.fixedDeltaTime;
        }
    }

    public void StopSwitching()
    {
        toggle = false;
    }

    private IEnumerator ToggleSets()
    {
        yield return new WaitForSeconds(switchTime);

        while (toggle)
        {
            if (switchSound != null)
                switchSound.Play();

            yield return new WaitForSeconds(switchTime - switchSoundTime);

            for (int i = 0; i < set1.Length; i++) //enable all of set 1
                set1[i].SetActive(!set1[i].activeSelf);

            for (int i = 0; i < set2.Length; i++) //disable all of set 2
                set2[i].SetActive(!set2[i].activeSelf);

            for (int i = 0; i < preview1.Length; i++) //disable all of preview set 1
                preview1[i].SetActive(!preview1[i].activeSelf);

            for (int i = 0; i < preview2.Length; i++) //enable all of preview set 2
                preview2[i].SetActive(!preview2[i].activeSelf);

            yield return new WaitForSeconds(switchSoundTime);
        }
    }
}
