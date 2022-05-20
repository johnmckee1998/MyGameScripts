using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngine.Events;
using TMPro;

public class WaveManagerScript : MonoBehaviour
{
    public static WaveManagerScript instance;
    public float timeBetweenRounds = 25f;
    private int EnemySpawners;
    private int activeSpawns = 0;
    //public GenericSpawner test;
    //public GameObject[] SecondarySpawners;
    //private GenericSpawner[] spawners; //Depreciated - Generic spawners no longer used
    private Transform[] spawnPositions;
    [Tooltip("Prioritise Spawns near player")]
    public bool useBiasSpawns = true;
    public bool slowEarlyRounds = true;
    //[Tooltip("Amount of objects to spawn in each spawner")]
    //public int perSpawnerAmount = 10;
    public int TotalSpawnPool = 10;
    private int SpawnPoolCount = 0; //only really used to track exact amount that are spawning, could be removed


    //[Tooltip("Base value to be used for the x in:  y = ((1.0/z)*(x-z)^2 + 3) /2")] //Z = spawn amount/2, x is current object count for each spawner, y is wait time
    //public float spawnIntervalBase = 2f;
    public static int roundCount = 1;
    public static int pickupAmount;

    public Text roundText;
    public TextMeshProUGUI roundTextTMP;
    public Text killText;

    public static int KillCount = 0;
    public static bool pauseSpawning = false; //For stopping spawning when a certain number of enemies are spawned
    public int maxAlive = 5; //maximum  number of enemies that can exists at once
    private int aliveCount = 0;

    //[Tooltip("How many of the given spawners to use at once")]
    //private int UseSpawners = 1;
    private int[] spawnerToUse;

    private bool roundFinished = false;
    bool roundStarted = true;
    public static bool allDead = false;

    public static DumbAi[] enemies;

    private int randSpawnAmount = 0;

    public AnimationCurve spawnRate;
    public AnimationCurve globalSpawnRate;
    public AnimationCurve highRoundSpawnRate;


    [Space]
    public UnityEvent newRoundEvent;
    public UnityEvent endRoundEvent;

    [Serializable]
    public struct EnemyTypes
    {
        public GameObject enemy;
        public int spawnFromRound;
        [Tooltip("if >=1 then when chosen randomly, it will be spawned. Below 1, ir will be compared to a random value to determine whether to spawn it or spawn somthing else")]
        public float spawnChance;
    }

    //Make the private field of our PlayerStats struct visible in the Inspector
    //by applying [SerializeField] attribute to it

    [Header("Enemy Info")]

    [SerializeField]
    private EnemyTypes[] enemiesToSpawn;

    //public GameObject[] enemyTypes; //maybe make this array for multiple enemies?
    //[Tooltip("What round should the corresponding enemy spawn from: NOTE: MUST BE IN ACENDING ORDER")]
    //public int[] enemyTypeRound;
    [Space]
    [Tooltip("Whether health is scaled by wave count")]
    public bool scaleHealth = false;
    [Range(0.1f, 10f)]
    [Tooltip("Lower value means faster health increase")]
    public float scaleFactor = 1f;

    private int leftToSpawn =0;


    public VisualEffect spawnEffect;
    public AudioSource spawnSound;

    private int maxEnemyType = 0; //used to limit which enemies are checked for enemy spawin - if it cant be spawned this round dont bother to check it

    private int spawnChoiceCount; //used to prevent endless looping of ChooseRandom


    [Header("Special Wave")]
    public bool useSpecialWave; //use SendMessage("StartWave", SendMessageOptions.DontRequireReceiver); to trigger wave
    [Range(2,10)]
    public int specialWaveInterval = 5;
    public Transform specialWaveTrigger;
    
    private bool specialWaveFinished;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        EnemySpawners = transform.childCount;
        CountActiveSpawns();

        //UseSpawners = activeSpawns / 2;

        //RandomiseList(); //Randomly select values to choose spawners

        ActivateSpawns();

        //Making sure static values dont carry over between restarts
        KillCount = 0;
        pauseSpawning = false;
        roundCount = 1;
        pickupAmount = 0;
        allDead = false;

