using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Covertester : MonoBehaviour
{
    public static bool repo; //used by all cover testers to see if someone else is repositioning, so that you cant try and reposistion multiple at once
    private NavMeshAgent nav;
    public bool nearest;
    private Transform dest;
    public int refundAmount = 1;
    public LayerMask raycastIgnore;
    private bool repositioning;

    private Transform destBackup;
    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        if (nearest)
            dest = AICoverpointManager.instance.GetNearestPoint(transform.position);
        else
            dest = AICoverpointManager.instance.GetNextPoint();
        nav.SetDestination(dest.position);

        destBackup = dest;
    }

    private void FixedUpdate()
    {
        if (nav.remainingDistance < 0.1f && !repositioning)
        {
            transform.rotation = dest.rotation;
            //this.enabled = false;
        }
        if (repositioning)
        {
            RaycastHit rHit;
            if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out rHit, 4, ~raycastIgnore))
            {
                dest = AICoverpointManager.instance.GetNearestPoint(rHit.point, false, true, 2f);
            }
            else
                dest = AICoverpointManager.instance.GetNearestPoint(transform.position, false, true, 2f);
        }
    }

    private void Update()
    {
        if (repositioning)
        {
            WeaponSelection.instance.SetPlacing(true); //need to do this as the unlockmovement method that the menu uses when closing sets placement to false - dont want to touch it
            if (Time.timeScale > 0 && CharacterControllerScript.Active)
            {
                CanvasScript.instance.popUp.text = "Reposistioning Bot - RightClick/i to Cancel";
                if (Input.GetButtonDown("Fire1"))
                {
                    if (dest != null)
                    {
                        repositioning = false;
                        Invoke("DisablePlace", 0.25f); //Disable placement now that it has been placed -> delayed by 0.25f to prevent accidental shooting

                        nav.SetDestination(dest.position);
                        AICoverpointManager.instance.DisableMeshRens();
                        repo = false;

                        AICoverpointManager.instance.FreePoint(destBackup); //mark old point as free
                        destBackup = dest;

                        AICoverpointManager.instance.UsePoint(dest); //mark new point as in use
                    }
                    //else
                    //{
                    //    dest = destBackup;
                    //}
                }
                else if (Input.GetKeyDown("i") || Input.GetButtonDown("Fire2")) //key that stops placement - basically means change pos is cancelled, is also cancelled by right click 
                {
                    repositioning = false;
                    Invoke("DisablePlace", 0.25f); //Disable placement now that it has been cancelled -> delayed by 0.25f to prevent accidental shooting
                    dest = destBackup;
                    nav.SetDestination(dest.position);

                    AICoverpointManager.instance.DisableMeshRens();
                    repo = false;
                }
            }
        }
    }

    public void OnDestroy()
    {
        AICoverpointManager.instance.FreePoint(dest);
        if (repositioning)
        {
            repositioning = false;
            WeaponSelection.instance.DisablePlacing(); //delay this by a bit
            AICoverpointManager.instance.DisableMeshRens();
            repo = false;
        }
    }

    public void Refund()
    {
        PlayerMoney.Money += refundAmount;
        Destroy(gameObject);
    }

    public void ChangePosition()
    {
        if (!repo) //only allow repositioning when: no other covertester is already reposistioning
        {
            repo = true;
            repositioning = true;
            WeaponSelection.instance.SetPlacing(true);
        }
        else
        {
            Debug.Log("Cant repo: " + repo);
        }
    }

    private void DisablePlace()//used to disable placement mode -> put it its own function so i can use invoke 
    {
        WeaponSelection.instance.DisablePlacing();
    }
}
