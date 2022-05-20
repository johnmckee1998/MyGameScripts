using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAutoTurret : MonoBehaviour
{
    [Tooltip("In degrees per second")]
    public float rotationSpeed = 100f;
    public float range;
    public bool overrideRot = true;
    public BotShooting turretGunScript;
    public Transform turretGunTransform;
    private bool turretCanSee;
    public bool clampRotation;
    public Vector2 xMinMax;
    public Vector2 yMinMax;
    public Transform pitchObject;
    private float pitch;
    private float yaw;
    // Start is called before the first frame update

    public Transform target;

    private DumbAi targetStats;
    public bool useFov;
    public Collider fov;
    public bool useAngle;
    [Tooltip("Stored as (Xmin, Xmax, Ymin, Ymax)")]
    public Vector4 xYMinMax;
    public Transform angleComparisonTransform;
    public enum TargetType {DumAI, Guard, SmartAI, Tank, TD, TDAir }; //probably put an armour limit as well - e.g. prevent mgs from shooting tanks
                                                                //Maybe have multiple types for TD - ground, air, 
    public TargetType typeOfEnemy;

    public enum TargetOrder {First, Last, Close, Far, Strong };

    public TargetOrder targettingOrder;
    [Tooltip("Constantly Update target - even if current target is still within range and alive. This causes more cpu usage but means the turret will always use the best target for its given order.")]
    public bool constantUpdate;

    public bool aimAssist;

    [Tooltip("If true, turret rotation will not reset when there is no valid target, it will simply stay at whatever rotation it is at")]
    public bool dontReset;
    // Update is called once per frame
    private TankAiScript[] tanks;

    private UniversalStats targetUniStats;

    public LayerMask raycastIgnore;

    public UnityEvent deathEvent;
    private void Start()
    {
        /*
        if(typeOfEnemy == TargetType.DumAI)
            InvokeRepeating("UpdateDumbTarget", 0.1f, 1f);
        else if(typeOfEnemy == TargetType.Tank)
            InvokeRepeating("UpdateTankTarget", 0.1f, 1f);
        else if(typeOfEnemy == TargetType.TD)
            InvokeRepeating("UpdateTDTarget", 0.1f, 1f);
        */

        if (pitchObject != null)
            pitch = pitchObject.localEulerAngles.x;
        else
            pitch = turretGunTransform.localEulerAngles.x;
        yaw = turretGunTransform.localEulerAngles.y;

        turretGunScript.useAimAssist = aimAssist;
    }

    void FixedUpdate()
    {
        ValidTargetCheck();
        UpdateTarget();

        if(overrideRot)
            UpdateRotationAndShoot();
        else
        {
            turretGunScript.shoot = (target != null);
            if (aimAssist && target != null)
                turretGunScript.aimAssistTarget = target.position;
        }
    }

    private void OnDestroy()
    {
        if (deathEvent != null)
            deathEvent.Invoke();
    }

    //update rotation, clamping if set to. Also enables shooting it applicable
    private void UpdateRotationAndShoot()
    {
        //Vector3 destRot;
        //if (target != null)
        //    destRot = Quaternion.LookRotation((target.position - turretGunTransform.position).normalized).eulerAngles;
        //else
        //    destRot = Quaternion.LookRotation(transform.forward).eulerAngles;

        Vector3 targetDir;
        if (target != null)
            targetDir = (target.position - turretGunTransform.position);
        else
        {
            if (dontReset) //dont rotate if no target and dontReset is true
            {
                turretGunScript.shoot = false;
                return;
            }
            else
                targetDir = transform.forward;
        }




        if (pitchObject != null)
        {
            Vector3 targetDirpitch;
            if (target != null)
                targetDirpitch = (target.position - pitchObject.position); //should this be pitchtransform?
            else
                targetDirpitch = transform.forward;
            //targetDirpitch.x = 0;//lock yaw 

            //targetDirpitch = transform.InverseTransformDirection(targetDirpitch);
            //targetDirpitch.x = 0; //remove x in local space causing yaw to be locked
            //targetDirpitch = transform.TransformDirection(targetDirpitch);
            targetDirpitch.Normalize();
            Vector3 newDirectionPitch = Vector3.RotateTowards(pitchObject.forward, targetDirpitch, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0.0f);
            pitchObject.rotation = Quaternion.LookRotation(newDirectionPitch);
            pitchObject.localEulerAngles = new Vector3(pitchObject.localEulerAngles.x, 0f, 0f);
            targetDir.y = 0;//lock pitch for the yaw object - does seem to slow rotation, maybe messing with magnitude
            
        }
        if(target!=null) //shouldn't normalise if using forward
            targetDir.Normalize();
        Vector3 newDirection = Vector3.RotateTowards(turretGunTransform.forward, targetDir, rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0.0f);

        turretGunTransform.rotation = Quaternion.LookRotation(newDirection);

        float angle = Vector3.Angle(turretGunTransform.forward, targetDir);

        angle = Mathf.Abs(angle);//dont think this is needed but whatever
        //Debug.Log(angle + " Angle ");
        turretGunScript.shoot = (target != null) && angle < 10f;
        if (aimAssist && target!=null)
            turretGunScript.aimAssistTarget = target.position;
        //**
        //turretGunTransform.LookAt(target.position);
        //destRot = Quaternion.Inverse(turretGunTransform.rotation) * destRot;
        //**

        //turretGunTransform.eulerAngles = destRot; //assign world space rotation

        //**
        //pitch = Mathf.MoveTowards(pitch , turretGunTransform.localEulerAngles.x, 100f*Time.fixedDeltaTime); //use localspace rotation
        //yaw = Mathf.MoveTowards(yaw, turretGunTransform.localEulerAngles.y, 100f * Time.fixedDeltaTime); 
        //**

        //float tempPitch = turretGunTransform.localEulerAngles.x;
        //float tempYaw = turretGunTransform.localEulerAngles.y;
        //if (tempPitch > 120) //shouldnt realistically pass 90 degree rotation unless going negative
        //   tempPitch -= 360f;
        //if (tempYaw > 120) //shouldnt realistically pass 120 degree rotation unless going negative
        //    tempYaw -= 360f;

        //if (clampRotation)
        //{
        //    tempPitch = Mathf.Clamp(tempPitch, xMinMax.x, xMinMax.y);
        //    tempYaw = Mathf.Clamp(tempYaw, yMinMax.x, yMinMax.y);
        //}

        //pitch = Mathf.MoveTowards(pitch, tempPitch, rotationSpeed * Time.fixedDeltaTime); //use localspace rotation
        //yaw = Mathf.MoveTowards(yaw, tempYaw, rotationSpeed * Time.fixedDeltaTime);

        //**
        //not needed? seems to work, previously reverted pitch and yaw back to non negative, but it seems to handle negatives fine
        //if (tempPitch < 0)
        //    tempPitch += 360;
        //if (tempYaw < 0)
        //   tempYaw += 360;
        //**


        //if (pitchObject == null)
        //    turretGunTransform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        //else
        //{
        //    turretGunTransform.localEulerAngles = new Vector3(0f, yaw, 0f);
        //   pitchObject.localEulerAngles = new Vector3(pitch, 0f, 0f);
        //}

        //**
        //turretGunScript.shoot = (target != null);
        //**

        //turretGunScript.shoot = (Mathf.Abs(yaw - tempYaw) < 10f) && target != null;
    }

    public void TurretSight(bool b)
    {
        turretCanSee = b;
    }

    private void UpdateTarget()
    {
        if (typeOfEnemy == TargetType.DumAI)
            UpdateDumbTarget();
        else if (typeOfEnemy == TargetType.Tank)
            UpdateTankTarget();
        else if (typeOfEnemy == TargetType.TD || typeOfEnemy == TargetType.TDAir)
            UpdateTDTarget();
    }

    private void UpdateDumbTarget() //should also check that the target is visible with a raycast
    {
        //Debug.Log("Updating Target...");
        if (WaveManagerScript.instance != null)
        {
            if (WaveManagerScript.instance.GetAlive() > 0) //so long as enemies are alive
            {
                if (target == null || Vector3.Distance(transform.position, target.position) > range || !targetStats.alive) //if no target, target out of range, or target dead
                {
                    target = null; //remove current target
                    for (int i = 0; i < WaveManagerScript.enemies.Length; i++) //find new target
                    {
                        if (WaveManagerScript.enemies[i] != null)
                        {
                            if (Vector3.Distance(transform.position, WaveManagerScript.enemies[i].transform.position) < range && WaveManagerScript.enemies[i].alive)
                            {
                                target = WaveManagerScript.enemies[i].transform; //target the first in the list that is in range
                                targetStats = WaveManagerScript.enemies[i];
                                break;
                            }
                        }
                    }
                }
            }
            else
                target = null;
        }
        else
            target = null;
    }

    private void UpdateGuardTarget()
    {

    }

    private void UpdateSmartAITarget()
    {

    }

    private void UpdateTankTarget()
    {
        if (TankSpecialWave.instance.GetAlive() > 0)
        {
            if (target == null || Vector3.Distance(transform.position, target.position) > range || targetUniStats.health<=0) //if no target, target out of range, or target dead
            {
                tanks = FindObjectsOfType<TankAiScript>();

                //Debug.Log("TankScripts: " + tanks.Length);

                target = null; //remove current target
                for (int i = 0; i < tanks.Length; i++) //find new target
                {
                    if (tanks[i] != null)
                    {
                        if (Vector3.Distance(transform.position, tanks[i].transform.position) < range && tanks[i].GetComponent<UniversalStats>().health>0)
                        {
                            target = tanks[i].transform; //target the first in the list that is in range
                            targetUniStats = tanks[i].GetComponent<UniversalStats>();
                            break;
                        }
                    }
                }
            }
        }
        //Debug.Log(TankSpecialWave.instance.GetAlive() + " Tanks");
    }

    private void ValidTargetCheck()
    {
        if (target != null) //various checks to see if target is still valid
        {
            if (useFov) //checking if target is still in fov
                if (!fov.bounds.Contains(target.position))
                {
                    target = null;
                    return;
                }
            if (useAngle && !CheckAngle(target.position))
            {
                //Debug.Log("Invalid Angle Failed");
                target = null;
                return;
            }
            if (!CheckSight(target))
            {
                //Debug.Log("Invalid Sight Failed");
                //CheckSight(target, true);
                target = null;
                return;
            }
        }
    }

    private void UpdateTDTarget()
    {
        if (TowerDefenceWaveManager.instance.WaveStatus())
        {

            Transform targetParent;
            if (typeOfEnemy == TargetType.TD)
                targetParent = TowerDefenceWaveManager.instance.transform;
            else if (typeOfEnemy == TargetType.TDAir) //air targetting
                targetParent = TowerDefenceWaveManager.instance.airEnemyParent;
            else
                targetParent = TowerDefenceWaveManager.instance.transform; //placeholder, should never be used, other td types will go here

            if ((constantUpdate || target == null || targetUniStats.health <= 0 || Vector3.Distance(transform.position, target.position) > range) && targetParent.childCount > 0)//if no target, or target it dead, or target out of range, and there must be alive enemies
            {
                float closestDist = float.MaxValue;
                float farthestDistance = -range * 2f - 1f;
                float highestHealth = 0f;
                bool foundTarget = false;
                int targetIndex = 0;
                for (int i = 0; i < targetParent.childCount; i++)
                {

                    if ((!useFov || fov.bounds.Contains(targetParent.GetChild(i).position))&&CheckAngle(targetParent.GetChild(i).position) && CheckSight(targetParent.GetChild(i))) //only consider if it is visible
                    {
                        //Debug.Log("InBounds");

                        float curIDist = Vector3.Distance(transform.position, targetParent.GetChild(i).position);

                        if (curIDist < range) //only consider it if within range
                        {
                            //Debug.Log("inRange");
                            if (targettingOrder == TargetOrder.Close)
                            {
                                if (closestDist > curIDist)
                                {
                                    foundTarget = true;
                                    targetIndex = i;
                                    closestDist = curIDist;
                                }
                            }
                            else if (targettingOrder == TargetOrder.First || targettingOrder == TargetOrder.Last)
                            {
                                float distToDest = Vector3.Distance(TowerDefenceWaveManager.instance.destination.position, targetParent.GetChild(i).position); //distance to destination
                                if (targettingOrder == TargetOrder.First)
                                {
                                    if (closestDist > distToDest)
                                    {
                                        foundTarget = true;
                                        targetIndex = i;
                                        closestDist = distToDest;
                                    }
                                }
                                else //last tergetting
                                {
                                    if (farthestDistance < distToDest)
                                    {
                                        foundTarget = true;
                                        targetIndex = i;
                                        farthestDistance = distToDest;
                                    }
                                }
                            }
                            else if (targettingOrder == TargetOrder.Far)
                            {
                                if (farthestDistance < curIDist)
                                {
                                    foundTarget = true;
                                    targetIndex = i;
                                    farthestDistance = curIDist;
                                }
                            }
                            else //Must be strong targetting if reaches this point
                            {
                                UniversalStats uni = targetParent.GetChild(i).GetComponent<UniversalStats>();
                                if (uni.health > highestHealth)
                                {
                                    foundTarget = true;
                                    targetIndex = i;
                                    highestHealth = uni.health;
                                }
                            }
                        }

                    }
                }
                if (!foundTarget) //no point is found within range, so dont target anyone
                {
                    target = null;
                }
                else
                {
                    target = targetParent.GetChild(targetIndex);
                    targetUniStats = target.GetComponent<UniversalStats>();
                }

            }
        }
        else
            turretGunScript.InstantReload();
    }

    private void OnDrawGizmosSelected()
    {
        if (target != null && angleComparisonTransform!=null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(angleComparisonTransform.position, target.position);
            Gizmos.DrawSphere(target.position, 1f);
        }
        if (useAngle && angleComparisonTransform!=null)
        {
            Gizmos.color = Color.red;
            Vector3 xMinDir = Quaternion.AngleAxis(xYMinMax.x, transform.right) * transform.forward;
            Vector3 xMaxDir = Quaternion.AngleAxis(xYMinMax.y, transform.right) * transform.forward;
            Vector3 yMinDir = Quaternion.AngleAxis(xYMinMax.z, transform.up) * transform.forward;
            Vector3 yMaxDir = Quaternion.AngleAxis(xYMinMax.w, transform.up) * transform.forward;

            if ( false && pitchObject != null)
            {
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + xMinDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + xMaxDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + yMinDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + yMaxDir * range);
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + pitchObject.forward * range);
                //connecting lines
                /*
                Gizmos.DrawLine(pitchObject.position, pitchObject.position + pitchObject.forward * range);
                Gizmos.DrawLine(pitchObject.position + xMinDir * range, pitchObject.position + pitchObject.forward * range);
                Gizmos.DrawLine(pitchObject.position + xMaxDir * range, pitchObject.position + pitchObject.forward * range);
                Gizmos.DrawLine(pitchObject.position + yMinDir * range, pitchObject.position + pitchObject.forward * range);
                Gizmos.DrawLine(pitchObject.position + yMaxDir * range, pitchObject.position + pitchObject.forward * range);
                Gizmos.DrawLine(pitchObject.position + yMaxDir * range, pitchObject.position + xMinDir * range);
                Gizmos.DrawLine(pitchObject.position + yMaxDir * range, pitchObject.position + xMaxDir * range);
                Gizmos.DrawLine(pitchObject.position + yMinDir * range, pitchObject.position + xMinDir * range);
                Gizmos.DrawLine(pitchObject.position + yMinDir * range, pitchObject.position + xMaxDir * range);
                */
            }
            else
            {
                Gizmos.DrawLine(angleComparisonTransform.position, angleComparisonTransform.position + xMinDir * 2f);
                Gizmos.DrawLine(angleComparisonTransform.position, angleComparisonTransform.position + xMaxDir * 2f);
                Gizmos.DrawLine(angleComparisonTransform.position, angleComparisonTransform.position + yMinDir * 2f);
                Gizmos.DrawLine(angleComparisonTransform.position, angleComparisonTransform.position + yMaxDir * 2f);
                Gizmos.DrawLine(angleComparisonTransform.position, angleComparisonTransform.position + angleComparisonTransform.forward * range);
            }
            Gizmos.DrawLine(transform.position, transform.position+transform.forward*2f);

        }
    }

    private bool CheckAngle(Vector3 tarPos)
    {
        if (!useAngle)
            return true;

        Vector3 DirToTargetPitch;
        Vector3 DirToTargetYaw;
        if(pitchObject!=null)
            DirToTargetPitch = tarPos - pitchObject.position;
        else
            DirToTargetPitch = tarPos - turretGunTransform.position;
        DirToTargetYaw = tarPos - turretGunTransform.position;
        DirToTargetPitch.Normalize();
        DirToTargetYaw.Normalize();

        //Now convert to localspace because working in worldspace can be weird when dealing with rotation - if the parent transform is rotated +- 90 in worldspace, the calculated angle is weird
        //It seems like there is something werid with the driection when working in worldspace - it only gives correct angle when worldspace rotation is 0 or 180. Maybe it is something to do with the forward vector i was using? not sure.
        //There is probably a way to get it working without converting to localspace, but this works well and doesnt add too much complexity, so its fine for now. I suspect the issue is either how i calculate direction or the forward direction i use.

        //I THINK I KNOW WHY LOCAL WORKS - orientation. If turret is looking along z axis, you want to lock x to ignore yaw, but if it is looking along x axis you want to lock z. by working in localspace, you can simply always lock x since local z is always forward and local x is always right/left
        DirToTargetPitch = transform.InverseTransformDirection(DirToTargetPitch); 
        DirToTargetYaw = transform.InverseTransformDirection(DirToTargetYaw);
        float angleToTargetY = Vector3.SignedAngle(new Vector3(DirToTargetYaw.x, 0f, DirToTargetYaw.z), Vector3.forward, Vector3.up); //removing y component means no elevation(pitch) movement is considered, giving only the yaw angle
        float angleToTargetX = Vector3.SignedAngle(new Vector3(0f, DirToTargetPitch.y, DirToTargetPitch.z), Vector3.forward, Vector3.up); //removing the x component means no yaw rotation is considered, giving only the pitch angle 
        if (angleToTargetY > xYMinMax.w || angleToTargetY < xYMinMax.z) //outside bounds of y
            return false;
        if (angleToTargetX > xYMinMax.y || angleToTargetX < xYMinMax.x) //outside bounds of x
            return false;
        return true;//if reaches here, then must be in bounds
    }

    private bool CheckSight(Transform tar, bool dbg = false) //dbg is debug
    {
        RaycastHit rHit;

        Vector3 targettingPos;
        UniversalStats tUni = tar.GetComponent<UniversalStats>();
        if (tUni!=null && tUni.targetPos != null)
            targettingPos = tUni.targetPos.position;
        else
            targettingPos = tar.position;

        if (Physics.Raycast(angleComparisonTransform.position, (targettingPos - angleComparisonTransform.position).normalized, out rHit, range, ~raycastIgnore))
        {
            //stealth enemy check
            TDStealthGunEnemy st = rHit.transform.GetComponentInParent<TDStealthGunEnemy>();
            if (st != null)
                if (!st.isVisible)
                    return false; //cant see when  invisible

            //Debug.Log("Name: " + rHit.transform.name + " Tar: " + tar.name);
            if (rHit.transform.IsChildOf(tar))
                return true;
            //HitboxScript hb = rHit.transform.GetComponent<HitboxScript>();
            //if (hb!=null && hb.unistats.transform.Equals(tar)) //if it isnt the direct child but has the correct unistats connected
            //    return true;
            if (dbg)
                Debug.Log("Failed: " + rHit.transform.name);
            return false;
        }
        else
            return false;
    }

    public void RefillAmmo()
    {
        turretGunScript.RefillAmmoPool();
    }

}
