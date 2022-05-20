using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawns")]
public class MultiplayerPlayerSpawns : ScriptableObject
{
    public string sceneID;
    public Vector3 defaultSpawn;
    public Vector3[] spawns;
}
