using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDProceduralWave : MonoBehaviour
{
    public float totalHealth = 1000;
    public float spawnRate = 600f;
    private int currentWaveHealth;
    [System.Serializable]
    public struct Enemy
    {
        public GameObject enemy;
        public int health;
        public int spawnIndex; //probs have this randomised or something
        public int minSpawnWave; //to be used
        public TowerDefenceWaveManager.EnemyAimType type;
    }

    public Enemy[] enemies;


    private int maxSpawnIndex;
    private int groupSpawn;
    public void StartWave()
    {
        StartCoroutine(WaveFunction());
    }

    private IEnumerator WaveFunction()
    {
        //yield return new WaitForSeconds(TowerDefenceWaveManager.instance.waveCooldownTime);
        currentWaveHealth = (int)totalHealth;
        int curEnemy=0;
        SetMaxIndex();
        while (currentWaveHealth > 0)
        {
            curEnemy = Random.Range(0, maxSpawnIndex+1);//+1 is coz max is exlusive
            if (currentWaveHealth - enemies[curEnemy].health < 0) //enemy too strong
                curEnemy = GetLowestHealth();

            TowerDefenceWaveManager.instance.SpawnEnemy(enemies[curEnemy].enemy, enemies[curEnemy].spawnIndex, enemies[curEnemy].type);
            //Instantiate(enemies[curEnemy].enemy, TowerDefenceWaveManager.instance.enemySpawnPoints[enemies[curEnemy].spawnIndex].position, TowerDefenceWaveManager.instance.enemySpawnPoints[enemies[curEnemy].spawnIndex].rotation, transform);
            currentWaveHealth -= enemies[curEnemy].health;
            yield return new WaitForSeconds( enemies[curEnemy].health/ spawnRate); //improve this - wait should scale with round
        }
        while (TowerDefenceWaveManager.instance.CheckEnemies())
            yield return new WaitForFixedUpdate();
        TowerDefenceWaveManager.instance.NextWave();
    }

    private int GetLowestHealth()
    {
        int lowestIndex = 0;
        for(int i=0; i<=maxSpawnIndex; i++)
        {
            if (enemies[i].health < enemies[lowestIndex].health)
                lowestIndex = i;
        }
        return lowestIndex;
    }

    private void SetMaxIndex()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (TowerDefenceWaveManager.instance.WaveNumber() < enemies[i].minSpawnWave)
            { //if this is true, then this index shouldnt spawn and all lower ones are ok
                maxSpawnIndex = i-1;
                maxSpawnIndex = Mathf.Clamp(maxSpawnIndex, 0, enemies.Length-1);
                return;
            }
        }
        //if it reaches here, all spawns are less than current wave, so max out spawn index
        maxSpawnIndex = enemies.Length-1;
    }
}
