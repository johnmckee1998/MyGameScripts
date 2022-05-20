using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TurretCrewmanAI : MonoBehaviour
{

    public Vector2 gatherRadius;

    [Space]
    public LayerMask raycastIgnore;

    protected NavMeshAgent nav;


    protected bool usingGun;

    protected MultiUserGunScript turret;
    protected bool turretRepositioning;

    protected MultiUserGunScript tempTurret;
    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        //go to gather point
        nav.SetDestination(CrewmanManager.instance.crewmanGatherPoint.position + new Vector3(Random.Range(-gatherRadius.x, gatherRadius.x), 0f, Random.Range(-gatherRadius.y, gatherRadius.y)));

        CrewmanManager.instance.AddCrewman(this);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (turretRepositioning)
        {
            TurretSearch();
        }
    }

    private void Update()
    {
        if (turretRepositioning)
        {
            WeaponSelection.instance.SetPlacing(true); //need to do this as the unlockmovement method that the menu uses when closing sets placement to false - dont want to touch it
            if (Time.timeScale > 0 && CharacterControllerScript.Active)
            {
                CanvasScript.instance.popUp.text = "Reposistioning Crew - RightClick to Cancel";
                if (Input.GetButtonDown("Fire1") && tempTurret!=null)
                {
                    StopTurretSearch();

                    nav.SetDestination(tempTurret.botPosition.position);
                    //AICoverpointManager.instance.DisableMeshRens();


                    //AICoverpointManager.instance.FreePoint(destBackup); //mark old point as free
                    //destBackup = dest;

                    if (turret != null) //if bot is already using a turret, make sure it leaves it correctly
                        turret.BotLeaveGun();

                    tempTurret.BotUseGun(transform); //shouldnt run this function untill bot has arrived - but still should block player from using it

                    turret = tempTurret;

                    usingGun = true;
                }
                else if (Input.GetButtonDown("Fire2")) //key that stops placement - basically means change pos is cancelled, is also cancelled by right click 
                {
                    turretRepositioning = false;
                    StopTurretSearch();
                }
            }
        }
    }

    protected void TurretSearch()
    {
        
        RaycastHit rHit;
        Vector3 inFrontPos = CharacterControllerScript.instance.pCam.transform.position + (CharacterControllerScript.instance.pCam.transform.forward*5f);
        if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out rHit, 6f, ~raycastIgnore))
        {
            tempTurret = AICoverpointManager.instance.GetNearestTurret(rHit.point, 6f);
        }
        else
            tempTurret = AICoverpointManager.instance.GetNearestTurret(inFrontPos, 6f);
    }

    public void FindTurret()
    {
        turretRepositioning = true;

        WeaponSelection.instance.SetPlacing(true);
    }

    public void StopTurretSearch()
    {
        turretRepositioning = false;
        AICoverpointManager.instance.DisableTurretPreviews();

        WeaponSelection.instance.DisablePlacing();
    }

    public void LeaveTurret() //the purpose of andGun is to allow the crewman directly to leave the turret, rather than it being called by multigun
    {
        if (turret != null)
        {
            turret.BotLeaveGun(); //run by multigun itself - this function is actually called in botleavegun -> not anymore?
            usingGun = false;
            if(nav!=null) 
                nav.SetDestination(CrewmanManager.instance.crewmanGatherPoint.position);
        }
    }

    public bool IsAvailable()
    {
        return !usingGun; 
    }

    private void OnDestroy()
    {
        CrewmanManager.instance.RemoveCrewman(this);
    }

    private void OnDisable()
    {
        CrewmanManager.instance.RemoveCrewman(this);
    }

}
