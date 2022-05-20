using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public string[] objectives;
    private int curObjective = 0;
    public TextMeshProUGUI objectiveText;

    public static ObjectiveManager instance;

    //if player has reached checkpoint
    public bool reachedCheckpoint;

    public Transform currentCheckpoint;

    //tracks which objective the checkpoint is tied too - delete all previous ones so past enemies dont respawn
    public int checkpointObjectiveIndex;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //if (CheckpointManager.activeCheckpoint)
        //    CheckPointSpawn();

        //CheckpointManager.activeCheckpoint = false;


        objectiveText.text = objectives[0];
    }

    private void Update()
    {
        if (curObjective >= objectives.Length)//all done
            CanvasScript.instance.winMenu.SetActive(true);
        
    }

    public void NextObjective()
    {
        curObjective++;
        if(curObjective<objectives.Length)
            objectiveText.text = objectives[curObjective];
    }

    public void UpdateCheckpoint(Transform check)
    {
        reachedCheckpoint = true;
        checkpointObjectiveIndex = curObjective;
        currentCheckpoint = check;
    }

    public void CheckPointSpawn(Vector3 pos, Quaternion rot, int index)
    {
        CharacterControllerScript.instance.transform.position = pos;
        CharacterControllerScript.instance.transform.rotation = rot;

        ObjectiveBase[] objs = FindObjectsOfType<ObjectiveBase>();

        for(int i=0; i < objs.Length; i++)
        {
            if (objs[i].objectiveIndex < index)
                Destroy(objs[i].gameObject);
        }


        curObjective = CheckpointManager.objectiveIndex;

        //Set correct objective

        objectiveText.text = objectives[curObjective];
    }

}
