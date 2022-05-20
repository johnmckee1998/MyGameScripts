using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class CanvasScript : MonoBehaviour
{
    public static CanvasScript instance;
    public bool isMp;
    public string currentLevel;
    public GameObject pauseMenu;
    public GameObject FailMenu;
    public GameObject winMenu;
    public TextMeshProUGUI Timer;
    public GameObject timerObj;
    private float timerCount = 0;
    private int timeMin;
    private float timeSec;
    private GameObject player;
    public AudioMixer master;
    //public Text fpsCount;

    public GameObject pReticle;
    public static GameObject reticle;
    public static Image reticleImage;
    public static bool hasReticle;

    public TextMeshProUGUI popUp;

    public bool startTimer = true;
    public TextMeshProUGUI healthText;
    public Image healthBar;
    public TextMeshProUGUI grenadeCountText;
    public TextMeshProUGUI placementText;


    [Header("Message Stuff")]
    public GameObject messageObj;
    public TextMeshProUGUI messageText;
    private bool messageDisplayed;

    private float timeScaleBackup = 1;
    //used to tell canvas if current level uses checkpoints
    //public bool checkpointsActive = false;
    //public bool enableFps = true;

    private CursorLockMode lockModeBackup;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        if (CheckpointManager.activeCheckpoint)
        {
            StartCoroutine(CheckpointSpawn());
            //CheckpointManager.currentCheckpoint = ObjectiveManager.instance.currentCheckpoint;
            //CheckpointManager.activeCheckpoint = true;
            //CheckpointManager.objectiveIndex = ObjectiveManager.instance.checkpointObjectiveIndex;
            
        }
        pauseMenu.SetActive(false);
        FailMenu.SetActive(false);
        winMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 1f;

        player = GameObject.FindWithTag("Player");

        //Enable the appropriate script -wtf is this doing
        if (player != null)
        {
            if (player.GetComponentInChildren<ShootingScript>() != null)
                player.GetComponentInChildren<ShootingScript>().enabled = true;
            else if (player.GetComponentInChildren<SixShooterScript>() != null)
                player.GetComponentInChildren<SixShooterScript>().enabled = true;
        }


        //master.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVol", 0));

        timerObj.SetActive(PlayerPrefs.GetInt("TimerActive", 1) == 1);
        //fpsCount.gameObject.SetActive(enableFps);
        /*if (PlayerPrefs.GetInt("FPSCounter", 1) == 1)
            fpsCount.gameObject.SetActive(true);
        else
            fpsCount.gameObject.SetActive(false);
        */
        AudioListener.pause = false;

        master.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVol", 0));
        master.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVol", 0));

        reticle = pReticle;
        reticleImage = pReticle.GetComponent<Image>();
        hasReticle = (pReticle != null);


        messageObj?.SetActive(false);
        LeanTween.scale(messageObj, Vector3.zero, 0.1f);

        currentLevel = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Time.timeScale > 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }*/
        if (GlobalStats.instance != null)
            GlobalStats.instance.isPaused = pauseMenu.activeSelf;


        if (grenadeCountText != null && WeaponSelection.instance !=null)
        {
            if (WeaponSelection.instance.grenade != null && WeaponSelection.instance.grenadeCount>0)
            {
                grenadeCountText.text = "Greande: " + WeaponSelection.instance.grenade.name;
                grenadeCountText.text += ": " + WeaponSelection.instance.grenadeCount.ToString();
            }
            else
                grenadeCountText.text = " ";
        }

        if (placementText != null && WeaponSelection.instance!=null)
        {
            if (WeaponSelection.instance.placementCount > 0)
                placementText.text = "Turret: " + WeaponSelection.instance.placementObject.name;
            else
                placementText.text = "Turret: None";
        }

        if (Input.GetKeyDown("escape") && !FailMenu.activeSelf && !winMenu.activeSelf)
        {
            if (pauseMenu.activeSelf) //Unpause
            {
                //if (GlobalStats.instance != null)
                //    GlobalStats.instance.isPaused = false;
                Cursor.lockState = lockModeBackup;
                Cursor.visible = false;
                pauseMenu.SetActive(false);
                if (!isMp)
                {
                    Time.timeScale = timeScaleBackup;
                    //player.GetComponentInChildren<ShootingScript>().enabled = true;
                    AudioListener.pause = false;
                }
            }
            else if(!pauseMenu.activeSelf) //Pause
            {
                //if (GlobalStats.instance != null)
                //    GlobalStats.instance.isPaused = true;
                lockModeBackup = Cursor.lockState; //save a backup because it may be locked or unlocked depending on what player was doing - more furture proof
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                pauseMenu.SetActive(true);
                if (!isMp)
                {
                    timeScaleBackup = Time.timeScale;
                    Time.timeScale = 0f;
                    //player.GetComponentInChildren<ShootingScript>().enabled = false;
                    AudioListener.pause = true;
                }
            }
        }
        if (Time.timeScale == 0)
            timerObj.SetActive(PlayerPrefs.GetInt("TimerActive", 1) == 1);
        //fpsCount.text =  "FPS " + ((int)(1f / Time.unscaledDeltaTime)).ToString();

        if (healthText != null)
            healthText.text = ((int)CharacterControllerScript.instance.health).ToString();
    }

    private void FixedUpdate()
    {
        popUp.text = "";

        if(startTimer)
            timerCount += Time.deltaTime;
        timeMin = (int)timerCount / 60;
        timeSec = (timerCount % 60);
        //if (timeMin < 10)
            //timeMin = "0" + timeMin.ToString();
        if(timeMin==0)
            Timer.SetText("Time: " + timeSec.ToString("F2"));
        else 
            Timer.SetText("Time: " + timeMin.ToString() + ":" +timeSec.ToString("F2"));

        
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        //player.GetComponentInChildren<ShootingScript>().enabled = true;
        AudioListener.pause = false;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(currentLevel);
        
        Time.timeScale = 1.0f;
    }

    public void SetActiveGenericOn(GameObject setOn)
    {
        setOn.SetActive(true);
    }

    public void SetActiveGenericOff(GameObject setOff)
    {
        setOff.SetActive(false);
    }

    IEnumerator CheckpointSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        ObjectiveManager.instance.CheckPointSpawn(CheckpointManager.currentCheckpoint.position, CheckpointManager.currentCheckpoint.rotation, CheckpointManager.objectiveIndex);
    }

    public void StartTimer(bool b)
    {
        startTimer = b;
    }

    public void Win()
    {
        winMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        AudioListener.pause = true;
    }

    public void Lose()
    {
        FailMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        AudioListener.pause = true;
    }


    public void DisplayMessage(string message, float time) //add some functionality to queue messages, maybe with a list?
    {
        if(!messageDisplayed)
            StartCoroutine(MessageRoutine(message, time));
    }

    IEnumerator MessageRoutine(string message, float time)
    {
        messageDisplayed = true;
        messageObj.SetActive(true);
        messageText.text = message;
        LeanTween.scale(messageObj, Vector3.one , 0.1f);
        yield return new WaitForSeconds(time);
        LeanTween.scale(messageObj, Vector3.zero, 0.1f);
        yield return new WaitForSeconds(0.1f);
        messageText.text = "";
        messageObj.SetActive(false);
        messageDisplayed = false;
    }

}
