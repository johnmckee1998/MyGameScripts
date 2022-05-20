using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherAI : MonoBehaviour
{
    public CameraMove player;
    public GameObject playerParent;
    //private CapsuleCollider bodyCol;

    public static bool switched = false;

    private bool switching;

    private AudioSource switchSound;
    // Start is called before the first frame update
    void Start()
    {
        //if (player == null)
        //player = FindObjectOfType<CharacterControllerScript>().transform;
        switched = false;

        switching = false;

        switchSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!switched)
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z + SwitcherColliderScript.currOffset);
        else
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z - SwitcherColliderScript.currOffset);
        transform.eulerAngles = player.getCamRot();
        //Debug.Log("y: " + player.transform.rotation.y);

        if (Input.GetKeyDown("f") && !SwitcherColliderScript.isBlocked && Time.timeScale > 0 && !switching)
        {
            StartCoroutine(SwitchPos());
        }
    }

    private IEnumerator SwitchPos()
    {
        if (!switching)
        {
            //Play sound 
            switching = true;
            switchSound.PlayOneShot(switchSound.clip, switchSound.volume);
            yield return new WaitForSeconds(1f);
            //switch
            Transform prevPos = playerParent.transform;
            if(playerParent.GetComponent<CharacterControllerScript>().getCrouch())
                playerParent.transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
            else
                playerParent.transform.position = new Vector3(transform.position.x, transform.position.y - 1.95f, transform.position.z);
            transform.position = prevPos.position;
            switched = !switched;
            yield return new WaitForSeconds(2f); //add a slight delay before the player can switch again
            switching = false;
        }
        yield return null;
    }
}
