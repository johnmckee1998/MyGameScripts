using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyObjective : ObjectiveBase
{
    private Guard[] enemies;
    private int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        enemies = GetComponentsInChildren<Guard>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isComplete)
        {
            count = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].getDead())
                    count++;

            }
            if (count == enemies.Length)
                ObjectiveComplete();
        }
    }
}
