using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Vector3 startLoc;
    private Vector3 curLoc;

    public float damage;
    public float range;

    //private GameObject blood;
    //private GameObject other;

    private Vector3 travelDir = Vector3.zero;
    Rigidbody rb;

    public float hitForce = 50f;

    public float timeToDie = 2f;

    public Vector3 prevPos;
    // Start is called before the first frame update
    void Start()
    {
        startLoc = transform.position;

        rb = GetComponent<Rigidbody>();
        //rb.velocity = transform.up*10;
        rb.velocity = travelDir;
        //blood = transform.GetChild(0).gameObject;
        //other = transform.GetChild(1).gameObject;

        Destroy(this, timeToDie);

        prevPos = transform.position;
    }

    void Update()
    {
        curLoc = transform.position;
        if (Vector3.Distance(startLoc, curLoc) > range)
            Destroy(gameObject);
    }

    void FixedUpdate()
    {
        prevPos = transform.position;
    }

    private void OnTriggerEnter(Collider collision)
    {
        UniversalStats uniStats = collision.gameObject.GetComponent<UniversalStats>();

        //Debug.Log("Hit: " + collision.gameObject.name);

        if (uniStats != null) //Damage objects/enemies with universal stats
        {
            uniStats.health -= damage;
            uniStats.PlayHitSound();
            uniStats.PlayHitEffect(prevPos, Quaternion.Inverse(transform.rotation));
            uniStats.AddDamage(transform.position, transform.rotation);

            //Rigidbody uniRb = collision.gameObject.GetComponent<Rigidbody>();

            //if (uniRb != null)
            //    uniRb.AddForce(-80*collision.transform.forward);
        }

        Rigidbody colRB = collision.gameObject.GetComponent<Rigidbody>();

        if(colRB!=null)
        {
            colRB.AddForceAtPosition(transform.forward * hitForce, transform.position);
        }

        //else {
            if (collision.gameObject.tag.Equals("Enemy"))
            {
                //GameObject b = Instantiate(blood, transform.position, transform.rotation);
                //b.GetComponent<ParticleSystem>().Play();
                //collision.gameObject.GetComponent<Guard>().health -= damage;
                //collision.gameObject.GetComponent<Rigidbody>().AddForce(-80 * collision.transform.forward);
                //Debug.Log("Hit!");
            }
            else if (collision.gameObject.tag.Equals("SmartEnemy"))
            {
                //GameObject b = Instantiate(blood, transform.position, transform.rotation);
                //b.GetComponent<ParticleSystem>().Play();
                collision.gameObject.GetComponent<SmartAI>().health -= damage;
                collision.gameObject.GetComponent<Rigidbody>().AddForce(-80 * collision.transform.forward);
                //GameObject b = Instantiate(other, transform.position, transform.rotation);
                //b.GetComponent<ParticleSystem>().Play();
            }
            else if (collision.gameObject.tag.Equals("DumbEnemy"))
            {
                //Debug.Log("Ye");
                collision.gameObject.GetComponent<DumbAi>().health -= damage;
                collision.gameObject.GetComponent<Rigidbody>().AddForce(-80 * collision.transform.forward);
            }
            else //if no enemy hit, draw bullet hole - now done in unistats
            {
                //Instantiate(WeaponSelection.decal0, transform.position, Quaternion.FromToRotation(Vector3.up, transform.forward));
            }
        //}
        Destroy(gameObject);
    }

    public void SetTravelDir(Vector3 dir)
    {
        travelDir = dir;
    }

}
