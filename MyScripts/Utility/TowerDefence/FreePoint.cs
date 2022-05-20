using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreePoint : MonoBehaviour
{
    public float leeway = 0.25f;
    public bool useNormalPoints = true;
    public void FreePointPostion()
    {
        TowerDefenceWaveManager.instance.FreeTurretPoint(transform.position, leeway, useNormalPoints);
    }

    public void FreePointIndex(int i)
    {
        TowerDefenceWaveManager.instance.FreeTurretPoint(i, useNormalPoints);
    }
}
