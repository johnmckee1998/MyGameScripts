using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveBase : MonoBehaviour
{
    //which objective is this? 0 is the first
    public int objectiveIndex;
    public bool destroyOnComplete = true;
    public float timeToDestroy = 0.1f;
    [HideInInspector]
    public bool isComplete = false;

    public GameObject[] objectsToDestroy;
    

    public bool isCheckpoint = false;
    public Transform checkpointSpawn;


    public void ObjectiveComplete()
    {
        if (isCheckpoint)
        {
            ObjectiveManager.instance.UpdateCheckpoint(checkpointSpawn);
            CheckpointManager.activeCheckpoint = true;
        }


        ObjectiveManager.instance.NextObjective();
        isComplete = true;

        foreach (GameObject g in objectsToDestroy)
            Destroy(g);

        if (destroyOnComplete)
            Destroy(gameObject, timeToDestroy);
    }
}
