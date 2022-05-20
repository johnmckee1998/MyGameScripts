using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool : MonoBehaviour
{
    public static SimpleObjectPool instance;

    [System.Serializable]
    public struct Pool
    {
        public string poolID;
        public GameObject obj;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool p in pools)
        {
            Queue<GameObject> objPool = new Queue<GameObject>();

            for(int i=0; i< p.size; i++)
            {
                GameObject g = Instantiate(p.obj);
                g.SetActive(false);
                objPool.Enqueue(g);
            }

            poolDictionary.Add(p.poolID, objPool);
        }

    }

    public GameObject SpawnFromPool(string poolID, Vector3 pos, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(poolID))
        {
            Debug.LogWarning("Invalid Pool ID: " + poolID);
            return null;
        }

        GameObject objToSpawn = poolDictionary[poolID].Dequeue(); //dequeue gives the game objects, but also removes it from queue

        if (!objToSpawn.activeSelf) //if not active so therefore not in use
        {
            
            objToSpawn.transform.position = pos;
            objToSpawn.transform.rotation = rot;
            poolDictionary[poolID].Enqueue(objToSpawn);//make sure it remains in queue
            objToSpawn.SetActive(true);
            return objToSpawn;
        }
        else //if non avaliable, add a new one to pool
        {
            poolDictionary[poolID].Enqueue(objToSpawn);//start by making sure old one remains in queue
            Debug.Log("Need new obj: " + poolID);
            objToSpawn = Instantiate(GetPoolObj(poolID), pos, rot);
            if (objToSpawn != null)
            {
                //double check rot, pos, and make sure its enabled -> not neccesary?
                objToSpawn.SetActive(true);
                objToSpawn.transform.position = pos;
                objToSpawn.transform.rotation = rot;
                poolDictionary[poolID].Enqueue(objToSpawn);//add new obj to queue
                return objToSpawn;
            }
        }

        return null;
    }

    private GameObject GetPoolObj(string poolID)
    {
        foreach(Pool p in pools)
        {
            if (p.poolID.Equals(poolID))
                return p.obj;
        }
        return null; //none found
    }
}
