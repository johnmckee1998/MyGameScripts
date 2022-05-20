using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameScript : MonoBehaviour
{
    private ParticleSystem flame;
    public float damage = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        flame = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        //Debug.Log(other.name + " HIT!");
        UniversalStats unistats = other.GetComponent<UniversalStats>();

        if (unistats != null)
        {
            if (unistats.armour == UniversalStats.armourResistance.none)
            {
                unistats.DoDamage(damage);
                unistats.PlayHitSound();
                unistats.SetOnFire();
            }
            //set on fire as well
        }
        else
        {
            HitboxScript hb = other.GetComponent<HitboxScript>();

            if (hb != null)
            {
                hb.DoDamage(other.transform.position, Quaternion.identity, damage);
                hb.SetOnFire();
            }
        }
    }
}
