using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeZoneScript : MonoBehaviour
{
    public float timeScaleEffect = 1.0f;

    /*private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Time.timeScale = timeScaleEffect;
        }
    }*/

    //CHANGE FIXEDDELTATIME COZ IT SCALES WITH TIMESCALE
    private float startDeltaTime;

    private void Start()
    {
        startDeltaTime = Time.fixedDeltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        Time.fixedDeltaTime = (startDeltaTime * timeScaleEffect);
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log(Time.timeScale + " T");
            if (Time.timeScale != timeScaleEffect && Time.timeScale >0)
            {
                //If timesacleEffect is faster than current timescale
                if (Time.timeScale < timeScaleEffect)
                {
                    //Stop it going to far
                    if (Time.timeScale + (timeScaleEffect / 25) * (1/Time.timeScale) < timeScaleEffect)
                        Time.timeScale += (timeScaleEffect / 25) * (1/Time.timeScale);
                    else
                        Time.timeScale = timeScaleEffect;
                }
                //If timescaleeffect is slower than current timescale
                else if (Time.timeScale > timeScaleEffect)
                {
                    if (Time.timeScale - (timeScaleEffect / 25) * (1/Time.timeScale) > timeScaleEffect)
                        Time.timeScale -= (timeScaleEffect / 25) * (1/Time.timeScale);
                    else
                        Time.timeScale = timeScaleEffect;

                    //Time.timeScale -= (timeScaleEffect / 10);
                }
            }


        }
    }



}
