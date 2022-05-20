using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTurret : MonoBehaviour
{
    public BotShooting turretGunScript;
    public Transform turretGunTransform;

    private bool turretCanSee;

    public bool clampRotation;
    public Vector2 xMinMax;
    public Vector2 yMinMax;
    public Transform pitchObject;
    public float rotationSpeed = 360f;
    public enum TargetType {Player, Bots, Both };
    public TargetType targetting;
    [Header("Manual Sight")]
    [Tooltip("If true, does a manual raycast, if false then it relies on a fov collider setup")]
    public bool manualSightCheck = true;
    public Transform sightCheckPos;
    public float range = 100f;
    [Tooltip("Used for sight checks")]
    public LayerMask raycastIgnore;
    [Tooltip("Used for finding targets - will target these layers")]
    public LayerMask physicsOverlayMask;
    private Transform[] potentialTargets;
    private float pitch;
    private float yaw;

    private Transform target;
    // Start is called before the first frame update

    public UniversalStats uniStats;
    private Transform player;
    private void Start()
    {
        if(uniStats==null)
            uniStats = transform.GetComponent<UniversalStats>(); 
        if (uniStats == null)
            uniStats = transform.GetComponentInParent<UniversalStats>(); //if still null, check parent
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player == null)
            player = CharacterControllerScript.instance.transform;
        if (uniStats.health>0) //should be health based
        {
            //turretGunScript.shoot = turretCanSee;
            UpdateTarget();
            ValidTargetCheck();
            UpdateRotationAndShoot();
        }
    }

    public void TurretSight(bool b)
    {
        turretCanSee = b;
    }

    private void UpdateRotationAndShoot()
    {
        Vector3 targetDir;
        if (target != null)
            targetDir = ((target.position+Vector3.up) - turretGunTransform.position).normalized;
        else
            targetDir = transform.forward;
        Vector3 newDirection = Vector3.RotateTowards(turretGunTransform.forward, targetDir, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0.0f);

        turretGunTransform.rotation = Quaternion.LookRotation(newDirection);

        if (pitchObject != null)
        {
            Vector3 targetDirpitch;
            if (target != null)
                targetDirpitch = (target.position - turretGunTransform.position).normalized;
            else
                targetDirpitch = transform.forward;

            Vector3 newDirectionPitch = Vector3.RotateTowards(pitchObject.forward, targetDirpitch, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0.0f);

            pitchObject.rotation = Quaternion.LookRotation(newDirection);
        }
        float angle = Vector3.Angle(turretGunTransform.forward, targetDir);

        angle = Mathf.Abs(angle);//dont think this is needed but whatever

        turretGunScript.shoot = (target != null) && angle < 10f;
    }

    private void ValidTargetCheck()
    {
        if (target != null) //various checks to see if target is still valid
        {
            if (clampRotation && !CheckAngle(target.position))
            {
                //Debug.Log("Failed Angle");
                target = null;
                return;
            }
            if (manualSightCheck)
            {
                if (!CheckSight(target))
                {
                    //Debug.Log("Failed Sight");
                    target = null;
                    return;
                }
            }
            else
            {
                if (!turretCanSee)
                {
                    target = null;
                    return;
                }
            }
        }
    }

    private bool CheckAngle(Vector3 tarPos)
    {
        if (!clampRotation)
            return true;

        Vector3 DirToTargetPitch;
        Vector3 DirToTargetYaw;
        if (pitchObject != null)
            DirToTargetPitch = tarPos - pitchObject.position;
        else
            DirToTargetPitch = tarPos - turretGunTransform.position;
        DirToTargetYaw = tarPos - turretGunTransform.position;
        DirToTargetPitch.Normalize();
        DirToTargetYaw.Normalize();

        //Now convert to localspace because working in worldspace can be weird when dealing with rotation - if the parent transform is rotated +- 90 in worldspace, the calculated angle is weird
        //It seems like there is something werid with the driection when working in worldspace - it only gives correct angle when worldspace rotation is 0 or 180. Maybe it is something to do with the forward vector i was using? not sure.
        //There is probably a way to get it working without converting to localspace, but this works well and doesnt add too much complexity, so its fine for now. I suspect the issue is either how i calculate direction or the forward direction i use.
        DirToTargetPitch = transform.InverseTransformDirection(DirToTargetPitch);
        DirToTargetYaw = transform.InverseTransformDirection(DirToTargetYaw);
        float angleToTargetY = Vector3.SignedAngle(new Vector3(DirToTargetYaw.x, 0f, DirToTargetYaw.z), Vector3.forward, Vector3.up); //removing y component means no elevation(pitch) movement is considered, giving only the yaw angle
        float angleToTargetX = Vector3.SignedAngle(new Vector3(0f, DirToTargetPitch.y, DirToTargetPitch.z), Vector3.forward, Vector3.up); //removing the x component means no yaw rotation is considered, giving only the pitch angle 
        if (angleToTargetY > yMinMax.y || angleToTargetY < yMinMax.x) //outside bounds of y
            return false;
        if (angleToTargetX > xMinMax.y || angleToTargetX < xMinMax.x) //outside bounds of x
            return false;
        return true;//if reaches here, then must be in bounds
    }

    private bool CheckSight(Transform tar)
    {
        Vector3 pos;
        //have to use different pos for player coz of the weird positioning that player has where its pos is at its feet - > switched to using cam - 50cm
        if ((targetting == TargetType.Player || targetting == TargetType.Both) && tar == player) //player targetting
            pos = CharacterControllerScript.instance.pCam.transform.position - Vector3.up * 0.5f;
        else
            pos = tar.position;
        RaycastHit rHit;
        if (Physics.Raycast(sightCheckPos.position, (pos - sightCheckPos.position).normalized, out rHit, range, ~raycastIgnore))
        {
            //Debug.Log("Name: " + rHit.transform.name + " Tar: " + tar.name);
            if (rHit.transform.Equals(tar))
                return true;
            else if (rHit.transform.parent != null && rHit.transform.parent.Equals(tar)) //hit child collider
                return true;
            else
                return false;
        }
        else
            return false;
    }

    private void UpdateTarget()
    {
        if (target == null) // no target - find one
        {
            if (targetting == TargetType.Player)
                target = player;
            else if (targetting == TargetType.Bots)
                FindBestTarget(false);
            else if (targetting == TargetType.Both)
                FindBestTarget(true);
        }
        else //target is set, check if it is still valid - done by valid check anyway
        {

        }
    }

    private void FindBestTarget(bool checkPlayer)
    {
        float distToP;
        if (checkPlayer && CheckSight(player))
            distToP = Vector3.Distance(transform.position, player.position);
        else
            distToP = float.MaxValue;
        float minDist = float.MaxValue;
        int minDistIndex = -1;

        //update potential targets
        Collider[] tarCols = Physics.OverlapSphere(transform.position, range, physicsOverlayMask);
        potentialTargets = new Transform[tarCols.Length];
        for (int i = 0; i < tarCols.Length; i++)
            potentialTargets[i] = tarCols[i].transform;
        for(int i=0; i<potentialTargets.Length; i++)
        {
            float checkDist = Vector3.Distance(transform.position, potentialTargets[i].position);
            if (checkDist < minDist)
            {
                if (CheckSight(potentialTargets[i]))
                {
                    minDist = checkDist;
                    minDistIndex = i;
                }
            }
        }
        if (checkPlayer)
        {
            if (minDist < distToP && minDist <= range)
                target = potentialTargets[minDistIndex];
            else if (distToP <= range && distToP<=minDist)
                target = player;
            else if(distToP>minDist)
                target = potentialTargets[minDistIndex];
            else
                target = null;
        }
        else
        {
            if (minDistIndex < 0) //no valid found
                target = null;
            else if (minDist <= range)
                target = potentialTargets[minDistIndex];
            else
                target = null;
        }
        
    }

    private void OldUpdate() //just a backup of old update method
    {
        turretGunTransform.LookAt(CharacterControllerScript.instance.pCam.transform.position - Vector3.up * 0.25f);
        pitch = turretGunTransform.localEulerAngles.x;
        yaw = turretGunTransform.localEulerAngles.y;
        if (pitch > 90) //shouldnt realistically pass 90 degree rotation unless going negative
            pitch -= 360f;
        if (yaw > 90) //shouldnt realistically pass 90 degree rotation unless going negative
            yaw -= 360f;

        pitch = Mathf.Clamp(pitch, xMinMax.x, xMinMax.y);
        yaw = Mathf.Clamp(yaw, yMinMax.x, yMinMax.y);

        if (pitch < 0)
            pitch += 360;
        if (yaw < 0)
            yaw += 360;
        if (pitchObject == null)
            turretGunTransform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        else
        {
            turretGunTransform.localEulerAngles = new Vector3(0f, yaw, 0f);
            pitchObject.localEulerAngles = new Vector3(pitch, 0f, 0f); //problem with this is that it uses the lookat direction from the turretgun, which only works so long as the center point and forward direction is the same for both gun and pitchobj
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(turretGunTransform.position, target.position);
        }
        if (clampRotation)
        {
            Gizmos.color = Color.red;
            Vector3 xMinDir = Quaternion.AngleAxis(xMinMax.x, transform.right) * transform.forward;
            Vector3 xMaxDir = Quaternion.AngleAxis(xMinMax.y, transform.right) * transform.forward;
            Vector3 yMinDir = Quaternion.AngleAxis(yMinMax.x, transform.up) * transform.forward;
            Vector3 yMaxDir = Quaternion.AngleAxis(yMinMax.y, transform.up) * transform.forward;

            if (pitchObject != null)
            {
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + xMinDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + xMaxDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + yMinDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + yMaxDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + pitchObject.forward * range);
            }
            else
            {
                Gizmos.DrawLine(turretGunTransform.position, turretGunTransform.position + xMinDir * 2f);
                Gizmos.DrawLine(turretGunTransform.position, turretGunTransform.position + xMaxDir * 2f);
                Gizmos.DrawLine(turretGunTransform.position, turretGunTransform.position + yMinDir * 2f);
                Gizmos.DrawLine(turretGunTransform.position, turretGunTransform.position + yMaxDir * 2f);
            }
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

        }
    }

    public Transform GetTarget()
    {
        return target;
    }
}
