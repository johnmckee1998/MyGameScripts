using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericSpawner : MonoBehaviour
{
    [Tooltip("Object to spawn")]
    public GameObject spawnObject;
    [Tooltip("Interval between spawns")]
    public float spawnDelay = 5.0f;
    private float realSpawnDelay;

    public float spawnRangeX = 5.0f;
    public float spawnRangeZ = 5.0f;

    public bool enableSpawning = true;
    

    private Transform spawnPos;

    public Text uiText;
    public int SpawnAmount = 0; //how many objects left to spawn
    private int amountToSpawn; //how many objects will be spawned
    private int amountSpawned = 0; //how many have been spawned
    private string textDisplay = "Amount to spawn: ";

    float TTW = 0; //Time to wait

    [Tooltip("Whether health is scaled by wave count")]
    public bool scaleHealth = false;
    [Range(0.1f, 10f)]
    [Tooltip("Lower value means faster health increase")]
    public float scaleFactor = 1f;
    // Start is called before the first frame update

    
    void Start()
    {
        //StartCoroutine(SpawnerFunc());
        //uiText.text = textDisplay + SpawnAmount.ToString();
    }

    // Update is called once per frame
    public IEnumerator SpawnerFunc()
    {
        yield return new WaitForSeconds(spawnDelay*5f);
        while (enableSpawning)
        {
            if (SpawnAmount > 0 && !WaveManagerScript.pauseSpawning)
            {
                SpawnAmount--;
                amountSpawned++;
                float ratio = amountSpawned / amountToSpawn;
                spawnPos = transform;
                //spawnPos.position += new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0f, Random.Range(-spawnRangeZ, spawnRangeZ));
                GameObject en = Instantiate(spawnObject, spawnPos);

                if (scaleHealth)
                {
                    float scaleAmount = (WaveManagerScript.roundCount / (20f*scaleFactor)) + 1f; //scales linearlly -> if scaleFactor is 1 health is double by round 20, at 1.5x at round 10, 3x at 40 -> so every twenty levels health increases by 100%
                    try
                    {
                        en.GetComponent<UniversalStats>().health *= scaleAmount;
                    }
                    catch
                    {
                        //didnt have unistats
                    }
                }


                //uiText.text = textDisplay + SpawnAmount.ToString();

                //Calculating waitTime   
                //x = amountSpawned;
                //z = amountToSpawn / 2;
                //TTW = ((1.0f / (amountToSpawn / 2f)) * (Mathf.Pow(amountSpawned - (amountToSpawn / 2f), 2)) + 3f) / 2f; //old crap formula
                //TTW = Random.Range(1f, 3f); //newer simple rate - random between 1 and 3 seconds
                //Mathf.Clamp(TTW, 1.5f, 3); no need to clamp with random

                //new system for waittime - ramp up to max, then tamper off at end using animation curve
                TTW = WaveManagerScript.instance.spawnRate.Evaluate(ratio);

                yield return new WaitForSeconds(TTW);
            }
            else
                yield return new WaitForSeconds(1);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (transform.position + transform.forward));
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(transform.position, new Vector3(2 * spawnRangeX, 1, 2 * spawnRangeZ));
    }

    public void ResetSpawnCount()
    {
        amountSpawned = 0;
        amountToSpawn = SpawnAmount;
    }

    public void SpawnOne()
    {
        GameObject en = Instantiate(spawnObject, transform);

        if (scaleHealth)
        {
            float scaleAmount = (WaveManagerScript.roundCount / (20f * scaleFactor)) + 1f; //scales linearlly -> if scaleFactor is 1 health is double by round 20, at 1.5x at round 10, 3x at 40 -> so every twenty levels health increases by 100%
            try
            {
                en.GetComponent<UniversalStats>().health *= scaleAmount;
            }
            catch
            {
                //didnt have unistats
            }
        }
    }
}
