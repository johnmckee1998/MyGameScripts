using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObectiveBase : MonoBehaviour
{
    public bool destroyOnComplete = true;
    public float timeToDestroy = 0.1f;
    [HideInInspector]
    public bool isComplete = false;

    public GameObject[] objectsToDestroy;

    public void ObjectiveComplete()
    {
        ObjectiveManager.instance.NextObjective();
        isComplete = true;

        foreach (GameObject g in objectsToDestroy)
            Destroy(g);

        if (destroyOnComplete)
            Destroy(gameObject, timeToDestroy);
    }
}
