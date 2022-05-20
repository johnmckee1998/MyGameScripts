using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModularLevelUI : MonoBehaviour
{
    public ModularBuildingSpawn modSpawn;
    public ProceduralEnemySpawner enemySpawn;

    [Header("Display Elements")]
    public Sprite gridPic;
    public Image enemyPic;
    public Image pickupPic;
    public Vector3 gridStart;
    public float gridPicSize;
    public float enemyPicSize;
    // Start is called before the first frame update
    void Start()
    {
        gridStart = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        Vector3 place = gridStart;
        for (int i = 1; i <= modSpawn.GridX; i++)
        {
            for (int j = 1; j <= modSpawn.GridZ + 1; j++)
            {
                
                if (j <= modSpawn.GridZ)//normal row placement
                {
                    
                    Instantiate(gridPic, place, transform.rotation);
                    place += new Vector3(0f, 0f, gridPicSize);
                }
            }
            place = new Vector3(gridStart.x + gridPicSize * i, gridStart.y, gridStart.z);
        }
    }
}