        StartCoroutine(CurrentRound());
    }

    // Update is called once per frame
    void Update()
    {
        if(roundText!=null)
            roundText.text = "ROUND " + roundCount.ToString();
        else if (roundTextTMP!=null)
            roundTextTMP.text = "ROUND " + roundCount.ToString();
        if (killText!=null)
            killText.text = "Kills: " + KillCount + " Round: " + roundCount;

        if((!useSpecialWave || ((roundCount) % specialWaveInterval != 0)) && roundStarted)
            CheckAlive(); //to determine when to pause spawning - stop too many spawning at once
        //Otherwise, wait for special wave to trigger end

        //for (int i=0; i<activeSpawns; i++)
        //{
            //Debug.Log(g2.gameObject.name);

            if (roundStarted) // detects when spawners are finished spawning - used in conjunction with alldead to determine when the round is actually finished, this bool is poorly named
            {
                //roundFinished = (spawners[i].SpawnAmount == 0); //DEPRECIATED
                roundFinished = (leftToSpawn == 0);
                //if (roundFinished == false)
                    //break;
            }

        //}
        if (roundFinished)
        {
            //enemies = FindObjectsOfType<DumbAi>(); //probably a better way of doing this, what about making all ai children of wavemanager or something
            //allDead = true;
            //foreach(DumbAi g3 in enemies)
            //{
            //    if (g3.alive)
            //        allDead = false;
            //}

            allDead = (aliveCount == 0);
        }
        if (useSpecialWave && ((roundCount) % specialWaveInterval == 0))
            roundFinished = specialWaveFinished;

        if (roundFinished && roundStarted && allDead)//if currnet round should end
        {
            if(!useSpecialWave)
                StartCoroutine(NextRound());
            else
            {
                if ((roundCount + 1) % specialWaveInterval == 0) //if next round is special
                    StartCoroutine(SpecialRound());
                else
                    StartCoroutine(NextRound());
            }
            roundStarted = false;
        }
        if (Input.GetKeyDown("k"))
        {
            Debug.Log("Kills: " + KillCount);
            Debug.Log("ActiveSpawns: " + activeSpawns);
            Debug.Log("SpawnPool: " + TotalSpawnPool);
        }
    }

    IEnumerator NextRound()
    {
        if(roundText!=null)
            roundText.color = Color.blue;
        if (endRoundEvent != null)
            endRoundEvent.Invoke();
        yield return new WaitForSeconds(timeBetweenRounds);
        roundCount++;
        roundStarted = true;
        roundFinished = false;
        specialWaveFinished = false;
        CountActiveSpawns();
        pickupAmount = 0;
        //Debug.Log("New Set " + activeSpawns / 2);
        //UseSpawners = activeSpawns / 2;

        //RandomiseList(); //not used anoymore

        TotalSpawnPool = (int)(TotalSpawnPool * 1.25f);

        maxEnemyType = 0; //used to limit which enemies are checked for enemy spawin - if it cant be spawned this round dont bother to check it
        for (int i = 0; i < enemiesToSpawn.Length; i++)
        {
            if (enemiesToSpawn[i].spawnFromRound <= roundCount)
                maxEnemyType++;
            else
                break;
        }
        if (maxEnemyType > enemiesToSpawn.Length) //just an out of bounds check
            maxEnemyType = enemiesToSpawn.Length;

        //ActivateSpawns(); //not used as now the spawning is handled by wavemanager, not the individual spawners


            /*
            for (int b =0; b<spawners.Length; b++)
            {
                //roundFinished = (g2.SpawnAmount == 0);
                foreach(int c in spawnerToUse) //Find those that have been selected
                {
                    if (b == c)
                    {

                        spawners[b].SpawnAmount = (TotalSpawnPool / activeSpawns);
                        //spawners[b].SpawnAmount = perSpanerAmount + (roundCount - 1);
                        spawners[b].ResetSpawnCount();
                    }
                }
            }*/
        if(roundText!=null)
            roundText.color = Color.black;

        StartCoroutine(CurrentRound());
        if (newRoundEvent != null)
            newRoundEvent.Invoke();
        //SetupNextRound
    }

    private void RandomiseList()
    {
        //UseSpawners = activeSpawns;
        //Debug.Log("AMount " + UseSpawners);
        if (spawnerToUse==null)
            spawnerToUse = new int[activeSpawns];

        for (int a = 0; a < spawnerToUse.Length; a++) //randomly select which spawners to use -> why
        {
            spawnerToUse[a] = UnityEngine.Random.Range(0, activeSpawns-1);
            //Debug.Log(spawnerToUse[a]);
            for(int b =0; b < spawnerToUse.Length; b++) //Check to make sure value does not repeat
            {
                if (spawnerToUse[a] == spawnerToUse[b] && a!=b)
                {
                    spawnerToUse[a] = UnityEngine.Random.Range(0, EnemySpawners);
                    b = -1; //Loop through again until all values are different
                }
            }
            //Debug.Log(spawnerToUse[a]);
        }

    }

    private void CountActiveSpawns()
    {
        activeSpawns = 0;
        //spawners = new GenericSpawner[EnemySpawners]; //reset array
        spawnPositions = new Transform[EnemySpawners]; //reset array
        int count = 0; //array index
        foreach(Transform sp in transform)
        {
            if (sp.gameObject.activeSelf)
            {
                activeSpawns++; //record active spawner
                //spawners[count] = sp.GetComponent<GenericSpawner>(); //add the active spawn to the list //DEPRECIATED
                spawnPositions[count] = sp;
                count++; //index array
            }
        }
        //Debug.Log("Active: " + activeSpawns + " Avaliable: ");
        //UseSpawners = activeSpawns;
        //Debug.Log("Active spawn count: " + activeSpawns);
    }

    private void ActivateSpawns() //not used anymore i think
    {
        //int i = 0;
        //spawners = new GenericSpawner[activeSpawns];
        SpawnPoolCount = 0;
        int testing = 0;
        for (int i =0; i<activeSpawns; i++)
        {
            //spawners[i] = EnemySpawners[i].GetComponent<GenericSpawner>();
            /*
            foreach (int j in spawnerToUse)
            {
                if (i == j) //activate spawners which were selected
                {
                    spawners[i].SpawnAmount = (TotalSpawnPool / activeSpawns);
                    SpawnPoolCount += (TotalSpawnPool / activeSpawns);
                    testing++;
                    break;
                }
                else // set non active spawners to 0
                    spawners[i].SpawnAmount = 0;


            }*/
            //randSpawnAmount = Random.Range(1, (TotalSpawnPool-SpawnPoolCount+1)/2); //spawn a random amount between 1 and half of whats left (the +1 is coz random(int,int) is not inclusive of second number, unlick random(float,flaot)

            /*
            if (TotalSpawnPool / activeSpawns < 1) //only used for early rounds really - if spawn amount is less than 1, make it =1 coz cant spawn half an enemy
            {
                spawners[i].SpawnAmount = 1;
                SpawnPoolCount++;
            }
            else
            {
                spawners[i].SpawnAmount = (TotalSpawnPool / activeSpawns);
                //spawners[i].SpawnAmount = randSpawnAmount;
                SpawnPoolCount += (TotalSpawnPool / activeSpawns);
            }
                
            //SpawnPoolCount += randSpawnAmount;
            testing++;
            spawners[i].ResetSpawnCount();
            StartCoroutine(spawners[i].SpawnerFunc());
            */
            //i++;
        }
        if (SpawnPoolCount != TotalSpawnPool)
        {
            //Debug.Log("Not right: " + SpawnPoolCount + " : " + TotalSpawnPool + " | " + testing + (TotalSpawnPool - SpawnPoolCount));
            //spawners[0].SpawnAmount += (TotalSpawnPool - SpawnPoolCount); //Just put leftovers in the first spawner
        }
    }

    private void CheckAlive()
    {
        int tempAliveCount = 0; //use temp just to make sure aliveCount isnt momentarily set to 0 when it shouldnt, which would cause allDead to be triggered at the wrong time
        //enemies = FindObjectsOfType<DumbAi>(); //Depreciated? coz enemies are now children of transform
        enemies = GetComponentsInChildren<DumbAi>();
        foreach (DumbAi g3 in enemies)
        {
            if (g3.alive)
                tempAliveCount++;
        }
        aliveCount = tempAliveCount;
        if (aliveCount >= maxAlive)
            pauseSpawning = true;
        else
            pauseSpawning = false;
    }


    //spawn enemies through wavemanager - rather than individual spawners - means things like dividing up spawns evenly arnt a problem and overall spawnrate - particularly the build up and taper off that i want - should be better
    private IEnumerator CurrentRound()
    {
        
        leftToSpawn = TotalSpawnPool;
        float TTW;
        yield return new WaitForSeconds(5f); //this nessecary? i have a wait in next round, could jsut make that longer *************************
        while (leftToSpawn > 0)
        {
            int randSpawner = UnityEngine.Random.Range(0, activeSpawns); //would usually think this would be activeSpawns-1, but rand(int, int) is exclusive on last, so dont need it

            if (useBiasSpawns)
            {
                int loopCount = 0;
                while (Vector3.Distance(spawnPositions[randSpawner].position, CharacterControllerScript.instance.transform.position) > 50f) //if the spawner is too far away, then find one that is closer
                {                                                                                                                       //Not the best solution, ineficient when lots of spawners active and relies on randomly finding close spawner
                    randSpawner = UnityEngine.Random.Range(0, activeSpawns - 1); //choose a new spawner
                    if (loopCount > 10)
                    {
                        Debug.Log("Long Loop");
                        break; //used to stop long loops
                    }
                    else
                        loopCount++;
                }
            }

            if (!pauseSpawning)
            {
                SpawnOne(spawnPositions[randSpawner].position); //potential improvement -> make spawners have a sphere trigger or just do a distance check to see if player is near, then only use spawners that are near player
                leftToSpawn--;
            }
            if(roundCount<10)
                TTW = globalSpawnRate.Evaluate(leftToSpawn / TotalSpawnPool);
            else //>=10
                TTW = highRoundSpawnRate.Evaluate(leftToSpawn / TotalSpawnPool);
            if(slowEarlyRounds)
                if (roundCount <= 5) //at rounds 5 and below, slow spawn a bit
                    TTW += (-0.5f*roundCount) + 2.5f;                    // y = -0.5x + 2.5
            yield return new WaitForSeconds(TTW);
        }
    }

    private void SpawnOne(Vector3 p)
    {
        spawnChoiceCount = 0;
        int randEnemy = ChooseRandom(UnityEngine.Random.Range(0, maxEnemyType));
        
        GameObject en = Instantiate(enemiesToSpawn[randEnemy].enemy, p, transform.rotation, transform);
        if (spawnEffect != null)
        {
            spawnEffect.transform.position = p;
            spawnEffect.Play();
        }
        AudioSource.PlayClipAtPoint(spawnSound.clip, p,spawnSound.volume);
        if (scaleHealth)
        {
            float scaleAmount = (roundCount / (15f * scaleFactor)) + 1f; //scales linearlly -> if scaleFactor is 1 health is double by round 20, at 1.5x at round 10, 3x at 40 -> so every twenty levels health increases by 100%
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

    public int GetRound()
    {
        return roundCount;
    }

    public void KillAllEnemies()
    {
        enemies = enemies = GetComponentsInChildren<DumbAi>();
        foreach (DumbAi g3 in enemies)
        {
            if (g3.alive)
                g3.GetComponent<UniversalStats>().health = 0;
        }
    }

    private int ChooseRandom(int i)
    {
        spawnChoiceCount++;
        if (enemiesToSpawn[i].spawnChance >= 1 || spawnChoiceCount>=10) //if 1 or more, guarentee spawn -> or if this has been attempted 10 times already
            return i;
        
        if (UnityEngine.Random.Range(0f, 1f) <= enemiesToSpawn[i].spawnChance) //check chance
            return i;
        return ChooseRandom(UnityEngine.Random.Range(0, maxEnemyType)); // choose another number
    }

    public void FinishSpecialWave()
    {
        specialWaveFinished = true;
    }


    private IEnumerator SpecialRound()
    {
        //Debug.Log("Special Round!");
        if (roundText != null)
            roundText.color = Color.blue;
        if (endRoundEvent != null)
            endRoundEvent.Invoke();
        yield return new WaitForSeconds(10f);
        roundCount++;
        roundStarted = true;
        roundFinished = false;
        specialWaveFinished = false;

        if (roundText != null)
            roundText.color = Color.black;
        yield return null; //make sure that the previous info is set correctly before starting spawning
        specialWaveTrigger.SendMessage("StartWave", SendMessageOptions.DontRequireReceiver);

        //Debug.Log("Signal Sent!");
    }

    public int GetAlive()
    {
        return aliveCount;
    }

}
