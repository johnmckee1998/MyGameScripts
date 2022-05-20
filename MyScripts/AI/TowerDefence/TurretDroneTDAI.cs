using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretDroneTDAI : MonoBehaviour
{
    public float moveSpeed = 10f;

    private float shootDist;
    [Tooltip("In degrees per second")]
    public float rotationSpeed = 100f;
    Vector3 travelDir;
    Vector3 dest;
    //private int targetIndex;
    private Vector3 acendPoint;
    private Vector3 acendDir;
    private bool acended = true;

    [Tooltip("Simple buffer to spread out drone spawns")]
    public Vector3 offset;

    public SimpleTurret turret;

    public UniversalStats uniStats;

    private Vector3 moveDir;

    private Vector3 moveDirRef;
    //public UniversalStats turretStats;

    //[Header("Audio")]
    //public AudioSource dropSound;
    // Start is called before the first frame update
    void Start()
    {
        //targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);
        //dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 15f;
        travelDir = dest - transform.position;
        travelDir.Normalize();
        
        if(uniStats==null)
            uniStats = GetComponent<UniversalStats>();

        shootDist = turret.range / 3f; //stop when 1/3rd of range away from target
    }

    private void FixedUpdate()
    {
        


    }

    // Update is called once per frame
    void Update()
    {
        if (uniStats.health>0 && Time.timeScale>0)
        {
            if (!acended) //acending to acend point
            {
                if (Vector3.Distance(transform.position, acendPoint) > 0.5f)
                    transform.Translate(acendDir * moveSpeed * Time.deltaTime);
                else
                {
                    Debug.Log("Acended");
                    acended = true;
                    travelDir = (dest - transform.position).normalized;
                }
            }
            else
            {
                if (turret.GetTarget() != null)
                    dest = turret.GetTarget().position; //use the turrets target - it is already doing sight checks and whatnot, dont need to do anything here - it also means this drone can target bots
                else
                    dest = TowerDefenceWaveManager.instance.destination.position; //fly towards base area if no target is found

                //adjust target dest to curret y postion
                dest.y = transform.position.y; 



                travelDir = dest - transform.position;
                travelDir.Normalize();
                

                if (Vector3.Distance(transform.position, dest) > shootDist) //normal movement
                {
                    moveDir = Vector3.SmoothDamp(moveDir,  (travelDir * moveSpeed * Time.deltaTime), ref moveDirRef, 1f);
                }
                else//slow down to zero
                {
                    moveDir = Vector3.SmoothDamp(moveDir, Vector3.zero, ref moveDirRef, 1f);
                    
                }
                transform.position += moveDir;
            }

            UpdateRotation();

        }
    }

    public void FlyToDest(Transform point)
    {
        acended = false;
        acendPoint = point.position + new Vector3(Random.Range(-offset.x, offset.x), Random.Range(-offset.y, offset.y), Random.Range(-offset.z, offset.z));
        acendDir = acendPoint - transform.position;
        acendDir.Normalize();
        //dest = TDPlayerBase.instance.buildings[targetIndex].building.position + Vector3.up * 5f;
        //travelDir = dest - point.position;
        //travelDir.Normalize();
    }

    private void OnDrawGizmosSelected()
    {
        if (dest != null)
        {
            Gizmos.DrawSphere(dest, 1f);
            Gizmos.DrawLine(transform.position, transform.position + travelDir * Vector3.Distance(transform.position, dest));
            Vector3 testDir = dest - transform.position;
            Gizmos.DrawLine(transform.position, transform.position + testDir);
        }
        
    }
    
    /*
    private bool CheckSight()
    {
        Vector3 pDir = CharacterControllerScript.instance.pCam.transform.position - transform.position;
        RaycastHit rHit;
        if (Physics.Raycast(transform.position, pDir.normalized, out rHit, 50f, ~raycastIgnore))
        {
            if (rHit.transform.tag.Equals("Player"))
                return true;
            else
                return false;
        }


        return false;
    }*/

    public void UpdateRotation()
    {
        Vector3 targetDir;
        if (dest != null)
            targetDir = (dest - transform.position); //should this be pitchtransform?
        else
            targetDir = transform.forward;

        targetDir.Normalize();
        Vector3 newDirectionPitch = Vector3.RotateTowards(transform.forward, targetDir, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirectionPitch);
        transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f); //lock pitch and roll (add roll later)
    }
}
