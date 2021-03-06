using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathManager : MonoBehaviour
{
    public static AIPathManager instance;
    private AIPath[] allPaths = new AIPath[0];
    private AIPath[] guardPaths;
    private AIPath[] sDronePaths; //small drone
    private AIPath[] lDronePaths; //large drone

    private void Start()
    {
        instance = this;
    }

    public void UpdateAllPaths() //refreshes list of paths and sorts them into arrays
    {
        allPaths = FindObjectsOfType<AIPath>();
        int guardCount = 0;
        int smallDroneCount = 0;
        int largeDroneCount = 0;
        for (int i = 0; i < allPaths.Length; i++) //find how many of each
        {
            if (allPaths[i].pathType == AIPath.AIPathtype.Guard)
                guardCount++;
            else if (allPaths[i].pathType == AIPath.AIPathtype.SmallDrone)
                smallDroneCount++;
            else if (allPaths[i].pathType == AIPath.AIPathtype.LargeDrone)
                largeDroneCount++;
        }

        guardPaths = new AIPath[guardCount];
        sDronePaths = new AIPath[smallDroneCount];
        lDronePaths = new AIPath[largeDroneCount];
        int gIndex = 0;
        int sIndex = 0;
        int lIndex = 0;
        for (int i = 0; i < allPaths.Length; i++) //do the loop again and assign values
        {
            if (allPaths[i].pathType == AIPath.AIPathtype.Guard)
            {
                guardPaths[gIndex] = allPaths[i];
                gIndex++;
            }
            else if (allPaths[i].pathType == AIPath.AIPathtype.SmallDrone)
            {
                sDronePaths[sIndex] = allPaths[i];
                sIndex++;
            }
            else if (allPaths[i].pathType == AIPath.AIPathtype.LargeDrone)
            {
                lDronePaths[lIndex] = allPaths[i];
                lIndex++;
            }
        }


    }

    public void UpdatePaths(AIPath p) //adds given path to appropriate array
    {
        if (p.pathType == AIPath.AIPathtype.Guard)
        {
            for (int i = 0; i < guardPaths.Length; i++)
            {
                if (guardPaths[i] == p) //path is already in the list
                    return;
            }
            //if it reaches here, path is not in list
            AIPath[] backup = (AIPath[])guardPaths.Clone();
            guardPaths = new AIPath[guardPaths.Length + 1];
            for (int i = 0; i < backup.Length; i++) //fill up slots with preexsisting paths
                guardPaths[i] = backup[i];
            guardPaths[guardPaths.Length - 1] = p; //add new one to the end
        }
        else if (p.pathType == AIPath.AIPathtype.SmallDrone)
        {
            for (int i = 0; i < sDronePaths.Length; i++)
            {
                if (sDronePaths[i] == p) //path is already in the list
                    return;
            }
            //if it reaches here, path is not in list
            AIPath[] backup = (AIPath[])sDronePaths.Clone();
            sDronePaths = new AIPath[sDronePaths.Length + 1];
            for (int i = 0; i < backup.Length; i++) //fill up slots with preexsisting paths
                sDronePaths[i] = backup[i];
            sDronePaths[sDronePaths.Length - 1] = p; //add new one to the end
        }
        else if (p.pathType == AIPath.AIPathtype.LargeDrone)
        {
            for (int i = 0; i < lDronePaths.Length; i++)
            {
                if (lDronePaths[i] == p) //path is already in the list
                    return;
            }
            //if it reaches here, path is not in list
            AIPath[] backup = (AIPath[])lDronePaths.Clone();
            lDronePaths = new AIPath[lDronePaths.Length + 1];
            for (int i = 0; i < backup.Length; i++) //fill up slots with preexsisting paths
                lDronePaths[i] = backup[i];
            lDronePaths[lDronePaths.Length - 1] = p; //add new one to the end
        }

        //if reached here, then it must have added the path to a list, so now add it to allPaths

        AIPath[] allbckp = (AIPath[])allPaths.Clone();
        allPaths = new AIPath[allPaths.Length + 1];
        for (int i = 0; i < allbckp.Length; i++) //fill up slots with preexsisting paths
            allPaths[i] = allbckp[i];
        allPaths[allPaths.Length - 1] = p; //add new one to the end
    }

    public Transform[] GetRandomPath(AIPath.AIPathtype type) //gets path of type
    {
        if (type == AIPath.AIPathtype.Guard)
        {
            return (guardPaths[Random.Range(0, guardPaths.Length)].path);
        }
        if (type == AIPath.AIPathtype.SmallDrone)
        {
            return (sDronePaths[Random.Range(0, sDronePaths.Length)].path);
        }
        if (type == AIPath.AIPathtype.LargeDrone)
        {
            return (lDronePaths[Random.Range(0, lDronePaths.Length)].path);
        }
        return null;
    }

    public Transform[] GetRandomPath(AIPath.AIPathtype type, Transform pos, float maxDist = 100f) //gets path of type that is closest to pos (if greater than max dist, then return null
    {
        if (type == AIPath.AIPathtype.Guard)
        {
            int closestPoint = 0;
            float closestDistance = Vector3.Distance(guardPaths[closestPoint].origin.position, pos.position);
            for (int i = 0; i < guardPaths.Length; i++)
            {
                if (Vector3.Distance(guardPaths[i].origin.position, pos.position) < closestDistance) //if closer than curret closest
                {
                    closestPoint = i;
                    closestDistance = Vector3.Distance(guardPaths[closestPoint].origin.position, pos.position); //update closestdistance
                }
            }

            if (closestDistance > maxDist)
                return null;

            return (guardPaths[closestPoint].path);
        }
        if (type == AIPath.AIPathtype.SmallDrone)
        {
            int closestPoint = 0;
            float closestDistance = Vector3.Distance(sDronePaths[closestPoint].origin.position, pos.position);
            for (int i = 0; i < sDronePaths.Length; i++)
            {
                if (Vector3.Distance(sDronePaths[i].origin.position, pos.position) < closestDistance) //if closer than curret closest
                {
                    closestPoint = i;
                    closestDistance = Vector3.Distance(sDronePaths[closestPoint].origin.position, pos.position); //update closestdistance
                }
            }

            if (closestDistance > maxDist)
                return null;

            return (sDronePaths[closestPoint].path);
        }
        if (type == AIPath.AIPathtype.LargeDrone)
        {
            int closestPoint = 0;
            float closestDistance = Vector3.Distance(lDronePaths[closestPoint].origin.position, pos.position);
            for (int i = 0; i < lDronePaths.Length; i++)
            {
                if (Vector3.Distance(lDronePaths[i].origin.position, pos.position) < closestDistance) //if closer than curret closest
                {
                    closestPoint = i;
                    closestDistance = Vector3.Distance(lDronePaths[closestPoint].origin.position, pos.position); //update closestdistance
                }
            }

            if (closestDistance > maxDist)
                return null;

            return (lDronePaths[closestPoint].path);
        }
        return null;
    }

}
