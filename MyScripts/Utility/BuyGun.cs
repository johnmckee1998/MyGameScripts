using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyGun : MonoBehaviour
{
    public GameObject gun;
    public string gunID; 
    public int Cost;
    public int ammoCost = 100;
    public WeaponSelection weapSelect;
    public Material lookatmat;
    private Material startmat;
    private Renderer ren;

    private bool lookingAt;
    public float interactDist = 4f;
    public LayerMask rayCastIgnore;
    private bool bought = false;
    //private bool inRange;
    public string buyTxt;
    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<Renderer>();
        startmat = ren.material;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (weapSelect == null)
            weapSelect = WeaponSelection.instance;

        CheckSight();

        if (lookatmat != null)
        {
            if(lookingAt)
                ren.material = lookatmat;
            else
                ren.material = startmat;
        }


        if (lookingAt && Time.timeScale > 0)
        {
               //buy gun

                if (!weapSelect.CheckID(gunID)) //player does not have gun - buy it
                {
                    CanvasScript.instance.popUp.text = buyTxt + Cost.ToString();
                    if(Input.GetKeyDown("e") && Cost <= PlayerMoney.Money){
                        GiveGun();
                        PlayerMoney.Money -= Cost;
                    }
                }
                else //player has gun - buy ammo
                {
                    CanvasScript.instance.popUp.text = "Buy Ammo: " + ammoCost.ToString();
                    if (Input.GetKeyDown("e") && Cost <= PlayerMoney.Money)
                    {
                        weapSelect.RefillAmmo(gunID);
                        PlayerMoney.Money -= ammoCost;
                    }
                }  
        }
        //if (Vector3.Distance(weapSelect.transform.position, transform.position) < 5f)
        //    inRange = true;
        //else
        //    inRange = false;
    }

    /*
    private void OnMouseEnter()
    {
        if(inRange)
        {
            lookingAt = true;
            ren.material = lookatmat;
        }
    }

    private void OnMouseExit()
    {
        lookingAt = false;
        ren.material = startmat;
    }
    */
    private void GiveGun()
    {
        GameObject g = Instantiate(gun);
        weapSelect.AddGun(g);
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
