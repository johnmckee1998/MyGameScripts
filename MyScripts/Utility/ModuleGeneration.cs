using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleGeneration : MonoBehaviour
{
    public float totalWidth = 50f;
    public float subModuleSize = 25f;

    private int cellCount; //how many smaller modules make up this one -> grid size (is always square so x and z are the same)

    private Vector3 currentSpawnPos; //spawn in localspace
    private float zeroPos; //since x and z are the same dont need a vector
    /* zero at 0,0 local space
     * first module at (-1/2*total witdth) + 1/2*submodule size. index by submodule size untill reaching (+1/2*total witdth) + 1/2*submodule size
     * 
     * 
     * 
     * 
     */
    public GameObject[] subModules;

    private int[] repeatCount;//trackRepitition
    private Vector3 moduleRotation;

    public bool randomiseRotation = true;
    // Start is called before the first frame update
    void Start()
    {
        if(totalWidth % subModuleSize != 0)
        {
            Debug.Log("WRONG MODULE SIZE: " + gameObject.name);
            return;
        }
        else
        {
            cellCount = (int)(totalWidth / subModuleSize);
        }

        repeatCount = new int[subModules.Length];

        zeroPos = (-0.5f * totalWidth) + 0.5f * subModuleSize;
        currentSpawnPos = new Vector3(zeroPos, 0f, zeroPos);
        PlaceBuildings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void PlaceBuildings()
    {
        for(int z=1; z<=cellCount; z++)
        {
            for(int x=1; x<=cellCount; x++)
            {
                if (randomiseRotation)
                    moduleRotation = transform.eulerAngles + (Random.Range(0, 4) * (Vector3.up * 90));
                else
                    moduleRotation = transform.eulerAngles;
                Vector3 worldspaceSpawn = transform.TransformPoint(currentSpawnPos);
                MinimalRepeatSpawn(worldspaceSpawn);
                //Instantiate(subModules[Random.Range(0, subModules.Length)], worldspaceSpawn, Quaternion.identity);

                currentSpawnPos.x +=subModuleSize; //index x
            }
            currentSpawnPos.x = zeroPos; //reset x
            currentSpawnPos.z += subModuleSize; //index z
        }
    }

    private void MinimalRepeatSpawn(Vector3 place)
    {
        int rand = Random.Range(0, subModules.Length); //int, int is exclusive on the last 
        if (repeatCount[rand] == 0)
        {
            Instantiate(subModules[rand], place, Quaternion.Euler(moduleRotation));
            //amountPlaced++;
            repeatCount[rand]++;
        }
        else //if all have spawned, place the one with lowest value - if all are equal, rand is used because nothing lower is found - see islowestvalue
        {
            int lowest = IsLowestValue(repeatCount, rand);
            Instantiate(subModules[lowest], place, Quaternion.Euler(moduleRotation));
            //amountPlaced++;
            repeatCount[lowest]++;
        }

        //else
        //  MinimalRepeatSpawn(place);


    }

    private int IsLowestValue(int[] array, int index) //returns lowest value - if all or multiple are equal to index, retuns index, if multiple that arnt index are equal, returns first instance of lowest value
    {
        int lowest = index;
        int equalCount = 0;//tracks how many values are equal
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < array[lowest])
                lowest = i;
            else if (array[i] == array[lowest])
                equalCount++;
        }
        if (equalCount == array.Length)
            return index; //if all values are equal, then return true and spawn this - probably isnt neccasary since i start lowest at rand anyway
        else
            return lowest;
    }
}
