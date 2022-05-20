using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SensorDoors : MonoBehaviour
{
    public Material unlockedMaterial;
    public Material lockedMaterial;

    private Renderer[] doorRens;
    private Collider[] doorColliders; //potential issue - relies on each door only having 1 collider. in reality this shouldnt cause an issue as since they probably wont have rigidbodies then a concave mesh collider can be used if a more detailed collider is needed

    [Header("Trigger Collider")]
    public Collider boundary;
    [Space]
    [Tooltip("If true, then doors will auto lock when player enters bounds")]
    public bool activateWhenEnter = true;
    [Space]
    public bool removeDoors;
    // Start is called before the first frame update
    void Start()
    {
        doorRens = new Renderer[transform.childCount];
        doorColliders = new Collider[transform.childCount];
        int i = 0;
        foreach(Transform child in transform)
        {
            doorRens[i] = child.GetComponent<Renderer>();
            doorColliders[i] = child.GetComponent<Collider>();
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (removeDoors) //when set to true (by completing an objective or whatever) destroy gameobject (and thus all child doors) as it is no longer needed
            Destroy(gameObject);
        else if (CharacterControllerScript.instance != null)
        {
            if (boundary.bounds.Contains(CharacterControllerScript.instance.transform.position) && activateWhenEnter) //this method of checking when the player enters is better as i can control what collider to use, otherwise ontriggerenter would use all child colliders
                LockDoors();
        }

    }

    public void UnlockDoors() //allow passthrough
    {
        for(int i=0; i<transform.childCount; i++)
        {
            doorRens[i].material = unlockedMaterial;
            doorColliders[i].isTrigger = true; //set them to trigger so no collision
        }
    }

    public void LockDoors()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            doorRens[i].material = lockedMaterial;
            doorColliders[i].isTrigger = false;
        }

        EnableObstacle();
    }

    public void RemoveDoors()
    {
        removeDoors = true;
    }
    

    private void EnableObstacle()
    {
        foreach(Transform chld in transform)
        {
            NavMeshObstacle ob = chld.GetComponent<NavMeshObstacle>();
            if (ob != null)
                ob.enabled = true;
            
        }
    }

}
