using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Beantlefield.Multi.ConnectionApproval;
using UnityEngine.SceneManagement;

public class MPCanvasScript : MonoBehaviour
{
    public bool loadSceneOnLeave;
    public string sceneToLoad;
    public void LeaveLobby()
    {
        PasswordNetworkManager p = FindObjectOfType<PasswordNetworkManager>();
        if(p!=null)
        {
            p.Leave();
            if (loadSceneOnLeave)
                try
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
                catch
                {
                    Debug.LogWarning("Invalid sceneToLoad");
                }
        }
    }
}
