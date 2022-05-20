using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialEffects : MonoBehaviour
{
    /*this script is intended for static object to give them hit effects. For rigidbodies/destructable objects, use universal stats
     * 
     * 
     */

    public GameObject hitEffect;
    public AudioSource hitSound;
    public GameObject DamageDecal;
    [Header("Material Properties")]
    [Tooltip("Penetration Resistance")]
    public float PenResistance = 10000;
    [Tooltip("Ricochet Chance")]
    [Range(0f, 1f)]
    public float hardness = 1f;

    //[Tooltip("Valid types: concrete, metal, wood, dirt, grass, mud")]
    //public string soundType;

    public enum Sound {concrete, metal, wood, dirt, grass, mud, sand, none };

    public Sound typeOfMaterial;


    private bool waitAFrame;// used to prevent lots of simultaneous plays
    // Start is called before the first frame update
    void Start()
    {
        if (hitSound == null)
            hitSound = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (waitAFrame)
            waitAFrame = false;
    }

    public void playHitSound(Vector3 pos)
    {
        if(hitSound != null && !waitAFrame)
        {
            AudioSource.PlayClipAtPoint(hitSound.clip, pos, hitSound.volume);
            waitAFrame = true;
            //Debug.Log("Sound Played: " + gameObject.name);
        }
    }

    public void playHitEffect(Vector3 pos, Quaternion rot)
    {
        if(hitEffect != null)
        {
            GameObject hitObj = Instantiate(hitEffect, pos, rot); //Create hit effect at point pos with rotation rot
            Destroy(hitObj, 2.5f); //destroy effect obj after 1 second
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
}
