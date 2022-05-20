using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSpecialWave : MonoBehaviour
{
    public static TankSpecialWave instance;

    public GameObject[] lightTanks;
    public GameObject[] mediumTanks;
    public GameObject[] heavyTanks;
    [Space]
    public int lightTankSpawnRound = 5;
    public int mediumTankSpawnRound = 10;
    public int heavyTankSpawnRound = 15;
    public int baseSpawnAmount = 1;
    private int amountToSpawn;
    private bool waveEnded;
    private bool spawnActive;
    [System.Serializable]
    public struct TankPath
    {
        public Transform spawnPoint;
        public Transform destination;
        [HideInInspector]
        public bool used;
    }
    [Space]
    public TankPath[] paths;

    private int amountAlive;

    private enum TankRoundType { light, medium, heavy};
    private TankRoundType roundtype;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartWave()
    {
        //Debug.Log("Signal Recieved!");
        spawnActive = true;
        waveEnded = false;
        StartCoroutine(CurrentWave());
    }

    private IEnumerator CurrentWave()
    {
        amountToSpawn = 1 * (WaveManagerScript.instance.GetRound() / WaveManagerScript.instance.specialWaveInterval);
        
        //reset all spawn points to unused

        if (amountToSpawn > paths.Length)
            amountToSpawn = paths.Length; //prevent multiple spawns on same path
        for (int i = 0; i < paths.Length; i++)
            paths[i].used = false;

        //check to see if this is the first round of a type of tank spawning - in which case make sure only 1 spawns (e.g, if its the first time a medium tank spawns, dont spawn 3 only spawn 1)
        if (WaveManagerScript.instance.GetRound() <= lightTankSpawnRound) //light tank check
            amountToSpawn = 1;
        else if (WaveManagerScript.instance.GetRound() == mediumTankSpawnRound) //medium tank check
            amountToSpawn = 1;
        else if (WaveManagerScript.instance.GetRound() == heavyTankSpawnRound)//heavy tank check
            amountToSpawn = 1;
        amountAlive = amountToSpawn;
        CheckRoundType();

        while (!waveEnded)
        {
            //if none left to spawn, end spawning
            if (amountToSpawn == 0)
                spawnActive = false;
            //if still spawning, spawn 1
            if (spawnActive)
            {
                int spawnPos = Random.Range(0, paths.Length);
                while(paths[spawnPos].used) //keep looping till valid path is found
                    spawnPos = Random.Range(0, paths.Length);
                paths[spawnPos].used = true; //mark spawnpos as used
                //int tankSelect = Random.Range(0, lightTanks.Length);
                GameObject tank = Instantiate(GetTank(), paths[spawnPos].spawnPoint.position, paths[spawnPos].spawnPoint.rotation);
                try
                {
                    //+ new Vector3(Random.Range(1f,5f), 0f , Random.Range(1f, 5f))
                    tank.GetComponent<TankAiScript>().destination = paths[spawnPos].destination;

                }
                catch
                {
                    Debug.Log("Failed to set Destination: " + tank.name);
                }
                amountToSpawn--;
            }

            //if non left to spawn and all dead, end wave
            if (amountAlive == 0 && !spawnActive)
                waveEnded = true;

            yield return new WaitForSeconds(1f);
        }
        WaveManagerScript.instance.FinishSpecialWave();
    }

    private void OnDrawGizmosSelected()
    {
        if (paths.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < paths.Length; i++)
                if(paths[i].spawnPoint!=null)
                    Gizmos.DrawLine(paths[i].spawnPoint.position, paths[i].spawnPoint.position+ paths[i].spawnPoint.forward);
        }
    }

    public void TankDied()
    {
        amountAlive--;
    }

    private GameObject GetTank()
    {
        if (roundtype == TankRoundType.light)
        {
            int i = Random.Range(0, lightTanks.Length);
            return lightTanks[i];
        }
        else if (roundtype == TankRoundType.medium)
        {
            int i = Random.Range(0, mediumTanks.Length);
            return mediumTanks[i];
        }
        else
        {
            int i = Random.Range(0, heavyTanks.Length);
            return heavyTanks[i];
        }
    }

    private void CheckRoundType()
    {
        if (WaveManagerScript.instance.GetRound() < mediumTankSpawnRound) //light tank check
            roundtype = TankRoundType.light;
        else if (WaveManagerScript.instance.GetRound() < heavyTankSpawnRound) //medium tank check
            roundtype = TankRoundType.medium;
        else //heavy tank check
            roundtype = TankRoundType.heavy;
    }

    public int GetAlive()
    {
        return amountAlive;
    }
}
