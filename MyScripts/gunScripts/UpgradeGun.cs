using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeGun : MonoBehaviour
{
    public int Cost =1;
    public string buyMessage = "";
    private bool lookingAt;
    public float interactDist = 4f;
    public LayerMask rayCastIgnore;

    [System.Serializable]
    public struct UpgradeableGuns
    {
        public GameObject gun;
        public string gunID;
    }

    public UpgradeableGuns[] guns;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        CheckSight();
        if (lookingAt && Time.timeScale > 0 && Input.GetButtonDown("Interact") && Cost <= PlayerMoney.Money)
        {
            if (CheckUpgradeID())
            {

                GiveGun(guns[GetIndex()].gun);
                PlayerMoney.Money -= Cost;
                


            }
        }
        if (lookingAt)
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

    private bool CheckUpgradeID()
    {
        for (int i = 0; i < guns.Length; i++)
            if (guns[i].gunID.Equals(WeaponSelection.instance.GetID() + "+"))
                return true;

        return false;
    }

    private int GetIndex()
    {
        for (int i = 0; i < guns.Length; i++)
            if (guns[i].gunID.Equals(WeaponSelection.instance.GetID() + "+"))
                return i;

        return -1;
    }

    private void GiveGun(GameObject gun)
    {
        GameObject g = Instantiate(gun);
        WeaponSelection.instance.AddGun(g, true);
    }

}
