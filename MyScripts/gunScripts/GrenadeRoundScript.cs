using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class GrenadeRoundScript : MonoBehaviour
{
    private Vector3 startLoc;
    private Vector3 curLoc;

    public float damage;
    public float explodeDamage = 150;
    public float range;

    private GameObject blood;
    private GameObject other;

    private Vector3 travelDir = Vector3.zero;

    public float timeToExplode = 3f;
    public float explodeRadius = 5f;
    public float explodeForce = 1000f;

    [Tooltip("Explode on impact with any collider")]
    public bool explodeOnImpact = false;

    public GameObject explosionEffect;

    private AudioSource boomSound;

    private bool isStuck = false;
    //private Vector3 stuckLoc;
    private Transform stucker;
    //private bool fired = false;
    private bool hitGround = false;

    // Start is called before the first frame update
    void Start()
    {
        startLoc = transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        boomSound = transform.GetComponent<AudioSource>();
        //rb.velocity = transform.up*10;
        rb.velocity = travelDir;
        blood = transform.GetChild(0).gameObject;
        other = transform.GetChild(1).gameObject;

        //rb.angularVelocity = new Vector3(0,0,20f);
    }

    void Update()
    {
        //if (isStuck)
        //transform.position = stucker.position - (transform.forward*0.25f);
        boomSound.pitch = Time.deltaTime;

        curLoc = transform.position;
        if (Vector3.Distance(startLoc, curLoc) > range)
            Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (timeToExplode <= 0)
        {
            //Debug.Log("Boom!");
            AudioSource.PlayClipAtPoint(boomSound.clip, transform.position, boomSound.volume);
            GameObject explodeObj = Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(explodeObj, 5);
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, explodeRadius);
            foreach (Collider hit in colliders) //explosion damage
            {
                
                Rigidbody rbH = hit.GetComponent<Rigidbody>();
                UniversalStats uni = hit.GetComponent<UniversalStats>();
                if (uni != null)
                {
                    uni.health -= (explodeDamage / (Vector3.Distance(transform.position, hit.transform.position)));
                }
                //Damage enemy
                else if (hit.gameObject.tag == "Enemy")
                {
                    hit.gameObject.GetComponent<Guard>().health -= (explodeDamage / (Vector3.Distance(transform.position, hit.transform.position)/4));
                    //ragdoll enemy
                    if (hit.gameObject.GetComponent<Guard>().health <= 0)
                    {
                        hit.gameObject.GetComponent<Rigidbody>().freezeRotation = false;
                        hit.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    }
                    //Debug.Log("Dist: " + (Vector3.Distance(transform.position, hit.transform.position) / 4) + " Damage: " + (explodeDamage / (Vector3.Distance(transform.position, hit.transform.position) / 4)));
                }
                else if (hit.gameObject.tag == "Player")
                {
                    hit.gameObject.GetComponent<CharacterControllerScript>().health -= ((explodeDamage/2) / (Vector3.Distance(transform.position, hit.transform.position)));

                    CameraShaker.Instance.ShakeOnce(2f, 2f, 0.1f, 1f);
                    //Debug.Log("Dist: " + (Vector3.Distance(transform.position, hit.transform.position) / 4) + " Damage: " + (explodeDamage / (Vector3.Distance(transform.position, hit.transform.position) / 4)));
                }

                if (hit.gameObject.tag == "DumbEnemy")
                {
                    if (hit.gameObject.GetComponent<DumbAi>().health <= 0)//ragdoll
                    {
                        hit.gameObject.GetComponent<Rigidbody>().freezeRotation = false;
                        hit.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    }
                }





                if (rbH != null)
                {
                    rbH.AddExplosionForce(explodeForce, transform.position, explodeRadius, 3.0f);
                    //Debug.Log(hit);
                }
                //else
                    //Debug.Log("No rigidBody " + hit);

            }

            Destroy(gameObject);
        }
        else
            timeToExplode -= Time.deltaTime;

        //Debug.Log(timeToExplode + " Time Left");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hitGround || explodeOnImpact)
        {
            //Instantiate(explosionEffect, transform.position, transform.rotation);
            //Debug.Log("yee");
            UniversalStats uni = collision.gameObject.GetComponent<UniversalStats>();
            if (uni != null)
            {
                uni.health -= (explodeDamage / (Vector3.Distance(transform.position, collision.transform.position)));
                timeToExplode = 0;
            }

            else if (collision.gameObject.tag == "Enemy")
            {
                GameObject b = Instantiate(blood, transform.position, transform.rotation);
                b.GetComponent<ParticleSystem>().Play();
                collision.gameObject.GetComponent<Guard>().health -= damage;
                collision.gameObject.GetComponent<Rigidbody>().AddForce(-80 * collision.transform.forward);
                //Debug.Log("Hit!");

                //Stick in enemy
                //isStuck = true;
                //stuckLoc = collision.transform.position;
                //stucker = collision.transform;
                //gameObject.layer = 13;

                //Debug.Log("YEP enemy");
                timeToExplode = 0;
            }
            else if (collision.gameObject.tag == "DumbEnemy")
            {
                //GameObject b = Instantiate(blood, transform.position, transform.rotation);
                //b.GetComponent<ParticleSystem>().Play();
                collision.gameObject.GetComponent<UniversalStats>().health -= damage;
                collision.gameObject.GetComponent<Rigidbody>().AddForce(-80 * collision.transform.forward);
                timeToExplode = 0;
            }
            else if(explodeOnImpact)
            {
                timeToExplode = 0;
                //GameObject b = Instantiate(other, transform.position, transform.rotation);
                //b.GetComponent<ParticleSystem>().Play();
            }
            //timeToExplode = 0;
            //Destroy(gameObject);
            

            //Debug.Log("Hit ground " + hitGround + " enemy " + collision.gameObject.tag);
        }

        if(collision.gameObject.tag == "Enemy" || collision.gameObject.layer == 11)
            hitGround = true;
        //hitGround = true;
    }

    public void SetTravelDir(Vector3 dir)
    {
        travelDir = dir;
    }

}
