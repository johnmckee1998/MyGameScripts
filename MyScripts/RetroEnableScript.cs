using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RetroEnableScript : MonoBehaviour
{



    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("RetroEnabled", 0) == 1)
            gameObject.GetComponent<Volume>().enabled = true;
        else
            gameObject.GetComponent<Volume>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt("RetroEnabled", 0) == 1)
            gameObject.GetComponent<Volume>().enabled = true;
        else
            gameObject.GetComponent<Volume>().enabled = false;
    }
}
