using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionScript : MonoBehaviour
{
    public GameObject player;
    private CharacterControllerScript playerControl;
    private float maxDetect;
    // Update is called once per frame
    private void Start()
    {
        playerControl = player.GetComponent<CharacterControllerScript>();

        maxDetect = 0;
    }



    void FixedUpdate()
    {
        maxDetect = 0;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).tag == "SmartEnemy")
            {
                float curDetect = transform.GetChild(i).GetComponent<SmartAI>().getDetect();
                if (curDetect > maxDetect)
                    maxDetect = curDetect;
            }
        }

        playerControl.SetDetect(maxDetect);
    }
}
