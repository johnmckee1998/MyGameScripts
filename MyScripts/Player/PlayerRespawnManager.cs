using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager instance;
    public Transform playerSpawnPoint;
    public float respawnTime = 5f;
    public GameObject spectatorCam;
    private bool respawning;
    public bool limitedRespawns;
    public int respawnLimit = 3;

    [Header("UI Stuff")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI respawnTimerText;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        spectatorCam.SetActive(false);
        respawning = false;
    }

    private void Update()
    {
        if (respawning)
        {
            CharacterControllerScript.Active = false;
            CameraMove.instance.Active = false;
            WeaponSelection.instance.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (respawnTimerText != null && respawning)
        {
            respawnTimerText.text = "Respawn in: " + ((int)timer).ToString();
            timer -= Time.fixedDeltaTime;
        }
        else if (respawnTimerText != null)
            respawnTimerText.text = "";

        if (livesText != null && limitedRespawns)
            livesText.text = "Lives: " + respawnLimit.ToString();

    }

    public void RespawnPlayer()
    {
        if (!respawning)
            if(!limitedRespawns || respawnLimit > 0)
                StartCoroutine(Respawn());
            else
                CanvasScript.instance.Lose();
        respawnLimit--;
    }

    private IEnumerator Respawn()
    {
        respawning = true;
        CharacterControllerScript.Active = false;
        CameraMove.instance.Active = false;
        WeaponSelection.instance.gameObject.SetActive(false);
        //CharacterControllerScript.instance.gameObject.SetActive(false);
        //spectatorCam.SetActive(true);
        timer = respawnTime;
        yield return new WaitForSeconds(respawnTime);
        respawning = false;
        CharacterControllerScript.Active = true;
        CameraMove.instance.Active = true;
        WeaponSelection.instance.gameObject.SetActive(true);
        //CharacterControllerScript.instance.gameObject.SetActive(true);
        //spectatorCam.SetActive(false);
        CharacterControllerScript.instance.transform.position = playerSpawnPoint.position;
        CharacterControllerScript.instance.transform.rotation = playerSpawnPoint.rotation;
        CharacterControllerScript.instance.FillHealth();
        
    }

    public bool IsRespawning()
    {
        return respawning;
    }
}
