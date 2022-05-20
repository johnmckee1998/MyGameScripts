using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShrinkingScreen : MonoBehaviour
{
    public Image Bleft;
    public Image Bright;
    public Image Btop;
    public Image Bbottom;
    [Range(0.1f, 5f)]
    [Tooltip("Rate in minutes")]
    public float closingRate = 1;
    private bool dontClose;


    private float ratePerSecond; //actual rate that will be used
    // Start is called before the first frame update
    void Start()
    {
        Bleft.fillAmount = 0;
        Bright.fillAmount = 0;
        Btop.fillAmount = 0;
        Bbottom.fillAmount = 0;

        dontClose = false;
        ratePerSecond = 0.5f / (closingRate * 60f);
        Debug.Log(ratePerSecond + " " + ratePerSecond*60);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CharacterControllerScript.instance != null && CharacterControllerScript.instance.gameObject.activeSelf)
        {

            if (Bleft.fillAmount < 0.5 && !dontClose)
            {
                Bleft.fillAmount += (ratePerSecond * Time.fixedDeltaTime);
                Bright.fillAmount += (ratePerSecond * Time.fixedDeltaTime);
                Btop.fillAmount += (ratePerSecond * Time.fixedDeltaTime);
                Bbottom.fillAmount += (ratePerSecond * Time.fixedDeltaTime);
            }
            else if (Bleft.fillAmount >= 0.5 && !dontClose)
            {
                CanvasScript.instance.FailMenu.SetActive(true);
                Time.timeScale = 0f;
            }
            else if (dontClose)
            {
                Bleft.fillAmount = 0;
                Bright.fillAmount = 0;
                Btop.fillAmount = 0;
                Bbottom.fillAmount = 0;
            }
        }

        if (CanvasScript.instance.FailMenu.activeSelf) //disable images when fail is active so they dont block menu
        {
            Bleft.gameObject.SetActive(false);
            Bright.gameObject.SetActive(false);
            Btop.gameObject.SetActive(false);
            Bbottom.gameObject.SetActive(false);
        }
    }

    public void explore()
    {
        dontClose = true;
    }

    public void SetRate(float f)
    {
        closingRate = f;
        ratePerSecond = 0.5f / (closingRate * 60f); //since the value i want to reach is 0.5, to get the rate per second i simply divide be number of seconds, and since closing rate is in minutes i times that by 60
    }
}
