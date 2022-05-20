using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;

public class UniversalStats : MonoBehaviour
{
    public float health = 100f;
    public bool invulnerable = false;
    public bool destroyOnDeath = false;
    public bool unParentChildrenOnDeath;
    public bool GiveMoneyOnDeath;
    public int money = 100;
    [Header("working on")]
    //public ParticleSystem effectParticle; //If a particle effect is used on death, put it here
    private bool usesParticle;
    public VisualEffect effectVFX; //If a visual effect graph is used on death, put it here
    private bool usesVFX;
    public AudioSource hitSound; //sound to play when hit
    public GameObject hitEffect; //effect to play when hit
    public GameObject DamageDecal; //decal to display damage

    public GameObject ObjOnDeath;
    [Tooltip("How long the ObjOnDeath will remain before being destroyed -> 0 means never destroyed")]
    public float DeathObjLife = 0f;
    [Tooltip("The best spot for targetting scripts to use")]
    public Transform targetPos;
    private bool dead = false;

    [Header("Fire")]
    public bool flamable = false;
    private bool onFire = false;
    public VisualEffect fireEffect;
    [Tooltip("DPS")]
    public float fireDamage = 10f;
    private float fireTimer = 0;

    [Header("Material Properties")]
    [Tooltip("Penetration Resistance")]
    public float PenResistance = 10000;
    [Tooltip("Ricochet Chance")]
    [Range(0f,1f)]
    public float hardness = 1f;
    //[Tooltip("Guns with damage lower than this will not deal damage")]
    public enum armourResistance {none, light, medium, heavy };
    public armourResistance armour;

    private bool hitSoundWait; //wait a fixed update 
    private float prevHealth;

    public bool useHitmarker = true;

    public UnityEvent deathEvent;

    public enum DeathType {none, Dissolve };

    private bool effectDeath; //true when some other effect is handling death -> e.g. dissolve
    // Start is called before the first frame update
    void Start()
    {
       
        usesParticle = (hitEffect != null);

        usesVFX = (effectVFX != null);

        hitSound = GetComponent<AudioSource>();

        prevHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (invulnerable)
            health = 10000;
        

        /*if (prevHealth > health && prevHealth > 0) //depreaceated - handle by dodamage so that fire doesnt cause excess hitmarkers
        {
            if (health > 0) //if still alive, normal hitmark
                HitmarkerScript.instance.Hit();
            else 
                HitmarkerScript.instance.KillHit();
        }*/

        prevHealth = health;
    }

    private void LateUpdate()
    {
        if (health <= 0 && !dead)
        {
            if (!effectDeath)
                Death();
            dead = true;
        }
    }

    private void FixedUpdate()
    {
        if (hitSoundWait)
            hitSoundWait = false;
    }

    public void Death()
    {
        //Debug.Log("OnDeath");

        if (GiveMoneyOnDeath)
            PlayerMoney.Money += money;

        if(ObjOnDeath != null)
        {
            GameObject deathObj = Instantiate(ObjOnDeath, transform.position, transform.rotation);
            if (DeathObjLife > 0)
                Destroy(deathObj, DeathObjLife);
        }

        if (unParentChildrenOnDeath)
        {
            foreach (Transform child in transform)
            {
                Rigidbody childRB = child.GetComponent<Rigidbody>();
                if (childRB != null) //make sure rigidbodies drop and act correctly
                {
                    childRB.isKinematic = false;
                    childRB.useGravity = true;
                }
                child.parent = null;
            }
        }

        if (deathEvent != null)
            deathEvent.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, DeathObjLife);
        //OnDestroy is also a function, maybe could look into that
    }

    public void PlayHitSound()
    {
        if (hitSound != null && !hitSoundWait)
            hitSound.PlayOneShot(hitSound.clip, hitSound.volume);

        
        //else
            //Debug.Log("No hitsound assigned to UniStats " + gameObject.name);
    }

    public void PlayHitEffect(Vector3 pos, Quaternion rot) //Weird note - mesh colliders work fine for rotations, but box colliders seem to get confused and so effects dont always work right
    {
        if (hitEffect != null)
        {
            if (usesParticle)
            {
                GameObject hitObj = Instantiate(hitEffect, pos, rot); //Create hit effect at point pos with rotation rot
                ParticleSystem p = hitObj.GetComponent<ParticleSystem>();
                if (p != null)
                    p.Play();
                Destroy(hitObj, 2.5f); //destroy effect obj after 1 second
            }
            else if (usesVFX)
            {

            }
        }
    }

    public void DoDamage(float d, bool b = true, DeathType dType = DeathType.none)
    {
        if (!invulnerable && !dead)
        {
            if (dType == DeathType.none || d<health) //no special death type or damage is less than health, just do damage as normal 
            {
                health -= d;
                if (useHitmarker && b)
                {
                    if (health > 0) //if still alive, normal hitmark
                        HitmarkerScript.instance.Hit();
                    else
                        HitmarkerScript.instance.KillHit();
                }
            }
            else if(dType== DeathType.Dissolve)
            {
                health -= d;
                Dissolve();
            }
        }
    }


    public void AddDamage(Vector3 pos, Quaternion rot)
    {
        if (DamageDecal != null)
        {
            GameObject DamageEffect = Instantiate(DamageDecal, pos, rot); //Create damage effect at point pos with rotation rot
            Destroy(DamageEffect, 25f); //destroy effect obj after 25 second
        }
    }

    public void SetOnFire()
    {
        
        if (flamable)
        {
            
            if (!onFire) //if not already on fire, set on fire
            {
                
                onFire = true;
                StartCoroutine(FireDamage());
            }
            fireTimer = 5f; //whether already on fire or just set, reset timer to max
        }
    }

    IEnumerator FireDamage()
    {
        Debug.Log("onFire!");
        if(fireEffect!=null)
            fireEffect.Play();
        while (onFire)
        {
            if (fireTimer <= 0)
            {
                onFire = false;
                if (fireEffect != null)
                    fireEffect.Stop();
            }
            else if (onFire)
            {
                if (health > 0)
                    DoDamage((fireDamage / 2f));
                else
                {
                    HitmarkerScript.instance.KillHit();
                    fireTimer = 0;
                    
                }
                PlayHitSound();
                fireTimer -= 0.5f;
            }
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Exstinguished");
    }

    
    public void Dissolve()
    {
        if (!effectDeath)
        {
            effectDeath = true;

            if (DeathEffectsManager.instance != null)
            {
                DeathEffectsManager.instance.Dissovle(transform);
            }
            else
            {
                Destroy(gameObject);
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
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private HitboxScript[] GetHitboxes() //unused
    {
        HitboxScript[] hbs = GetComponentsInChildren<HitboxScript>();
        if (hbs.Length == 0)
            return null;
        
        return hbs;
    }

    public void EffectDeath() //tell this script that an effect death is being used elsewhere -> most likely by hitbox
    {
        effectDeath = true;
        health = 0f;

        Debug.Log("Effect Death");
    }

    public bool GetEffectDeath()
    {
        return effectDeath;
    }

}
