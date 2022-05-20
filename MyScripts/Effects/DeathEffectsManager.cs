using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffectsManager : MonoBehaviour
{
    public static DeathEffectsManager instance;

    public GameObject dissolveObj;
    public GameObject burnObj;
    public GameObject burnDissolveObj;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void Dissovle(Transform p)
    {
        Instantiate(dissolveObj, p);
    }

    public void Burn(Transform p, bool b) //bool determines whether or not to dissovle after burning
    {
        if (b)
            Instantiate(burnDissolveObj, p);
        else
            Instantiate(burnObj, p);
    }
}
