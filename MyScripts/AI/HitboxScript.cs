using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxScript : MonoBehaviour
{
    public UniversalStats unistats;
    public float damageModifier = 1f;
    [HideInInspector]
    public float penResistance;

    public GameObject deathObj;
    public bool destroyOnDeath = true;
    [Tooltip("If a connected body is set, then this will be attached to it when unistats dies")]
    public Transform connectedObj;

    public bool unparentOnDeath = true;
    private bool die = false;

    public UniversalStats.armourResistance armour;

    //used to prevent damage and other things while using a death effect e.g. dissolve
    private bool effectDeath;
    // Start is called before the first frame update
    void Start()
    {
        if(unistats==null)
            unistats = GetComponentInParent<UniversalStats>();
        penResistance = unistats.PenResistance;
    }

    private void FixedUpdate()
    {
        if(unistats.health <=0 && transform.parent != null && !effectDeath && !unistats.GetEffectDeath()) // when unistats dies
        {
            //Debug.Log("Hb OnDeath");

            if (connectedObj != null)
            {
                transform.parent = connectedObj; //problem - if connected obj is destoryed it ends up destroying this too
                Destroy(gameObject,5f);
            }
            else if (unparentOnDeath)
            {
                transform.parent = null;
                Destroy(gameObject,5f);
            }
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddTorque(new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f)));//put a bit of torque to make them fall over
            }
            
        }
    }

    public void DoDamage(Vector3 hitPos, Quaternion hitRot, float damage, bool b = true, UniversalStats.DeathType dType = UniversalStats.DeathType.none)
    {
        if (!effectDeath)
        {
            unistats.AddDamage(hitPos, hitRot);
            unistats.PlayHitEffect(hitPos, hitRot);
            unistats.PlayHitSound();
            unistats.DoDamage((damage * damageModifier), b, dType);
            if (unistats.health <= 0 && deathObj != null) // when unistats dies and this was the last part to be hit OR if this is hit after unistats has died
            {
                die = true;
            }
        }
    }

    public void DoDamage(float damage, bool b = true, UniversalStats.DeathType dType = UniversalStats.DeathType.none)
    {
        if(!effectDeath)
            unistats.DoDamage(damage*damageModifier, b, dType);
    }

    private void LateUpdate()
    {
        if (die && !effectDeath)
        {
            if (deathObj != null)
            {
                GameObject g = Instantiate(deathObj, transform.position, transform.rotation);
                g.transform.localScale = transform.localScale;
                Destroy(g, 5f);
                g.transform.parent = null;
            }
            if(destroyOnDeath)
                Destroy(gameObject);
        }
    }

    public void SetOnFire()
    {
        unistats.SetOnFire();
    }

    public void Dissolve() //used for instantaneous dissolve, if done only when killed then that is handelled by dodamage and unistats
    {
        if (!effectDeath)
        {
            effectDeath = true;
            if (DeathEffectsManager.instance != null)
            {
                DeathEffectsManager.instance.Dissovle(transform);
                if (!unistats.GetEffectDeath())
                {
                    unistats.EffectDeath();
                    Destroy(transform.parent.gameObject, 60f); //give the effet 1 minute, then destroy parent
                }
            }
            else
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }

    public void Burn(bool b)
    {
        if (!effectDeath)
        {
            effectDeath = true;
            if (DeathEffectsManager.instance != null)
            {
                DeathEffectsManager.instance.Burn(transform, b);
                if (!unistats.GetEffectDeath())
                {
                    unistats.EffectDeath();
                    Destroy(transform.parent.gameObject, 60f); //give the effet 1 minute, then destroy parent
                }
            }
            else
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }
}
