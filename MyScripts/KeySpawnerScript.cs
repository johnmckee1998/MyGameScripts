using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySpawnerScript : MonoBehaviour
{
    public GameObject key;
    public int keyNum;
    public Transform[] spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        int r;
        for(int i = 0; i<keyNum; i++)
        {
            r = Random.Range(0, spawnPoints.Length);
            if (spawnPoints[r] != null)
            {
                if(spawnPoints[r].childCount ==0)
                    Instantiate(key, spawnPoints[r]);
                else
                {
                    int r2 = Random.Range(0, spawnPoints[r].childCount);
                    Instantiate(key, spawnPoints[r].GetChild(r2));
                }
            }
            else
                i--;
            spawnPoints[r] = null;
        }
    }

}
