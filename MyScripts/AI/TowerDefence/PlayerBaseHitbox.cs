using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseHitbox : MonoBehaviour
{
    public int index;
    public void HitByBullet(float f)
    {
        TDPlayerBase.instance.DamageBase(f, index);
    }
}
