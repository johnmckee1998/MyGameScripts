using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperHot : MonoBehaviour
{
    public float timeScale = 1f;
    private bool enable;

    private float fixedTimeBackup;
    // Start is called before the first frame update
    void Start()
    {
        fixedTimeBackup = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("u"))
            enable = !enable;

        if (Time.timeScale > 0)
        {
            if (enable)
            {
                Time.timeScale = timeScale;
                Time.fixedDeltaTime = fixedTimeBackup * timeScale;
            }
            else
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = fixedTimeBackup;
            }
        }
    }
}
