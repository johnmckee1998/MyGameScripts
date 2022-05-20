using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundActivator : MonoBehaviour
{
    [Tooltip("At the start of which round do the connected objects activate")]
    public int roundActive = 5;

    public GameObject[] ObjectsToActivate;
    public GameObject[] ObjectsToDeActivate;

    private bool act = false; 
    // Start is called before the first frame update
    void Start()
    {
        //if (ObjectsToActivate == null)
            //Debug.LogError("CONNECTED OBJECTS REQUIRED: " + gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (!act && WaveManagerScript.allDead && WaveManagerScript.roundCount >= roundActive)
        {
            foreach(GameObject g in ObjectsToActivate)
            {
                g.SetActive(true);
            }

            foreach (GameObject g in ObjectsToDeActivate)
            {
                g.SetActive(false);
            }

            act = true;
        }
    }
}
