using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDGroupSpawn : MonoBehaviour
{
    public TowerDefenceWaveManager.EnemyAimType groupType;
    // Start is called before the first frame update
    void Start()
    {
        SeparateGroup();

        Destroy(gameObject, 0.25f);
    }

    
    private void SeparateGroup()
    {
        Transform[] group = new Transform[transform.childCount]; //make an array of the children - this way i reference the array and dont have issues due to message with the heirarchy while indexing through child objects
        for (int i = 0; i < group.Length; i++)
            group[i] = transform.GetChild(i);

        for (int i = 0; i < group.Length; i++)
            if(groupType == TowerDefenceWaveManager.EnemyAimType.Normal) //normal parent
                group[i].parent = TowerDefenceWaveManager.instance.transform;
            else //assum air parent
                group[i].parent = TowerDefenceWaveManager.instance.airEnemyParent;
    }

}
