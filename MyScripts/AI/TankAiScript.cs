using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class TankAiScript : MonoBehaviour
{
    public float moveSpeed =1f;
    public float turretRotationSpeed = 10f;
    public Transform tankBody;
    public Transform turretObj;
    public Transform turretGunObj;
    public BotShooting turretGunScript;
    public float shootDist = 100f;
    [Space]
    public LayerMask rayCastIgnore;
    [Space]
    public Transform destination;

    private NavMeshAgent navAgent;
    private bool isVisible;

    private bool turretCanSee; //triggered by a fov collider, makes sure gun on shoots when pointing towards player


    [Header("UI Shit")]
    public Image healthBar;

    /* to be used
     * 
     * public audiosource movingSound (tank tred noises)
     * public audiosource turretRotSound;
     * 
     * 
     * could also have multi stage damage - have smoke or fire come out of it when below 30%, possibly even reduced turret rotation or movement speed
     * 
     */
    // Start is called before the first frame update
    private UniversalStats unistats;
    private float startHelth;

    private bool destSet;
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        unistats = GetComponent<UniversalStats>();
        startHelth = unistats.health;
    }

    private void Update()
    {
        if (!destSet && destination!=null)
        {
            navAgent.SetDestination(destination.position);
            destSet = true;
        }

        if (healthBar != null)
            healthBar.fillAmount = (unistats.health / startHelth);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rotate tank
        RaycastHit hit;
        Physics.Raycast(tankBody.position+tankBody.up, Vector3.down, out hit, 5f, ~rayCastIgnore);
        Vector3 forward = tankBody.forward;
        tankBody.up = hit.normal;
        tankBody.forward = forward;
        //tankBody.localPosition = transform.InverseTransformPoint(hit.point);
        //tankBody.rotation = Quaternion.FromToRotation(tankBody.up, hit.normal) * transform.rotation;


        if (turretObj != null)
        {
            isVisible = CheckSight();
            //Debug.Log("Visible: " + isVisible);
            if (isVisible)
            {
                //rotate turret
                //Quaternion lookrot = Quaternion.LookRotation(CharacterControllerScript.instance.pCam.transform.position - transform.position);
                //turretObj.rotation = Quaternion.RotateTowards(turretObj.rotation, new Quaternion(turretObj.rotation.x, lookrot.y, turretObj.rotation.z, turretObj.rotation.w), turretRotationSpeed*Time.fixedDeltaTime);
                //turretGunObj.rotation = Quaternion.RotateTowards(turretGunObj.rotation, new Quaternion(lookrot.x, turretGunObj.rotation.y, turretGunObj.rotation.z, turretGunObj.rotation.w), turretRotationSpeed*Time.fixedDeltaTime);

                //turretObj.LookAt(CharacterControllerScript.instance.pCam.transform.position - Vector3.up * 0.33f);
                Quaternion lookRot = Quaternion.LookRotation((CharacterControllerScript.instance.pCam.transform.position - transform.position) + (CharacterControllerScript.characterController.velocity / 5));

                turretObj.rotation = Quaternion.Lerp(turretObj.rotation, lookRot, Time.fixedDeltaTime * turretRotationSpeed);

                turretObj.localEulerAngles = new Vector3(0, turretObj.localEulerAngles.y, 0);


                turretGunObj.LookAt(CharacterControllerScript.instance.pCam.transform.position - Vector3.up * 0.33f);
                //lookRot.x *= -1f;
                //turretGunObj.rotation = Quaternion.Lerp(turretGunObj.rotation, lookRot, Time.fixedDeltaTime * turretRotationSpeed*2f);

                turretGunObj.localEulerAngles = new Vector3(turretGunObj.localEulerAngles.x, 0, 0);

            }

            turretGunScript.shoot = (isVisible && turretCanSee);
        }
        //Debug.Log("Vis: " + isVisible + " CanSee: " + turretCanSee + " Both: " + (isVisible && turretCanSee));
    }

    private bool CheckSight()
    {
        if (CharacterControllerScript.instance != null)
        {
            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < shootDist)
            {
                RaycastHit hit;

                // if raycast hits, it checks if it hit this
                if (Physics.Raycast(turretObj.position, (CharacterControllerScript.instance.pCam.transform.position - Vector3.up*0.25f)-turretObj.position  , out hit, shootDist, ~rayCastIgnore))
                {
                    //Ray r = new Ray(turretObj.position, CharacterControllerScript.instance.pCam.transform.position - turretObj.position);
                    //Debug.DrawRay(turretObj.position, (CharacterControllerScript.instance.pCam.transform.position - Vector3.up * 0.25f) - turretObj.position, Color.green, 10f);
                    if (hit.collider.tag.Equals("Player"))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;

            }
            else return false;
        }
        else return false;
    }

    public void TurretSight(bool b)
    {
        turretCanSee = b;
    }

    public void SendDeathMessage()
    {
        if (TankSpecialWave.instance != null)
            TankSpecialWave.instance.TankDied();
    }

}
