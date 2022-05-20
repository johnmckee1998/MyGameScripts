using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveRoundScript : MonoBehaviour
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

    public GameObject explosionEffect;

    private AudioSource boomSound;
    private Rigidbody rb;

    private bool isStuck = false;
    //private Vector3 stuckLoc;
    private Transform stucker;
    //private bool fired = false;
    private bool hitGround = false;

    // Start is called before the first frame update
    void Start()
    {
        startLoc = transform.position;

        rb = GetComponent<Rigidbody>();
        boomSound = transform.GetComponent<AudioSource>();
        //rb.velocity = transform.up*10;
        rb.velocity = travelDir;
        blood = transform.GetChild(0).gameObject;
        other = transform.GetChild(1).gameObject;
    }

    void Update()
    {
        if (isStuck)
            transform.position = stucker.position - (transform.forward*0.25f);


        curLoc = transform.position;
        if (Vector3.Distance(startLoc, curLoc) > range)
            Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (timeToExplode <= 0)
        {
            //Debug.Log("Boom!");
            AudioSource.PlayClipAtPoint(boomSound.clip, transform.position, 1.0f);
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, explodeRadius);
            foreach (Collider hit in colliders)
            {
                
                Rigidbody rbH = hit.GetComponent<Rigidbody>();

                if (hit.gameObject.tag == "Enemy")
                {
                    hit.gameObject.GetComponent<Guard>().health -= (explodeDamage / (Vector3.Distance(transform.position, hit.transform.position)));

                    if (hit.gameObject.GetComponent<Guard>().health <= 0)
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hitGround)
        {
            //Instantiate(explosionEffect, transform.position, transform.rotation);
            //Debug.Log("yee");
            if (collision.gameObject.tag == "Enemy")
            {
                GameObject b = Instantiate(blood, transform.position, transform.rotation);
                b.GetComponent<ParticleSystem>().Play();
                collision.gameObject.GetComponent<Guard>().health -= damage;
                collision.gameObject.GetComponent<Rigidbody>().AddForce(-80 * collision.transform.forward);
                //Debug.Log("Hit!");

                //Stick in enemy
                isStuck = true;
                //stuckLoc = collision.transform.position;
                stucker = collision.transform;
                gameObject.layer = 13;

                //Debug.Log("YEP enemy");
            }
            /*else if(collision.gameObject.layer == 11) //only stick into objects on wall layer
            {
                rb.velocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                transform.position -= 0.25f*transform.forward;
                //GameObject b = Instantiate(other, transform.position, transform.rotation);
                //b.GetComponent<ParticleSystem>().Play();
            }*/
            //timeToExplode = 0;
            //Destroy(gameObject);
            

            //Debug.Log("Hit ground " + hitGround + " enemy " + collision.gameObject.tag);
        }
        

        //if(collision.gameObject.tag == "Enemy" || collision.gameObject.layer == 11)
           // hitGround = true;
        
    }

    public void SetTravelDir(Vector3 dir)
    {
        travelDir = dir;
    }

}
