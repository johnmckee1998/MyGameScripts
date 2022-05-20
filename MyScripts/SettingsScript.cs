using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsScript : MonoBehaviour
{
    public AudioMixer master;
    public Slider masterSlider;
    public Slider sfxSlider;
    //public Slider footstepSlider;
    //public Slider ambientSlider;
    public Slider fovSlider;
    public TextMeshProUGUI fovDisplay;
    public Toggle fpsToggle;
    public Toggle retroToggle;
    [Tooltip("Resoltuion Dropwdown")]
    public TMP_Dropdown resDropDown;
    public Toggle timerToggle;

    [System.Serializable]
    public struct VolumeSlider //generic setup for volume sliders to shorten code and allow for as many sliders as I want
    {                           //could do a similar setup for toggles (timer, fps, etc)
        public string volumeID;
        public Slider slider;
    }

    public VolumeSlider[] volSliders;

    private float startVol;
    private float startSFXVol;
    private float startFootVol;
    //private bool fpstog;

    Resolution[] res;

    int curResIndex = 0;

    bool updatingVolSliders; //used to prevent premature changes (see set volume) 
    void Start()
    {
        if (master != null)
        {
            updatingVolSliders = true;
            for(int i=0; i<volSliders.Length; i++)
            {
                master.SetFloat(volSliders[i].volumeID, PlayerPrefs.GetFloat(volSliders[i].volumeID, 0));
                master.GetFloat(volSliders[i].volumeID, out float tempVol);
                float slideVal = Mathf.Pow(10, tempVol / 20f); //calculate what slider should be based on log formula -> slide=10^(vol/20) -> the volume formula is log10(slide)*20=volume
                volSliders[i].slider.value = slideVal;
            }
            updatingVolSliders = false;
            /*
            master.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVol", 0));
            master.GetFloat("MasterVolume", out startVol);
            if (masterSlider != null)
            {
                float slideVal = Mathf.Pow(10, startVol / 20f); //calculate what slider should be based on log formula -> slide=10^(vol/20) -> the volume formula is log10(slide)*20=volume
                masterSlider.value = slideVal;
            }

            master.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVol", 0));
            master.GetFloat("SFXVolume", out startSFXVol);
            if (sfxSlider != null)
            {
                float slideVal = Mathf.Pow(10, startSFXVol / 20f); //calculate what slider should be based on log formula -> slide=10^(vol/20) -> the volume formula is log10(slide)*20=volume
                sfxSlider.value = slideVal;
            }
            /*
            master.SetFloat("FootStepsVol", PlayerPrefs.GetFloat("FootVol", 0));
            master.GetFloat("FootStepsVol", out startFootVol);
            if (footstepSlider != null)
            {
                float slideVal = Mathf.Pow(10, startSFXVol / 20f); //calculate what slider should be based on log formula -> slide=10^(vol/20) -> the volume formula is log10(slide)*20=volume
                footstepSlider.value = slideVal;
            }*/
        }
        
        if (fovSlider != null)
            fovSlider.value = PlayerPrefs.GetInt("FieldOfView", 70);

        if (fovDisplay != null)
            fovDisplay.text = fovSlider.value.ToString();

        if (fpsToggle != null)
        {
            if (PlayerPrefs.GetInt("FPSCounter", 0) == 1)
                fpsToggle.isOn = true;
            else
                fpsToggle.isOn = false;
        }

        if (retroToggle != null)
        {
            if (PlayerPrefs.GetInt("RetroEnabled", 0) == 1)
                retroToggle.isOn = true;
            else
                retroToggle.isOn = false;
        }

        if (timerToggle != null)
        {
            if (PlayerPrefs.GetInt("TimerActive", 1) == 1)
                timerToggle.isOn = true;
            else
                timerToggle.isOn = false;
        }


        //Detection and adding possible resolutions 
        res = Screen.resolutions;

        if (resDropDown != null)
        {
            resDropDown.ClearOptions();

            List<string> resOptions = new List<string>();

            for (int i = 0; i < res.Length; i++)
            {
                string option = res[i].width + "x" + res[i].height + " " + res[i].refreshRate + "Hz";

                resOptions.Add(option);

                if (res[i].width == Screen.currentResolution.width && res[i].height == Screen.currentResolution.height)
                    curResIndex = i;
            }

            resDropDown.AddOptions(resOptions);
            resDropDown.value = curResIndex;
            resDropDown.RefreshShownValue();
        }

        //StartCoroutine(UpdateSliders());
    }



    IEnumerator UpdateSliders() //This is run as a coroutine as then it will run even when timescale=0
    {
        while (true)
        {
            /* //do i really need to constantly update sliders?
            if (master != null)
            {
                master.GetFloat("MasterVolume", out startVol);
                float slideVal = Mathf.Pow(10, startVol / 20f); //calculate what slider should be based on log formula -> slide=10^(vol/20) -> the volume formula is log10(slide)*20=volume
                masterSlider.value = slideVal;


                master.GetFloat("SFXVolume", out startSFXVol);
                slideVal = Mathf.Pow(10, startSFXVol / 20f);
                sfxSlider.value = slideVal;

                master.GetFloat("FootStepsVol", out startFootVol);
                slideVal = Mathf.Pow(10, startFootVol / 20f);
                footstepSlider.value = slideVal;
            }

            if (fovSlider != null)
                fovSlider.value = PlayerPrefs.GetInt("FieldOfView", 70);
                */
            if (fovDisplay != null)
                fovDisplay.text = fovSlider.value.ToString();

            if (Time.timeScale > 0)//not paused
                yield return new WaitForSecondsRealtime(1f);
            else
                yield return new WaitForSecondsRealtime(0.02f);
        }
    }


    /*
    public void SetVolume(float volume)
    {
        float realVol = Mathf.Log10(volume) * 20f;
        master.SetFloat("MasterVolume", realVol);
        PlayerPrefs.SetFloat("MasterVol", realVol);
    }

    public void SetSFXVolume(float volume)
    {
        float realVol = Mathf.Log10(volume) * 20f;
        master.SetFloat("SFXVolume", realVol);
        PlayerPrefs.SetFloat("SFXVol", realVol);
    }*/

    //public void SetFootstepVolume(float volume)
    //{
    //    float realVol = Mathf.Log10(volume) * 20f;
    //    master.SetFloat("FootStepsVol", realVol);
    //    PlayerPrefs.SetFloat("FootVol", realVol);
    //}

    public void SetQuality(int qual)
    {
        QualitySettings.SetQualityLevel(qual);
    }

    public void SetFullscreen (bool isFull)
    {
        Screen.fullScreen = isFull;
       
    }

    public void SetFPS (bool fps)
    {
        if(fps)
            PlayerPrefs.SetInt("FPSCounter", 1);
        else
            PlayerPrefs.SetInt("FPSCounter", 0);
    }


    public void SetRetro(bool fps)
    {
        if (fps)
            PlayerPrefs.SetInt("RetroEnabled", 1);
        else
            PlayerPrefs.SetInt("RetroEnabled", 0);
    }
    

    public void ResetSettings()
    {
        //PlayerPrefs.DeleteKey("MasterVol");
        //PlayerPrefs.DeleteKey("SFXVol");

        master.SetFloat("MasterVolume", 0);
        master.SetFloat("SFXVolume", 0);

        /*
        for (int i = 0; i < volSliders.Length; i++)
        {
            master.SetFloat(volSliders[i].volumeID, 0);
            master.GetFloat(volSliders[i].volumeID, out float tempVol);
            if (masterSlider != null)
            {
                float slideVal = Mathf.Pow(10, tempVol / 20f); //calculate what slider should be based on log formula -> slide=10^(vol/20) -> the volume formula is log10(slide)*20=volume
                volSliders[i].slider.value = slideVal;
            }
        }*/
    }

    public void SetRes(int resIndex)
    {
        if (res != null)
        {
            Resolution newRes = res[resIndex];

            Screen.SetResolution(newRes.width, newRes.height, Screen.fullScreen);
        }
    }

    public void SetTimerActive(bool act)
    {
        if (act)
            PlayerPrefs.SetInt("TimerActive", 1);
        else
            PlayerPrefs.SetInt("TimerActive", 0);
    }

    public void SetFov(float i)
    {
        PlayerPrefs.SetInt("FieldOfView", (int)i);

        if (fovDisplay != null)
            fovDisplay.text = ((int)i).ToString();
    }

    public void SetVolume()  //have it run a method that updates all volumes based on current slider values (so then there is no need for params) 
    {
        for(int i=0; i < volSliders.Length; i++)
        {
            if (volSliders[i].slider.transform.parent.gameObject.activeSelf) //Only update when settings menu is active - > should prevent unintentional updates to values - 
            {                                                                //e.g. when slider values are set at start, that triggers this method which causes any sliders after the first to update with the default value 0, thus undoing any saved value
                float realVol = Mathf.Log10(volSliders[i].slider.value) * 20f;
                master.SetFloat(volSliders[i].volumeID, realVol);
                PlayerPrefs.SetFloat(volSliders[i].volumeID, realVol);
            }
        }
    }

}
