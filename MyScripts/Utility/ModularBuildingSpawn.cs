using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularBuildingSpawn : MonoBehaviour
{
    public static ModularBuildingSpawn instance;

    public float buildingSize;
    public int GridX;
    public int GridZ;

    public GameObject[] buildings;
    public GameObject[] boundary;
    [Tooltip("If this script is not being trigger by something else - it is independent")]
    public bool independent = false;
    public bool minimiseRepeat;

    private int[] repeatCount;


    [Header("Special Conditions")]
    public bool useWinModule;
    public GameObject winModule;
    public bool randomRotation;

    [Space]
    public GameObject spawnModule;
    public int spawnIndexX;
    public int spawnIndexZ;
    public enum SpawnModulePos {staticSpawn, randomSpawn, centerSpawn };
    public SpawnModulePos SpawnSetup;
    public Transform player;

    private Vector3 zeroPosition;
    private Vector3 moduleRotation;
    //private int amountPlaced;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        repeatCount = new int[buildings.Length];
        if(buildings!=null && buildings.Length>0 && independent)
            PlaceBuildings();

        zeroPosition = transform.position + new Vector3(buildingSize / 2f, 0f, buildingSize / 2f);

        if (!randomRotation)
            moduleRotation = transform.eulerAngles;
    }

    

    public void PlaceBuildings()
    {
        int maxIndex = buildings.Length;
        Vector3 place = zeroPosition;
        if (GridX <= 0) //Edge case stuff - if someone is dumb enough to put 0 or negative number, make it 1
            GridX = 1;
        if (GridZ <= 0)
            GridZ = 1;

        if (GridX == 1 && GridZ == 1 && useWinModule) //if some retard put both as 1 when there is a win module, increase z by 1 - this is because enemies cant spawn in win module, so the enemy spawner will get stuck in a loop
            GridZ = 2; //technically only needed if the replace last module method is used, if instead the replace boundary with win module is used then this check isnt needed


        if (SpawnSetup == SpawnModulePos.randomSpawn)
        {
            spawnIndexX = Random.Range(1, GridX+1);
            spawnIndexZ = Random.Range(1, GridZ+1);
        }
        else if (SpawnSetup == SpawnModulePos.centerSpawn)
        {
            spawnIndexX = (GridX+1) / 2;
            spawnIndexZ = (GridX+1) / 2;
        }

        for(int i =1; i<=GridX; i++)
        {
            for(int j=1; j<= GridZ+1; j++) //does +1 so that the final bondary at the end of the row is placed
            {
                if(randomRotation)
                    moduleRotation = transform.eulerAngles + (Random.Range(0,4)* (Vector3.up*90));

                if (i == 1)//if its the first row, put boundaries to the left
                {
                    Instantiate(boundary[Random.Range(0, boundary.Length)], (place - new Vector3(buildingSize, 0f, 0f)), Quaternion.Euler(moduleRotation));
                }
                if (i==GridX)//if its the last row, put boundaries to the right - needs to be if and not else if because if it is 1 row then it puts it on both sides
                {
                    Instantiate(boundary[Random.Range(0, boundary.Length)], (place + new Vector3(buildingSize, 0f, 0f)), Quaternion.Euler(moduleRotation));
                }

                if (j==1 && i!=1) //at the first of each row, place one boundary behind EXCEPT for the very first box - thats where you spawn so dont do it there
                {
                    Instantiate(boundary[Random.Range(0, boundary.Length)], (place - new Vector3(0f, 0f, buildingSize)), Quaternion.Euler(moduleRotation));
                }
                if (j <= GridZ)//normal row placement
                {
                    if ((spawnModule != null && player!=null) && (i == spawnIndexX && j == spawnIndexZ))
                    {
                        Instantiate(spawnModule, place, transform.rotation);
                        player.position = place + Vector3.up;
                    }

                    else if (useWinModule && GridZ == j && GridX == i) //this way the win module is placed at the last index of the grid
                        Instantiate(winModule, place, transform.rotation);

                    else if (!minimiseRepeat)
                    {
                        int rand = Random.Range(0, maxIndex); //int, int is exclusive on the last 
                        Instantiate(buildings[rand], place, Quaternion.Euler(moduleRotation));
                        //amountPlaced++;
                    }
                    else
                        MinimalRepeatSpawn(place);
                    place += new Vector3(0f, 0f, buildingSize);
                }
                //else if (useWinModule && GridZ + 1 == j && GridX == i) //this way the win module is placed above the final module - replaces the final bondary and the end of final row
                //    Instantiate(winModule, place, transform.rotation);
                else if (j == GridZ + 1) //place a boundary and the end of the row
                    Instantiate(boundary[Random.Range(0, boundary.Length)], place, Quaternion.Euler(moduleRotation));
            }
            place = new Vector3(zeroPosition.x + buildingSize*i, zeroPosition.y, zeroPosition.z);
        }
    }

    public void SetX(string x)
    {
        if (int.TryParse(x, out GridX))
            Debug.Log("Success X: " + GridX);
        else
            Debug.Log("Fail X");
    }

    public void SetZ(string z)
    {
        if (int.TryParse(z, out GridZ))
            Debug.Log("Success Z: " + GridZ);
        else
            Debug.Log("Fail Z");
    }

    public void SetMinimalSpawn(bool b)
    {
        minimiseRepeat = b;
        Debug.Log("MinimalSpawn: " + b);
    }

    private void MinimalRepeatSpawn(Vector3 place)
    {
        int rand = Random.Range(0, buildings.Length); //int, int is exclusive on the last 
        if (repeatCount[rand] == 0)
        {
            Instantiate(buildings[rand], place, Quaternion.Euler(moduleRotation));
            //amountPlaced++;
            repeatCount[rand]++;
        }
        else //if all have spawned, place the one with lowest value - if all are equal, rand is used because nothing lower is found - see islowestvalue
        {
            int lowest = IsLowestValue(repeatCount, rand);
            Instantiate(buildings[lowest], place, Quaternion.Euler(moduleRotation));
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
        for(int i=0; i<array.Length; i++)
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
