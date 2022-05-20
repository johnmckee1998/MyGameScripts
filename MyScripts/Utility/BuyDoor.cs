using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyDoor : MonoBehaviour
{
    public GameObject player;
    public int Cost;

    private bool lookingAt;

    private bool bought = false;
    private bool inRange;

    public GameObject[] spawnersToActivate;
    public GameObject[] spawnersToDeactivate;

    private float disToPlayer;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnersToActivate.Length; i++)
            spawnersToActivate[i].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        disToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (disToPlayer < 5f)
            inRange = true;
        else
            inRange = false;

        CheckSee();

        if(lookingAt && !bought)
            CanvasScript.instance.popUp.text = "Buy Door: " + Cost.ToString();

        if (lookingAt && Time.timeScale > 0 && Input.GetKeyDown("e") && Cost <= PlayerMoney.Money && !bought)
        {
            
            PlayerMoney.Money -= Cost;

            bought = true;
            for (int i = 0; i < spawnersToActivate.Length; i++)
                spawnersToActivate[i].SetActive(true);
            foreach (GameObject g in spawnersToDeactivate)
                g.SetActive(false);


            CanvasScript.instance.popUp.text = "";
            gameObject.SetActive(false);//disable door
        }


        if (inRange && !lookingAt)
            CanvasScript.instance.popUp.text = "";
    }

    private void CheckSee()
    {
        if (inRange)
        {
            RaycastHit rayHit;

            if (Physics.Raycast(player.transform.position, player.transform.forward, out rayHit, 2f/*, ~rayCastIgnore*/)) //doesnt account for drop here
            {
                //hit
                if (rayHit.collider.gameObject == gameObject)//itsa me!
                    lookingAt = true;
                else
                    lookingAt = false;
            }
            else
            {
                lookingAt = false;
            }
        }
        else
            lookingAt = false;
    }

    /*
    private void OnMouseEnter()
    {
        if (inRange)
        {
            lookingAt = true;
        }
    }

    private void OnMouseExit()
    {
        lookingAt = false;
    }
    */
    
}
