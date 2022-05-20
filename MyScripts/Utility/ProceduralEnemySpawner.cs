using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProceduralEnemySpawner : MonoBehaviour
{
    public float XMaxlimit;
    public float ZMaxlimit;
    [Tooltip("hight of the gap between levels")]
    public float YLevelSize;
    [Tooltip("Amount of levels")]
    public int YLevelCount;


    [Space]
    public int EnemiesToSpawn;

    [System.Serializable]
    public struct EnemySet
    {
        public GameObject enemy;
        public float offset;
        public float spawnChance;
    }

    public EnemySet[] enemies;

    //public GameObject[] enemies;
    //public float[] offsets;

    private int loopCatch = 0;//used to prevent infinite loops

    public void PlaceEnemies()
    {
        for(int i =0; i<EnemiesToSpawn; i++)
        {
            int rand = Random.Range(0, enemies.Length);
            ChooseRandom(rand);
            float randX = Random.Range(transform.position.x, transform.position.x+XMaxlimit);
            float randZ = Random.Range(transform.position.z, transform.position.z+ZMaxlimit);
            float randY; //to prevent nneding to do a large sampleposition, randy is calculated differently -> starting at 0, is is randomly indexed by the level height so it should align with a level
            if (YLevelSize <= 0) //only 1 level
                randY = 0;
            else
                randY = Random.Range(0, YLevelCount) * YLevelSize; //YLevelCount+1 is used coz rang(int,int) is exclusive on the latter <- doesnt actually matter coz if it starts from 0, you add level-1*height so it works fine as is
            Vector3 point = new Vector3(randX, randY + 1f, randZ);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, 3f, NavMesh.AllAreas))//if a point is found, spawn enemy
            {
                //instatiate at nearest valid point, randomised y rotation
                Instantiate(enemies[rand].enemy, new Vector3(hit.position.x, hit.position.y + enemies[rand].offset, hit.position.z), Quaternion.Euler(Vector3.up*(Random.Range(0f,360f))), transform);
            }
            else
            {
                //Debug.Log("No Point Found");
                i--;
                loopCatch++;
            }

            if(loopCatch >= EnemiesToSpawn * 3f) //allow up to 3 tries per spawn
            {
                Debug.Log("Loop Caught!");
                break;
            }

        }
    }

    public void EnemySpawnAmount(string e)
    {
        if (int.TryParse(e, out EnemiesToSpawn))
            Debug.Log("Success Enemy: " + EnemiesToSpawn);
        else
            Debug.Log("Fail Enemy");
    }

    public void SetSize(float bSize, float x, float z)
    {
        XMaxlimit = bSize * x;
        ZMaxlimit = bSize * z;
    }

    private int ChooseRandom(int i, int count = 0)
    {
       // Debug.Log("Recursion Count: " + count);
        count++;
        if (enemies[i].spawnChance >= 1 || count >= 10) //if 1 or more, guarentee spawn -> or if this has been attempted 10 times already
            return i;

        if (Random.Range(0f, 1f) <= enemies[i].spawnChance) //check chance
            return i;
        return ChooseRandom(Random.Range(0, enemies.Length), count); // choose another number
    }
}
