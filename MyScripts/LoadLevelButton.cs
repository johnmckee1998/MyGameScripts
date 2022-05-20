using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelButton : MonoBehaviour
{
	
	public string CurrentLevel = "Level1";
	
	public void LoadLevel(){
		SceneManager.LoadScene(CurrentLevel);
		Time.timeScale = 1f;
	}
}
