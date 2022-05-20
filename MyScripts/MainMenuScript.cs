using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public GameObject LoadScreen;
    public Image LoadSlider;
    public TextMeshProUGUI loadText;
    

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (LoadScreen != null)
        {
            LoadScreen.SetActive(false);
            LoadSlider.gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuitGameMenu()
    {
        Application.Quit();
    }

    public void LoadLevelMenu(string level)
    {
        //reset checkpoint manager
        CheckpointManager.activeCheckpoint = false;
        CheckpointManager.currentCheckpoint = null;
        CheckpointManager.objectiveIndex = 0;

        StartCoroutine(LoadAsyncLevel(level));
    }

    IEnumerator LoadAsyncLevel(string lvl)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(lvl);
        LoadScreen.SetActive(true);
        LoadSlider.gameObject.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); //changes the progress scale from 0-0.9 to 0-1
            //Debug.Log(progress);
            if(LoadSlider!=null)
                LoadSlider.fillAmount = progress;
            if(loadText!=null)
                loadText.text = ((int)(progress * 100f)).ToString();

            yield return null;
        }

        if (operation.isDone)
            Time.timeScale = 1f;
    }


    

}
