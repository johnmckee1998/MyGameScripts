using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class GrenadeScript : MonoBehaviour
{
    private Rigidbody rb;
    public float throwForce = 10000f;
    public float explodeDelay =  3f;
    public float explosionRadius = 2f;
    public float explosionDamage = 100f;
    public float explosionForce = 100f;
    public ParticleSystem explosionEffect;
    public AudioSource explosionSound;
    public bool disableMeshOnExplode = true;
    [Tooltip("optional extra object that will spawn on explode. Useful for making special grenades")]
    public GameObject spawnOnExplode;
    [Space]
    public bool fragmentation;
    [Tooltip("The gameobject that has a raycast bullet script on it- fragments are just bullets")]
    public GameObject fragment;
    public int fragmentCount = 2;
    [Space]
    public bool impact;
    public float impactDelay = 1f;
    private bool startImpactTimer;
    [Space]
    public float shakeRadius = 5f;
    public float shakeMagnitude = 0.5f;
    public float shakeRoughness = 0.5f;
    public float shakeFadeIn = 0.1f;
    public float shakeFadeOut = 0.1f;
    private bool exploded;
    [Space]
    public bool playerSuppression;
    [Range(1,100)]
    public float suppressionAmount = 1f;
    [Space]
    [Tooltip("Armour level this grenade can damage")]
    public UniversalStats.armourResistance armourDamage;
    public LayerMask ignoreLayers;
    [Space]
    [Tooltip("If true then applies force in the direction the player is looking, otherwise just applies it in the objects forward direction")]
    public bool usePlayerDirection = true;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(rb!=null) //used for special cases, like how i use grenades for explosive ammo
            if(usePlayerDirection)
                rb.AddForce(CharacterControllerScript.instance.pCam.transform.forward * throwForce);
            else
                rb.AddForce(transform.forward * throwForce);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (explodeDelay > 0)
        {
            if (Time.timeScale > 0)
            {
                explodeDelay -= Time.fixedDeltaTime;
                if (startImpactTimer)
                {
                    if (impactDelay > 0)
                        impactDelay -= Time.fixedDeltaTime;
                    else
                        explodeDelay = 0;
                }
            }
            
        }
        else if(!exploded)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (impact)
            startImpactTimer = true;
    }

    private void Explode()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        exploded = true;
        
        if (explosionEffect != null)
            explosionEffect.Play();
        if (spawnOnExplode != null)
            Instantiate(spawnOnExplode, transform.position, Quaternion.identity, transform.parent);
        if (explosionSound != null)
            explosionSound.Play();

        if(explosionRadius > 0 && explosionDamage > 0)
        {
            Vector3 explosionPos = transform.position;
            if (shakeRadius > 0 && Vector3.Distance(CharacterControllerScript.instance.transform.position, explosionPos) <=shakeRadius)
                CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeIn, shakeFadeOut);
            Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
            foreach (Collider hit in colliders) //explosion damage
            {
                if (((1 << hit.gameObject.layer) & ignoreLayers) == 0) //check if it is not in ignore layer
                { 
                    if (hit.tag.Equals("Player"))
                    {
                        float distanceModifier = (Mathf.InverseLerp(0, explosionRadius, Vector3.Distance(hit.transform.position, explosionPos)) - 1f) * -1f; //problem is inverse lerp returns 0 at 0m, and 1 at radius meters, i want the opposite so i do -1 and then *-1 to flip it
                        CharacterControllerScript.instance.health -= 0.75f * explosionDamage * distanceModifier;
                        if (playerSuppression)
                            SuppressionManager.instance.AddSurpression(suppressionAmount * distanceModifier);
                    }
                    else
                    {
                        Rigidbody rbH = hit.GetComponent<Rigidbody>();
                        UniversalStats uni = hit.GetComponent<UniversalStats>();
                        HitboxScript hb = hit.GetComponent<HitboxScript>();
                        if (rbH != null)
                        {
                            if (!rbH.isKinematic)
                                rbH.AddExplosionForce(explosionForce, explosionPos, explosionRadius);
                        }
                        if (uni != null)
                        {
                            if (uni.armour <= armourDamage)
                                uni.DoDamage(explosionDamage * (Mathf.InverseLerp(0, explosionRadius, Vector3.Distance(hit.transform.position, explosionPos)) - 1f) * -1f);
                        }
                        else if (hb != null)
                        {
                            if (hb.armour <= armourDamage)
                                hb.DoDamage(explosionDamage * (Mathf.InverseLerp(0, explosionRadius, Vector3.Distance(hit.transform.position, explosionPos)) - 1f) * -1f);
                        }
                        else
                        {
                            try
                            {
                                hit.gameObject.SendMessage("HitByBullet", explosionDamage * (Mathf.InverseLerp(0, explosionRadius, Vector3.Distance(hit.transform.position, explosionPos)) - 1f) * -1f, SendMessageOptions.DontRequireReceiver);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }

            if (fragmentation && fragment!=null)
            {
                for(int i =0; i <fragmentCount; i++)
                {
                    Quaternion direction = Quaternion.Euler(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360));
                    GameObject frag = Instantiate(fragment, transform.position, direction);
                }
            }
        }

        if (disableMeshOnExplode)
        {
            try
            {
                GetComponent<MeshRenderer>().enabled = false;
            }
            catch { }
        }

        Destroy(gameObject, 5f);

    }

}
