using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerDefenceWaveManager : MonoBehaviour
{
    public static TowerDefenceWaveManager instance;

    public Transform[] enemySpawnPoints; //could have multiple or different ones for different enemy types
    public Transform airEnemyParent; //separate parent so that turrets have an easier time targetting air units
    public Transform destination;
    public Transform airAcendPoint;
    public float destRadius = 3f;


    public float initialSetupTime = 30f;

    public float waveCooldownTime = 10f;

    [System.Serializable]
    public struct TDEnemyBlock //stores info on an enemy set that will spawn in a given wave - e.g. a block of 10 zombies or 5 drones
    {
        public int spawnCount;
        public float spawnRatePerSecond;
        public int spawnPoint;
        public EnemyAimType type;
        public GameObject enemy;
        [Tooltip("Time to wait after spawning this set before next enemy is spawned")]
        public float timeToWait;
    }
    [System.Serializable]
    public struct TDWave //stores all the individual block of enemies for a given wave in order - the manager spawns the enemies in a block then moves to next block. once all blocks spawned and killed, wave is ended
    {
        public string title;
        public TDEnemyBlock[] enemies; //atm only 1 set. Could have different sets for different difficulties
    }

    public TDWave[] waves;

    public enum EnemyAimType {Normal, Air};
    public Transform landingZonesParent;
    [HideInInspector]
    public Transform[] landingZones; //landing zones for drop ships
    private bool[] landingZonesInUse;
    public Transform[] missileZones; //points that missile ships stop at to launch missiles - could have different types of zomes, like different ranges or some in air some on ground
    private bool[] missileZonesInUse;
    public Transform friendlyAIParent;
    public Transform turretPointsParent;
    public Transform turretPointMarkers;
    public Transform turretLargePointsParent;
    [HideInInspector]
    public Transform[] turretPoints;
    [HideInInspector]
    public Transform[] turretLargePoints;
    private bool[] turretPointsInUse;
    private bool[] turretLargePointsInUse;
    private MeshRenderer[] turretMeshRens; //POTENTIAL PROBLEM - this relies on all turret points to have exactly 1 mesh renderer, no more no less
    private MeshRenderer[] turretLargeMeshRens; //POTENTIAL PROBLEM - this relies on all turret points to have exactly 1 mesh renderer, no more no less
    [Space]
    public bool useProceduralWave;
    public TDProceduralWave proWave;
    public int maxProWave = 5;
    [Space]
    public Collider bomberZone;
    private int currentWave;
    private int currentEnemy;
    private float timeToWait;
    private bool waveActive;
    [Header("UI Stuff")]
    public TextMeshProUGUI waveCount;
    public TextMeshProUGUI timeTillWave;
    public AudioSource finalCountSound;
    public AudioSource waveStartSound;
    public AudioSource waveWinSound;
    private int countDown;
    private int prevCountDown;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        if (useProceduralWave && proWave == null)
            proWave = GetComponent<TDProceduralWave>();
        timeToWait = initialSetupTime;

        landingZones = new Transform[landingZonesParent.childCount];
        for (int i = 0; i < landingZonesParent.childCount; i++)
            landingZones[i] = landingZonesParent.GetChild(i);

        turretPoints = new Transform[turretPointsParent.childCount];
        for (int i = 0; i < turretPointsParent.childCount; i++)
            turretPoints[i] = turretPointsParent.GetChild(i);

        turretLargePoints = new Transform[turretLargePointsParent.childCount];
        for (int i = 0; i < turretLargePointsParent.childCount; i++)
            turretLargePoints[i] = turretLargePointsParent.GetChild(i);

        landingZonesInUse = new bool[landingZones.Length];
        missileZonesInUse = new bool[missileZones.Length];
        turretPointsInUse = new bool[turretPoints.Length];
        turretLargePointsInUse = new bool[turretLargePoints.Length];

        turretMeshRens = turretPointsParent.GetComponentsInChildren<MeshRenderer>();
        turretLargeMeshRens = turretLargePointsParent.GetComponentsInChildren<MeshRenderer>();


        HideTurretPoints(true);
        HideTurretPoints(false);
    }

    private void Update()
    {
        if (timeToWait > 5) //when more than 5 seconds is left, let player skip countdown
            if (Input.GetKeyDown(KeyCode.F5))
                timeToWait = 5;

        //Should Probably go elsewhere, but this is the code to allow player to open Buy menu anywhere as oppossed to rpeviously when you had to interact with an object
        if (TDBuyMenu.instance != null && CharacterControllerScript.instance.health > 0)
            if (Input.GetButtonDown("BuyMenu") && Time.timeScale > 0 && !WeaponSelection.instance.IsPlacing())
                TDBuyMenu.instance.gameObject.SetActive(!TDBuyMenu.instance.gameObject.activeSelf);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timeToWait <= 0)
        {
            if(!useProceduralWave && !waveActive)
                StartCoroutine(WaveFunction());
            else if (!waveActive)
            {
                proWave.StartWave();
                waveActive = true;
                
            }
        }
        else
            timeToWait -= Time.fixedDeltaTime;

        if (timeTillWave != null)
        {
            if (!waveActive)
            {
                countDown = ((int)timeToWait);
                timeTillWave.text = "Next Wave: " + countDown.ToString() + "\n F5 To Skip";
                if (finalCountSound != null && prevCountDown != countDown && countDown<5)
                    finalCountSound.Play();
                prevCountDown = countDown;
            }
            else
                timeTillWave.text = "";
        }
        if (waveCount != null)
            waveCount.text = "Wave: " + (currentWave+1).ToString();

    }

    private IEnumerator WaveFunction()
    {
        waveActive = true;
        while (waveActive)
        {
            if (waves[currentWave].enemies[currentEnemy].spawnCount > 0)
            {
                if (timeToWait <= 0)
                {
                    if (waves[currentWave].enemies[currentEnemy].type == EnemyAimType.Normal) //normal spawn
                        Instantiate(waves[currentWave].enemies[currentEnemy].enemy, enemySpawnPoints[waves[currentWave].enemies[currentEnemy].spawnPoint].position, enemySpawnPoints[waves[currentWave].enemies[currentEnemy].spawnPoint].rotation, transform);
                    else //air spawn
                    {
                        GameObject g = Instantiate(waves[currentWave].enemies[currentEnemy].enemy, enemySpawnPoints[waves[currentWave].enemies[currentEnemy].spawnPoint].position, enemySpawnPoints[waves[currentWave].enemies[currentEnemy].spawnPoint].rotation, airEnemyParent);
                        g.SendMessage("FlyToDest", airAcendPoint, SendMessageOptions.DontRequireReceiver);
                    }
                    waves[currentWave].enemies[currentEnemy].spawnCount--;
                    //if (waves[currentWave].enemies[currentEnemy].spawnCount > 0) //if still using this block, use current rate, otherwise use next blocks rate
                    timeToWait = 1f / waves[currentWave].enemies[currentEnemy].spawnRatePerSecond;
                }   
                yield return null;
            }
            else
            {
                if (currentEnemy < waves[currentWave].enemies.Length - 1) //check that there is another enemy block 
                {
                    yield return new WaitForSeconds(waves[currentWave].enemies[currentEnemy].timeToWait);
                    currentEnemy++;
                }
                else if (!CheckEnemies())
                {
                    NextWave();
                }
                yield return null;
            }

        }
    }

    public void NextWave()
    {

        if ((currentWave < waves.Length - 1 && !useProceduralWave )||(useProceduralWave && currentWave+1<maxProWave)) //check there are still waves left
        {
            currentWave++;
            currentEnemy = 0;
            if (useProceduralWave)
                proWave.totalHealth *= 1.25f; 
            timeToWait = waveCooldownTime;
            waveActive = false;
            //Hard coded Player money bonus 
            PlayerMoney.Money += 500;

            waveWinSound.Play();
        }
        else
        {
            //player has completed all waves
            StopCoroutine(WaveFunction());
            Debug.Log("Won!");
            if (waveCount != null)
                waveCount.text = "Finished!";
            enabled = false;
        }
    }

    public bool CheckEnemies() //checks if enemies are alive
    {
        return (transform.childCount>0 || airEnemyParent.childCount>0);
    }

    private void OnDrawGizmosSelected()
    {
        if (destination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(destination.position, destRadius);
        }
    }

    public void SpawnEnemy(GameObject e, int sIndex, EnemyAimType t) //used by procedural wave to simplify spawning
    {
        if (sIndex > enemySpawnPoints.Length - 1)
            sIndex = enemySpawnPoints.Length - 1;//edge case catch

        if (t == EnemyAimType.Normal)
            Instantiate(e, enemySpawnPoints[sIndex].position, enemySpawnPoints[sIndex].rotation, transform);
        else //air spawn
        {
            GameObject g = Instantiate(e, enemySpawnPoints[sIndex].position, enemySpawnPoints[sIndex].rotation, airEnemyParent);
            g.SendMessage("FlyToDest", airAcendPoint, SendMessageOptions.DontRequireReceiver);
        }
    }

    public bool WaveStatus()
    {
        return waveActive;
    }

    public int WaveNumber()
    {
        return currentWave+1;
    }

    public int GetRandomLandingZone()
    {
        return GetRandomZoneOrPoint(landingZones, landingZonesInUse);
    }

    public void FreeLZ(int i)
    {
        if (i < landingZonesInUse.Length)
            landingZonesInUse[i] = false;//frees it up for use
    }

    public int GetRandomMissileZone()
    {
        return GetRandomZoneOrPoint(missileZones, missileZonesInUse);
    }

    private int GetRandomZoneOrPoint(Transform[] zones, bool[] inuse)
    {
        for (int i = 0; i < zones.Length; i++) //try missilezones.length number of random tries
        {
            int index = Random.Range(0, zones.Length);
            if (!inuse[index])
            {
                inuse[index] = true;
                return index;
            }
        }

        //if reaches here then random was unsuccesful, so cycle through all points to find first avaliable
        for (int i = 0; i < zones.Length; i++)
        {
            if (!inuse[i])//not in use, so use it
            {
                inuse[i] = true;
                return i;
            }
        }
        //if reaches here, no point avaliable, return negative to show failure
        return -1;
    }

    public void FreeMZ(int i)
    {
        if (i < missileZonesInUse.Length)
            missileZonesInUse[i] = false;//frees it up for use
    }

    public int GetClosestTurretPoint(Vector3 t, float range)
    {
        float closestDist = range * 5f;
        int index = -1;
        for (int i = 0; i < turretPoints.Length; i++)
        {
            if (!turretPointsInUse[i]) //not in use
            {
                float dist = Vector3.Distance(t, turretPoints[i].position);
                if(dist<range && dist < closestDist) //withing range and close
                {
                    index = i;
                    closestDist = dist;
                }
            }
        }
        //if (index >= 0) // if a valid point is found
        //    turretPointsInUse[index] = true; //mark it as in use -> done elsewhere
        return index;
    }

    public int GetClosestLargeTurretPoint(Vector3 t, float range)
    {
        float closestDist = range * 5f;
        int index = -1;
        for (int i = 0; i < turretLargePoints.Length; i++)
        {
            if (!turretLargePointsInUse[i]) //not in use
            {
                float dist = Vector3.Distance(t, turretLargePoints[i].position);
                if (dist < range && dist < closestDist) //withing range and close
                {
                    index = i;
                    closestDist = dist;
                }
            }
        }
        //if (index >= 0) // if a valid point is found
        //    turretPointsInUse[index] = true; //mark it as in use -> done elsewhere
        return index;
    }

    public void TurretPointUsage(int index, bool usage, bool t = true) //different to other functions because other scripts also need to mark the points as in use as well as not in use
    {
        if (t)
            turretPointsInUse[index] = usage;
        else
            turretLargePointsInUse[index] = usage;
    }

    public void ShowTurretPoints(bool t = true) //if t is true, enable normal points, if false enable large points
    {
        if (t)
        {
            for (int i = 0; i < turretMeshRens.Length; i++)
                if (!turretPointsInUse[i])
                {
                    turretMeshRens[i].enabled = true;
                    turretPointMarkers.GetChild(i).gameObject.SetActive(true);
                }
        }
        else
        {
            for (int i = 0; i < turretLargeMeshRens.Length; i++)
                if (!turretLargePointsInUse[i])
                    turretLargeMeshRens[i].enabled = true;
        }
    }


    public void HideTurretPoints(bool t=true)
    {
        if (t)
        {
            for (int i = 0; i < turretMeshRens.Length; i++)
                turretMeshRens[i].enabled = false;

            for (int i = 0; i < turretPointMarkers.childCount; i++)
                turretPointMarkers.GetChild(i).gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < turretLargeMeshRens.Length; i++)
                turretLargeMeshRens[i].enabled = false;
        }
    }

    public void FreeTurretPoint(Vector3 pos, float leeway = 0.2f, bool t =true)
    {
        if (t)
        {
            for (int i = 0; i < turretPoints.Length; i++)
                if (Vector3.Distance(turretPoints[i].position, pos) < leeway) //use distance rather than equals to give a slight margin of error due to turret offsets and floating point numbers being a bitch
                {
                    turretPointsInUse[i] = false;
                    return;
                }
        }
        else
        {
            for (int i = 0; i < turretLargePoints.Length; i++)
                if (Vector3.Distance(turretLargePoints[i].position, pos) < leeway) //use distance rather than equals to give a slight margin of error due to turret offsets and floating point numbers being a bitch
                {
                    turretLargePointsInUse[i] = false;
                    return;
                }
        }
    }

    public void FreeTurretPoint(int i, bool t = true)
    {
        if (t)
        {
            if (i < turretPointsInUse.Length)
                turretPointsInUse[i] = false;//frees it up for use
        }
        else
        {
            if (i < turretLargePointsInUse.Length)
                turretLargePointsInUse[i] = false;//frees it up for use
        }
    }


    public Transform GetClosestPlayerTarget(Vector3 compPos) //returns closest target on the players team - e.g. player or bots
    {
        float shortestDist = float.MaxValue;
        int shortestIndex = -1;
        float playerDist = float.MaxValue;
        if (CharacterControllerScript.instance.health > 0)
            playerDist = Vector3.Distance(compPos, CharacterControllerScript.instance.transform.position);
        if (friendlyAIParent.childCount > 0) //find closest bot
        {
            for(int i =0; i< friendlyAIParent.childCount; i++)
            {
                float tempDist = Vector3.Distance(friendlyAIParent.GetChild(i).position, compPos);
                if (tempDist < shortestDist)
                {
                    shortestDist = tempDist;
                    shortestIndex = i;
                }
            }
        }

        if (playerDist < shortestDist && CharacterControllerScript.instance.health > 0)
            return CharacterControllerScript.instance.transform;
        else if (shortestIndex > -1 && friendlyAIParent.childCount > 0)
            return friendlyAIParent.GetChild(shortestIndex);
        else //no valid target
            return null;


    }

    public Transform GetRandomPlayerTarget()
    {
        float playerORBot = Random.Range(0f, 3f);
        if (friendlyAIParent.childCount == 0 || playerORBot <= 1f)
            return CharacterControllerScript.instance.transform; //target player

        int randBot = Random.Range(0, friendlyAIParent.childCount);
        return friendlyAIParent.GetChild(randBot);

    }

    
}
