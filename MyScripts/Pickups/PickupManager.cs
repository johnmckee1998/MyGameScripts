using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager instance;

    private int previousSpawnIndex; // what pickup was spawned last? used to avoid spawning the same one too often

    [Serializable]
    public struct Pickups
    {
        public GameObject pickup;
        [Tooltip("if >=1 then when chosen randomly, it will be spawned. Below 1, it will be compared to a random value to determine whether to spawn it or spawn somthing else")]
        public float spawnChance;
    }

    public Pickups[] pickupSet;

    private void Start()
    {
        instance = this;

        previousSpawnIndex = 100; //make it a stupid number so the first spawn isnt affected 
    }

    //[Tooltip("Returns a randomly selected pickup from array of pickups")]
    public GameObject GetPickup()
    {
        int randSelect = UnityEngine.Random.Range(0, pickupSet.Length);
        float randomChance = UnityEngine.Random.Range(0f, 1f);

        if (pickupSet[randSelect].spawnChance >= randomChance && randSelect != previousSpawnIndex)
        {
            previousSpawnIndex = randSelect;
            return pickupSet[randSelect].pickup;
        }

        return GetPickup();
    }

    public GameObject PickupFunction() //determines whether or not a pickup can be spawned, and if so returns one
    {
        float dropChance = UnityEngine.Random.Range(0f, 1f);
        if (WaveManagerScript.pickupAmount < (int)(1 + (WaveManagerScript.roundCount / 5))) //Every 5 rounds increase pickup limit by 1 (from a base of 1 per round) e.g. roudn 15 has a pickup limit of 4 
        {
            if (dropChance < 0.1f)
            {
                WaveManagerScript.pickupAmount++;
                return GetPickup();
            }
            else
                return null;
        }
        else
            return null;
    }

}
