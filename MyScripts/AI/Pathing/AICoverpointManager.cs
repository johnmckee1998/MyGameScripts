using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICoverpointManager : MonoBehaviour
{
    public static AICoverpointManager instance;
    public bool drawGizmos;
    [Space]
    public Material previewMat;
    public Material validMat;

    [System.Serializable]
    private struct CoverPoint
    {
        public Transform point;
        public bool inUse;
        //other things i could add - cover type, cover hight, 
    }
    private CoverPoint[] coverPoints;
    private MeshRenderer[] pointMeshRens;


    [Header("TurretStuff")]
    private MultiUserGunScript[] turrets = new MultiUserGunScript[0];
    private bool[] turretsInUse = new bool[0];
    //public Transform crewmanGatherPoint;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        UpdateCoverPoints();
    }

    private void UpdateCoverPoints()
    {
        coverPoints = new CoverPoint[transform.childCount];
        pointMeshRens = new MeshRenderer[transform.childCount];
        for(int i=0; i<coverPoints.Length; i++)
        {
            coverPoints[i].point = transform.GetChild(i);
            coverPoints[i].inUse = false;
            pointMeshRens[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
            if(pointMeshRens[i]!=null)
                pointMeshRens[i].enabled = false;
        }
    }
    
    public Transform GetNextPoint() //gets first point that isnt in use
    {
        for (int i = 0; i < coverPoints.Length; i++)
            if (!coverPoints[i].inUse)
            {
                coverPoints[i].inUse = true;
                return coverPoints[i].point;
            }

        return null;//no point is found return null
    }

    public Transform GetNearestPoint(Vector3 pos, bool use = true, bool show = false, float range = 2) //gets nearest point to pos that is not in use 
    {                                                                                                  //-> use determines whether or not to mark point as in use, show and range determines whether mesh renderer should be shown 
        float closestDist = float.MaxValue;
        int index = coverPoints.Length*2;
        for (int i = 0; i < coverPoints.Length; i++) //Find Closest Point within range, and enable mesh renderers for points in range (disabling those not in range
            if (!coverPoints[i].inUse)
            {
                float dist = Vector3.Distance(coverPoints[i].point.position, pos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    index = i;
                }
                if (show && dist <= range * 3f && pointMeshRens[i] != null)
                {
                    pointMeshRens[i].enabled = true;
                    pointMeshRens[i].material = previewMat;
                }
                else if (show && pointMeshRens[i] != null)
                    pointMeshRens[i].enabled = true; //Was False, currently testing what it is like when all points are shown ignoring range (to make it more like the turret placement preview)

            }
            else if (pointMeshRens[i] != null)
                pointMeshRens[i].enabled = false; //Disable mesh renderer for points that are already in use

        if (index < coverPoints.Length && (!show || closestDist<=range)) //if index is valid and in range (range only relevant for repositioning, so is ignored if show is false)
        {
            if(use)
                coverPoints[index].inUse = true;
            if(show && pointMeshRens[index]!=null)
                pointMeshRens[index].material = validMat;
            return coverPoints[index].point;
        }
        
        return null;//no valid point was found
    }

    public void FreePoint(Transform t) //sets given point to no longer be in use - frees it up for use
    {
        for (int i = 0; i < coverPoints.Length; i++)
            if (coverPoints[i].point == t)
            {
                coverPoints[i].inUse = false;
                return;
            }
    }

    public void FreePoint(int i) //frees up point at index i
    {
        coverPoints[i].inUse = false;
    }

    public void UsePoint(Transform t)
    {
        for (int i = 0; i < coverPoints.Length; i++)
            if (coverPoints[i].point == t)
            {
                coverPoints[i].inUse = true;
                return;
            }
    }

    public void UsePoint(int i)
    {
        coverPoints[i].inUse = true;
    }

    private void OnDrawGizmosSelected()
    {
        if(drawGizmos && transform.childCount > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < transform.childCount; i++)
                Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.5f);
        }
    }

    public bool ArePointsAvaliable()
    {
        //bool b = false;
        for (int i = 0; i < coverPoints.Length; i++)
            if (!coverPoints[i].inUse)
            {
                return true; // a valid point is found
                //break;
            }
        return false;
    }

    public void DisableMeshRens()
    {
        for (int i = 0; i < pointMeshRens.Length; i++)
            pointMeshRens[i].enabled = false;
        
    }

    public int FreePointsAvaliable()
    {
        int count = 0;
        for (int i = 0; i < coverPoints.Length; i++)
            if (!coverPoints[i].inUse)
                count++;
        return count;
    }

    public void AddTurret(MultiUserGunScript g)
    {
        MultiUserGunScript[] backup = turrets;
        turrets = new MultiUserGunScript[turrets.Length + 1];//increase array size
        bool[] tInuseBckp = turretsInUse;
        turretsInUse = new bool[turrets.Length];
        for (int i = 0; i < backup.Length; i++)
        {
            turrets[i] = backup[i]; // re-add backed up values from original array
            turretsInUse[i] = tInuseBckp[i];
        }

        turrets[turrets.Length - 1] = g; //add new value to the end
        turretsInUse[turrets.Length - 1] = false;
    }

    public void RemoveTurret(MultiUserGunScript g)
    {
        MultiUserGunScript[] backup = turrets;
        turrets = new MultiUserGunScript[turrets.Length - 1];//decrease array size
        bool[] tInuseBckp = turretsInUse;
        turretsInUse = new bool[turrets.Length];
        for (int i = 0, j = 0; i < backup.Length; i++)
        {
            if (backup[i] != g) //if current turret isnt the one being removed 
            {
                turrets[j] = backup[i]; // re-add backed up values from original array
                turretsInUse[j] = tInuseBckp[j];
                j++; //increase turret index
            }
        }


    }

    public MultiUserGunScript GetNearestTurret(Vector3 pos, float range = 5f)
    {
        float closestDist = float.MaxValue;
        int closestIndex = -1;

        for(int i=0; i < turrets.Length; i++)
        {
            if (!turretsInUse[i]) //so long as not already in use
            {
                float tempDist = Vector3.Distance(pos, turrets[i].transform.position);
                if (tempDist < closestDist)
                {
                    closestDist = tempDist;
                    closestIndex = i;
                }
            }
            
            turrets[i].placementPreview.SetActive(false); //disable previews - closest one will be show after
        }

        if (closestDist <= range)
        {
            turrets[closestIndex].placementPreview.SetActive(true); //show preview to denote it as being  a valid placement
            return turrets[closestIndex];
        }

        return null;

    }

    public void DisableTurretPreviews()
    {
        for (int i = 0; i < turrets.Length; i++)
            turrets[i].placementPreview.SetActive(false);
    }

    public void UseTurret(MultiUserGunScript g)
    {
        for (int i = 0; i < turrets.Length; i++)
            if (turrets[i] == g)
                turretsInUse[i] = true;
    }

    public void LeaveTurret(MultiUserGunScript g)
    {
        for (int i = 0; i < turrets.Length; i++)
            if (turrets[i] == g)
                turretsInUse[i] = false;
    }

}
