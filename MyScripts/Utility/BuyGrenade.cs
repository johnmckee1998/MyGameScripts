using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyGrenade : MonoBehaviour
{
    public int Cost;
    public GameObject grenade;
    public int grenadeCount;
    public string buyMessage = "";
    private bool lookingAt;
    public bool repeatBuy = true;
    private bool bought = false;
    public float interactDist = 4f;
    public LayerMask rayCastIgnore;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!bought || repeatBuy)
            CheckSight();
        if (lookingAt && Time.timeScale > 0 && Input.GetButtonDown("Interact") && Cost <= PlayerMoney.Money && (repeatBuy || !bought))
        {

            PlayerMoney.Money -= Cost;
            bought = true;

            WeaponSelection.instance.ChangeGrenade(grenade, grenadeCount);
        }
        if (lookingAt && (!bought || repeatBuy))
            CanvasScript.instance.popUp.text = buyMessage;
    }

    private void CheckSight()
    {
        if (CharacterControllerScript.instance != null)
        {
            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < interactDist)
            {
                RaycastHit hit;

                // if raycast hits, it checks if it hit this
                if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out hit, interactDist, ~rayCastIgnore))
                {
                    if (hit.collider.gameObject.Equals(gameObject))
                    {
                        lookingAt = true;
                    }
                    else
                        lookingAt = false;
                }
                else
                    lookingAt = false;

            }
            else lookingAt = false;
        }
    }
}
